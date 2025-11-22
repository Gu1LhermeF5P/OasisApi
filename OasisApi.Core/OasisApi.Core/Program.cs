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



builder.Services.AddHealthChecks()
    .AddOracle(oracleConnection, name: "OracleDB-Check")
    // Adiciona o Health Check para MongoDB
    .AddMongoDb(builder.Configuration.GetConnectionString("MongoDbConnection"), name: "MongoDB-Check");



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



builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Oasis API - v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "Oasis API - v2" });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
        var isV2 = methodInfo.DeclaringType.FullName.Contains("OasisApi.Core.Controllers.V2");
        if (docName == "v2") return isV2;
        return !isV2;
    });

    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta forma: Bearer {seu token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    
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


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Oasis API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Oasis API v2");
    options.RoutePrefix = string.Empty;
});


app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();





app.MapHealthChecks("/health/details", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false });


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
.RequireAuthorization() /
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));



app.MapPost("/v1/sentences", (Sentence item, InMemoryRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(item.Text)) return Results.BadRequest("O campo 'Text' é obrigatório.");
    var newSentence = new Sentence { Text = item.Text, IsPositive = item.IsPositive };
    repository.Add(newSentence);
    return Results.Created($"/v1/sentences/{newSentence.Id}", newSentence);
})
.WithName("CreateSentenceV1")
.RequireAuthorization() 
.WithApiVersionSet(app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build())
.MapToApiVersion(new ApiVersion(1, 0));




app.Run();

// Linha para os testes xUnit
public partial class Program { }