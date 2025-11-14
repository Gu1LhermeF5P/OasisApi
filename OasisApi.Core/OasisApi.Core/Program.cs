using Microsoft.AspNetCore.Mvc.ApiExplorer; 
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OasisApi.Core.Data;
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

// --- 2. Configuração do MongoDB ---
builder.Services.AddSingleton<MongoDbService>();

// --- 3. Configuração do Health Check ---
builder.Services.AddHealthChecks()
    .AddOracle(oracleConnection, name: "OracleDB-Check");

// --- Serviços Padrão da API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSwaggerGen(options =>
{
    // Definição do Documento V1
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Oasis API - v1",
        Description = "API de dados legada (sem fuso horário)"
    });

    // Definição do Documento V2
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "Oasis API - v2",
        Description = "API de dados com novas funcionalidades (inclui fuso horário)"
    });

    // Lógica para dizer ao Swagger qual endpoint pertence a qual versão.
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        
        if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

        // Encontra os controllers que estão no namespace "V2"
        var isV2 = methodInfo.DeclaringType
            .FullName.Contains("OasisApi.Core.Controllers.V2");

        
        if (docName == "v2")
        {
            return isV2;
        }

        
        return !isV2;
    });
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // --- 5. CORREÇÃO DO SWAGGER UI (PARA V1 e V2) ---
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Oasis API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Oasis API v2");
        options.RoutePrefix = "swagger";
    });
    // --- FIM DA CORREÇÃO ---
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

// Linha para os testes xUnit
public partial class Program { }