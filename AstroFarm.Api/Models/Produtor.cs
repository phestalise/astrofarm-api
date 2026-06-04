using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstroFarm.Api.Models
{
    [Table("PRODUTOR")]
    public class Produtor
    {
        [Key]
        [Column("ID_PRODUTOR")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("NOME")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column("CPF")]
        public string Cpf { get; set; } = string.Empty;

        [Column("EMAIL")]
        public string? Email { get; set; }

        [Column("SENHA")]
        public string? Senha { get; set; }

        [Column("TELEFONE")]
        public string? Telefone { get; set; }

        [Required]
        [Column("ESTADO")]
        public string Estado { get; set; } = string.Empty;

        [Required]
        [Column("CIDADE")]
        public string Cidade { get; set; } = string.Empty;

        [Column("DT_CADASTRO")]
        public DateTime DtCadastro { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public ICollection<Propriedade>? Propriedades { get; set; }
    }
}