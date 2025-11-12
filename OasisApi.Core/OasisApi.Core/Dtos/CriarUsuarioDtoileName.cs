using System.ComponentModel.DataAnnotations;

namespace OasisApi.Core.Dtos
{
    // Classe para receber dados no POST (Bean Validation)
    public class CriarUsuarioDto
    {
        [Required]
        public int EmpresaId { get; set; }
        [Required]
        public string NomeCompleto { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Cargo { get; set; }
        public string? FusoHorario { get; set; } = "America/Sao_Paulo";
    }
}