using System.ComponentModel.DataAnnotations;

namespace OasisApi.Core.Dtos
{
    // DTO para o endpoint PUT (Atualizar)
    public class UpdateUsuarioDto
    {
        [Required]
        public string NomeCompleto { get; set; }

        [Required]
        public string Cargo { get; set; }

        public string? FusoHorario { get; set; } = "America/Sao_Paulo";
    }
}