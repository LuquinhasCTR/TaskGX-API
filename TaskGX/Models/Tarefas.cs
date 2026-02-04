using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    public class Tarefas
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Lista")]
        public int? ListaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [ForeignKey("Prioridade")]
        public int? PrioridadeId { get; set; }

        public string? Tags { get; set; }

        public bool Concluida { get; set; } = false;

        public bool Arquivada { get; set; } = false;

        public DateTime? DataVencimento { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Navigation properties
        public Listas? Lista { get; set; }
        public Prioridades? Prioridade { get; set; }
    }
}
