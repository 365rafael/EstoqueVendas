using EstoqueVendas.Models;
using Microsoft.EntityFrameworkCore;

namespace EstoqueVendas.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<EntradaProduto> EntradaProduto { get; set; }
        public DbSet<SaidaProduto> SaidaProduto { get; set; }
        public DbSet<Produto> Produto { get; set; }
        public DbSet<Fornecedor> Fornecedor { get; set; }
    }
}
