using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OasisApi.Core.Models
{
    [Table("TB_EMPRESA")]
    public class Empresa
    {
        [Key]
        [Column("empresa_id")]
        public int EmpresaId { get; set; }

        [Column("nome_empresa")]
        public string NomeEmpresa { get; set; }

      
        [Column("cnpj")]
        public string Cnpj { get; set; }
      

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}