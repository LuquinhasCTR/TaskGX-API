using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    [Table("Prioridades")]
    public class Prioridade
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;
    }
}
