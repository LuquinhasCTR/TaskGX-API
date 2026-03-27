using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    [Table("Tarefas")]
    public class Tarefa
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [Column("Lista_id")]
        public int ListaID { get; set; }

        public Lista? Lista { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column("Descricao")]
        public string? Descricao { get; set; }

        [MaxLength(255)]
        [Column("Tags")]
        public string? Tags { get; set; }

        [Column("Prioridade_id")]
        public int? PrioridadeID { get; set; }

        public Prioridade? Prioridade { get; set; }

        [Column("Concluida")]
        public bool Concluida { get; set; }

        [Column("Arquivada")]
        public bool Arquivada { get; set; }

        [Column("DataVencimento")]
        public DateTime? DataVencimento { get; set; }

        [Column("DataCriacao")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [Column("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        [Column("Ordem")]
        public int Ordem { get; set; }
    }
}
