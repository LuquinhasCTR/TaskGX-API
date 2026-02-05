using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    [Table("Tarefas")]
    public class Tarefas
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        // ✅ no banco: Lista_id (NOT NULL)
        [Required]
        [Column("Lista_id")]
        public int ListaID { get; set; }

        // navigation
        public Listas? Lista { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("Titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Column("Descricao")]
        public string? Descricao { get; set; }

        [MaxLength(255)]
        [Column("Tags")]
        public string? Tags { get; set; }

        // ✅ no banco: Prioridade_id (nullable)
        [Column("Prioridade_id")]
        public int? PrioridadeID { get; set; }

        // navigation
        public Prioridades? Prioridade { get; set; }

        [Column("Concluida")]
        public bool Concluida { get; set; } = false;

        [Column("Arquivada")]
        public bool Arquivada { get; set; } = false;

        // no banco é DATE; no C# pode ser DateTime? (ok)
        [Column("DataVencimento")]
        public DateTime? DataVencimento { get; set; }

        [Column("DataCriacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Column("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;

        [Column("Ordem")]
        public int Ordem { get; set; } = 0;
    }
}

