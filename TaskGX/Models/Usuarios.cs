using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskGX.API.Models
{
    [Table("Usuarios")]
    public class Usuarios
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        // ✅ no banco é "Senha"
        [Required]
        [Column("Senha")]
        public string SenhaHash { get; set; } = string.Empty;

        [Column("Avatar")]
        public string? Avatar { get; set; }

        [Column("Ativo")]
        public bool Ativo { get; set; } = true;

        [Column("EmailVerificado")]
        public bool EmailVerificado { get; set; } = false;

        [Column("CodigoVerificacao")]
        public string? CodigoVerificacao { get; set; }

        [Column("CodigoVerificacaoExpiracao")]
        public DateTime? CodigoVerificacaoExpiracao { get; set; }

        // ✅ no banco é "Criado_em"
        [Column("Criado_em")]
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        // ✅ no banco é "DataAtualizacao"
        [Column("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }
}
