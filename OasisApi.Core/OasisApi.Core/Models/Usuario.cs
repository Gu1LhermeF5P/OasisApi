using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OasisApi.Core.Models
{
    [Table("TB_USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("empresa_id")]
        public int EmpresaId { get; set; }

        [Column("nome_completo")]
        public string NomeCompleto { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("cargo")]
        public string Cargo { get; set; }

       
        [Column("fuso_horario")]
        public string FusoHorario { get; set; }
        

        [ForeignKey("EmpresaId")]
        public virtual Empresa Empresa { get; set; }
    }
}