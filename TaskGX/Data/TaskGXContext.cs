using Microsoft.EntityFrameworkCore;
using TaskGX.API.Models;

namespace TaskGX.API.Data
{
    public class TaskGXContext : DbContext
    {
        public TaskGXContext(DbContextOptions<TaskGXContext> options)
            : base(options) { }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Listas> Listas { get; set; }
        public DbSet<Tarefas> Tarefas { get; set; }
        public DbSet<Prioridades> Prioridades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aqui você pode configurar relacionamentos, nomes de tabelas, seeds etc.
        }
    }
}
