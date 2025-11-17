using Microsoft.EntityFrameworkCore;
using OasisApi.Core.Models;

namespace OasisApi.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           

            modelBuilder.Entity<Empresa>(e =>
            {
                // Mapeia a tabela para "TB_EMPRESA"
                e.ToTable("TB_EMPRESA");

                // Mapeia cada propriedade para sua coluna (MAIÚSCULA)
                e.Property(p => p.EmpresaId).HasColumnName("EMPRESA_ID");
                e.Property(p => p.NomeEmpresa).HasColumnName("NOME_EMPRESA");
                e.Property(p => p.Cnpj).HasColumnName("CNPJ");
            });

            modelBuilder.Entity<Usuario>(e =>
            {
                // Mapeia a tabela para "TB_USUARIO"
                e.ToTable("TB_USUARIO");

                // Mapeia cada propriedade para sua coluna (MAIÚSCULA)
                e.Property(p => p.UsuarioId).HasColumnName("USUARIO_ID");
                e.Property(p => p.EmpresaId).HasColumnName("EMPRESA_ID");
                e.Property(p => p.NomeCompleto).HasColumnName("NOME_COMPLETO"); 
                e.Property(p => p.Email).HasColumnName("EMAIL");
                e.Property(p => p.Cargo).HasColumnName("CARGO");
                e.Property(p => p.FusoHorario).HasColumnName("FUSO_HORARIO");

                // Define o relacionamento
                e.HasOne(u => u.Empresa)
                 .WithMany(p => p.Usuarios)
                 .HasForeignKey(u => u.EmpresaId);
            });
            
        }
    }
}