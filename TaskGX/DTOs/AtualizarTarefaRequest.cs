using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.DTOs
{
    public class AtualizarTarefaRequest
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public int ListaID { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [StringLength(255)]
        public string? Tags { get; set; }

        public int? PrioridadeID { get; set; }
        public bool Concluida { get; set; }
        public bool Arquivada { get; set; }
        public DateTime? DataVencimento { get; set; }
        public int Ordem { get; set; }
    }
}
