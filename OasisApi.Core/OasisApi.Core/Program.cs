using Microsoft.EntityFrameworkCore;
using OasisApi.Core.Data;     // (Vamos criar esta pasta)
using OasisApi.Core.Services; // (Vamos criar esta pasta)

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuração do Oracle (EF Core) ---
var oracleConnection = builder.Configuration.GetConnectionString("OracleDbConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(oracleConnection, opt => opt.UseOracleSQLCompatibility("11"))
);

// --- 2. Configuração do MongoDB (Requisito 5 do BD) ---
builder.Services.AddSingleton<MongoDbService>();

// --- 3. Configuração do Health Check (Requisito .NET) ---
builder.Services.AddHealthChecks()
    .AddOracle(oracleConnection, name: "OracleDB-Check");

// --- Serviços Padrão da API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline de execução
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health"); // Endpoint do Health Check
app.Run();