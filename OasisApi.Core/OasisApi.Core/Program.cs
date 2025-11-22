using Asp.Versioning;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.OpenApi.Models;
using OasisApi.Core.Data;
using OasisApi.Core.Models;
using OasisApi.Core.Services;
using Oracle.ManagedDataAccess.Client;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuração do Oracle (EF Core) ---
var oracleConnection = builder.Configuration.GetConnectionString("OracleDbConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnection, opt =>
    {
        opt.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
    })
);

// --- 2. Configuração do MongoDB e Repositório CRUD ---
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<InMemoryRepository>();

// --- Configuração do ML.NET (25 pts) ---
var mlContext = new MLContext();
// Dados de Treinamento Simulado (Definidos em SentimentData.cs)
var sampleData = new List<SentimentData>
{
    new SentimentData { SentimentText = "Adorei a funcionalidade, é fantástica!", Sentiment = true },
    new SentimentData { SentimentText = "O sistema está muito lento e cheio de bugs.", Sentiment = false },
    new SentimentData { SentimentText = "Isso me fez rir muito, ótimo trabalho!", Sentiment = true },
    new SentimentData { SentimentText = "Absolutamente o pior dia de todos.", Sentiment = false },
};

IDataView dataView = mlContext.Data.LoadFromEnumerable(sampleData);
var pipeline = mlContext.Transforms.Text.FeaturizeText(
    outputColumnName: "Features",
    inputColumnName: nameof(SentimentData.SentimentText))
    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
        labelColumnName: nameof(SentimentData.Sentiment),
        featureColumnName: "Features"));

var trainedModel = pipeline.Fit(dataView);
var predEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(trainedModel);
builder.Services.AddSingleton(predEngine);

// --- Configuração do Health Check (10 pts) ---
builder.Services.AddHealthChecks()
    .AddOracle(oracleConnection, name: "OracleDB-Check")
    // Adiciona o Health Check para MongoDB
    .AddMongoDb(builder.Configuration.GetConnectionString("MongoDbConnection"), name: "MongoDB-Check");


// --- Configuração do Versionamento (10 pts) ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Versão via URL: /v1/recurso
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


// --- Serviços Padrão da API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Configuração do Swagger com Segurança (25 pts) ---
builder.Services.AddSwaggerGen(options =>
{
    // *** CORREÇÃO: MUDANÇA DE NOME NO SWAGGER ***
    options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "API Classificação Humor - v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "API Classificação Humor - v2" });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
        var isV2 = methodInfo.DeclaringType.FullName.Contains("OasisApi.Core.Controllers.V2");
        if (docName == "v2") return isV2;
        return !isV2;
    });

    // Configuração de Segurança JWT (25 pts)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta forma: Bearer {seu token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    // Adiciona o requisito de segurança globalmente
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// --- Middleware ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // *** CORREÇÃO: MUDANÇA DE NOME NO SWAGGER UI ***
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Classificação Humor v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API Classificação Humor v2");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ----------------------------------------------------------------------------------
// --- Mapeamento dos Endpoints ---
// ----------------------------------------------------------------------------------

// Mapeamento dos Health Checks (10 pts)
app.MapHealthChecks("/health/details", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });


// Mapeamento do Endpoint ML.NET (25 pts) com Versionamento
app.MapPost("/v1/ml/classificar-humor", (SentimentData input, PredictionEngine<SentimentData, SentimentPrediction> engine) =>
{
    var prediction = engine.Predict(input);
    string resultado = prediction.Prediction ? "Positivo" : "Negativo";
    return Results.Ok(new
    {
        Texto = input.SentimentText,
        ResultadoClassificacao = resultado,
        ProbabilidadePositivo = prediction.Probability,
    });
})
.WithName("ClassificarHumor")
.WithTags("ClassificacaoHumorML") // Adiciona tag para agrupamento
.RequireAuthorization()
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));


// --- CRUD COMPLETO de Sentenças (/v1/sentences) ---
// Agrupa os endpoints CRUD com uma tag e exige autorização
var sentenceApi = app.MapGroup("/v1/sentences")
                     .WithTags("SentencesCRUD")
                     .RequireAuthorization();

// 1. CREATE (POST /v1/sentences)
sentenceApi.MapPost("/", (Sentence item, InMemoryRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(item.Text)) return Results.BadRequest("O campo 'Text' é obrigatório.");
    var newSentence = new Sentence { Text = item.Text, IsPositive = item.IsPositive };
    repository.Add(newSentence);
    return Results.Created($"/v1/sentences/{newSentence.Id}", newSentence);
})
.WithName("CreateSentenceV1")
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));

// 2. READ ALL (GET /v1/sentences)
sentenceApi.MapGet("/", (InMemoryRepository repository) => Results.Ok(repository.GetAll()))
.WithName("GetAllSentencesV1")
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));

// 3. READ BY ID (GET /v1/sentences/{id})
sentenceApi.MapGet("/{id:guid}", (Guid id, InMemoryRepository repository) =>
{
    var item = repository.GetById(id);
    return item != null ? Results.Ok(item) : Results.NotFound();
})
.WithName("GetSentenceByIdV1")
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));

// 4. UPDATE (PUT /v1/sentences/{id})
sentenceApi.MapPut("/{id:guid}", (Guid id, Sentence item, InMemoryRepository repository) =>
{
    var updated = repository.Update(id, item);
    return updated != null ? Results.Ok(updated) : Results.NotFound();
})
.WithName("UpdateSentenceV1")
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));

// 5. DELETE (DELETE /v1/sentences/{id})
sentenceApi.MapDelete("/{id:guid}", (Guid id, InMemoryRepository repository) =>
{
    bool deleted = repository.Delete(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteSentenceV1")
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));


app.Run();

// Linha para os testes xUnit
public partial class Program { }