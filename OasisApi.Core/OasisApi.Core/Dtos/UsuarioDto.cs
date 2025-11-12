using OasisApi.Core.Models;

namespace OasisApi.Core.Dtos
{
    // Classe para enviar dados no GET
    public class UsuarioDto
    {
        public int UsuarioId { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();

        // Converte do Modelo do BD para o DTO
        public static UsuarioDto FromUsuario(Usuario usuario)
        {
            return new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                NomeCompleto = usuario.NomeCompleto,
                Email = usuario.Email,
                Cargo = usuario.Cargo
            };
        }
    }
}