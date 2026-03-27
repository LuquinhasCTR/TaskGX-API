using Microsoft.EntityFrameworkCore;
using TaskGX.API.Models;
using TaskGX.Data;

namespace TaskGX.API.Repositories
{
    public class UsuarioRepository
    {
        private readonly TaskGXContext _db;

        public UsuarioRepository(TaskGXContext db)
        {
            _db = db;
        }

        public Task<Usuarios?> ObterPorEmailAsync(string email)
        {
            return _db.Usuarios
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Select(u => new Usuarios
                {
                    ID = u.ID,
                    Nome = u.Nome,
                    Email = u.Email,
                    SenhaHash = u.SenhaHash,
                    Avatar = u.Avatar,
                    Ativo = u.Ativo,
                    EmailVerificado = u.EmailVerificado,
                    CodigoVerificacao = u.CodigoVerificacao,
                    CodigoVerificacaoExpiracao = u.CodigoVerificacaoExpiracao,
                    CriadoEm = u.CriadoEm,
                    DataAtualizacao = u.DataAtualizacao
                })
                .FirstOrDefaultAsync();
        }

        public Task<Usuarios?> ObterPorIdAsync(int usuarioId)
        {
            return _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.ID == usuarioId);
        }

        public Task<Usuarios?> ObterParaEdicaoPorIdAsync(int usuarioId)
        {
            return _db.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
        }

        public Task<bool> ExisteEmailAsync(string email)
        {
            return _db.Usuarios.AnyAsync(u => u.Email == email);
        }

        public Task<bool> ExisteEmailEmOutroUsuarioAsync(string email, int usuarioId)
        {
            return _db.Usuarios.AnyAsync(u => u.Email == email && u.ID != usuarioId);
        }

        public async Task InserirAsync(Usuarios usuario)
        {
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
        }

        public async Task AtualizarDadosAsync(int usuarioId, string nome, string email)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (usuario == null) return;

            usuario.Nome = nome;
            usuario.Email = email;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task AtualizarSenhaAsync(int usuarioId, string senhaHash)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (usuario == null) return;

            usuario.SenhaHash = senhaHash; // mapeado para coluna "Senha"
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task AtualizarVerificacaoEmailAsync(
            int usuarioId,
            bool emailVerificado,
            bool ativo,
            string? codigoVerificacao,
            DateTime? expiracao)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (usuario == null) return;

            usuario.EmailVerificado = emailVerificado;
            usuario.Ativo = ativo;
            usuario.CodigoVerificacao = codigoVerificacao;
            usuario.CodigoVerificacaoExpiracao = expiracao;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task AtualizarSolicitacaoAlteracaoEmailAsync(
            int usuarioId,
            string novoEmail,
            string codigoVerificacao,
            DateTime expiracao)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (usuario == null) return;

            usuario.EmailPendente = novoEmail;
            usuario.CodigoVerificacao = codigoVerificacao;
            usuario.CodigoVerificacaoExpiracao = expiracao;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task ConfirmarAlteracaoEmailAsync(int usuarioId, string novoEmail)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ID == usuarioId);
            if (usuario == null) return;

            usuario.Email = novoEmail;
            usuario.EmailPendente = null;
            usuario.EmailVerificado = true;
            usuario.CodigoVerificacao = null;
            usuario.CodigoVerificacaoExpiracao = null;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
