using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    public class Listas
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Cor { get; set; }

        public bool Favorita { get; set; } = false;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Relação opcional com usuário (navigation property)
        public Usuarios? Usuario { get; set; }
    }
}
