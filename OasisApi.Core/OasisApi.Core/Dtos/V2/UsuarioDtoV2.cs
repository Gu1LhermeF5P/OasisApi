using OasisApi.Core.Models;
using OasisApi.Core.Dtos; 

namespace OasisApi.Core.Dtos.V2
{
    // Este é o DTO da V2, que agora inclui o Fuso Horário
    public class UsuarioDtoV2
    {
        public int UsuarioId { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }

        
        public string FusoHorario { get; set; }

        public List<LinkDto> Links { get; set; } = new List<LinkDto>();

        // Helper para converter do Model do EF para o DTO V2
        public static UsuarioDtoV2 FromUsuario(Usuario usuario)
        {
            return new UsuarioDtoV2
            {
                UsuarioId = usuario.UsuarioId,
                NomeCompleto = usuario.NomeCompleto,
                Email = usuario.Email,
                Cargo = usuario.Cargo,
                FusoHorario = usuario.FusoHorario
            };
        }
    }
}