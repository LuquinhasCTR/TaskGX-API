using System;
using System.ComponentModel.DataAnnotations;

namespace TaskGX.API.Models
{
    /// <summary>
    /// Usuário do TaskGX
    /// </summary>
    public class Usuarios
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string SenhaHash { get; set; }  // Nome alterado para deixar claro que é hash

        public string Avatar { get; set; }
        public bool Ativo { get; set; } = true;
        public bool EmailVerificado { get; set; } = false;
        public string CodigoVerificacao { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? CodigoVerificacaoExpiracao { get; set; }

        public Usuarios() { }
    }
}
