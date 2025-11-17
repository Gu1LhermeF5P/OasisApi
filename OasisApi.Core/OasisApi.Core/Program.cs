using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
    .AddOracle(oracleConnection, name: "OracleDB-Check"); // <-- O nome "OracleDB-Check" é importante

// --- Serviços Padrão da API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- 4. Configuração do Swagger (V1 e V2) ---
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
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Oasis API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Oasis API v2");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// --- 5.MAPEAMENTO DOS HEALTH CHECKS  ---
app.MapHealthChecks("/health");


app.MapHealthChecks("/health/details", new HealthCheckOptions
{
    
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});



app.Run();

// Linha para os testes xUnit
public partial class Program { }