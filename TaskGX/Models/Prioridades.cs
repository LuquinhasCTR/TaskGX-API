using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.Models
{
    public class Prioridades
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nome { get; set; } = string.Empty;
    }
}
