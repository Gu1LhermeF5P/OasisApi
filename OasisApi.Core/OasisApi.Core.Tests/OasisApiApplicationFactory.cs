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

                // 4. Adiciona um "Mock" (simulação) do MongoDbService
                services.AddSingleton(new Mock<MongoDbService>(Mock.Of<IConfiguration>()).Object);

                // 5. Semeia (Seed) o banco em memória
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                    SeedDatabase(db);
                }
            });
        }

        private void SeedDatabase(AppDbContext db)
        {
            var empresa = new Empresa { EmpresaId = 1, NomeEmpresa = "Empresa Teste" };
            db.Empresas.Add(empresa);
            db.Usuarios.Add(new Usuario { UsuarioId = 1, NomeCompleto = "Usuario Teste 1", Email = "teste1@empresa.com", Cargo = "Tester", EmpresaId = 1 });
            db.Usuarios.Add(new Usuario { UsuarioId = 2, NomeCompleto = "Usuario Teste 2", Email = "teste2@empresa.com", Cargo = "Tester", EmpresaId = 1 });
            db.SaveChanges();
        }
    }
}