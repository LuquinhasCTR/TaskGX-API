using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    [Table("Listas")]
    public class Listas
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [Column("Usuario_id")]
        public int UsuarioID { get; set; }

        public Usuarios? Usuario { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("Cor")]
        public string? Cor { get; set; }

        [Column("Favorita")]
        public bool Favorita { get; set; }

        [Column("Ordem")]
        public int Ordem { get; set; }

        [Column("DataCriacao")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
