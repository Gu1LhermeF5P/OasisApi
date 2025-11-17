using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OasisApi.Core.Data;
using OasisApi.Core.Models;
using OasisApi.Core.Services;
using Moq;

namespace OasisApi.Core.Tests
{
    // A classe da 'Factory' deve ser 'public' para ser acessível pelo xUnit
    public class OasisApiApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remove o DbContext do Oracle
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // 2. Adiciona um Banco em Memória
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // 3. Remove o MongoDbService real
                var mongoServiceDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(MongoDbService));
                if (mongoServiceDescriptor != null)
                {
                    services.Remove(mongoServiceDescriptor);
                }

                // 4. Cria uma configuração falsa (mas não nula) para o Mock do MongoDB
                var fakeConfiguration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConnectionStrings:MongoDbConnection"] = "mongodb://fake-test-server",
                        ["MongoDbSettings:DatabaseName"] = "fake-db",
                        ["MongoDbSettings:CollectionName"] = "fake-collection"
                    })
                    .Build();

                // 5. Adiciona o Mock do MongoDbService, passando a configuração falsa
                var mockMongoDbService = new Mock<MongoDbService>(fakeConfiguration) { CallBase = true };
                services.AddSingleton(mockMongoDbService.Object);


                // 6. Semeia (Seed) o banco em memória
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();

                    // Garante que o banco seja semeado apenas uma vez
                    if (!db.Usuarios.Any())
                    {
                        SeedDatabase(db);
                    }
                }
            });
        }

        // Método para popular o banco em memória com dados falsos
        private void SeedDatabase(AppDbContext db)
        {
            var empresa = new Empresa { EmpresaId = 1, NomeEmpresa = "Empresa Teste" };
            db.Empresas.Add(empresa);

            // Adiciona os dados de teste, incluindo o campo 'FusoHorario'
            db.Usuarios.Add(new Usuario
            {
                UsuarioId = 1,
                NomeCompleto = "Usuario Teste 1",
                Email = "teste1@empresa.com",
                Cargo = "Tester",
                EmpresaId = 1,
                FusoHorario = "America/Sao_Paulo"
            });
            db.Usuarios.Add(new Usuario
            {
                UsuarioId = 2,
                NomeCompleto = "Usuario Teste 2",
                Email = "teste2@empresa.com",
                Cargo = "Tester",
                EmpresaId = 1,
                FusoHorario = "America/Sao_Paulo"
            });

            db.SaveChanges();
        }
    }
}