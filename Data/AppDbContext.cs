using Microsoft.EntityFrameworkCore;
using Controle.Models;
 
namespace Controle.Data;

public class AppDbContext : DbContext{
    public AppDbContext(){}
    public AppDbContext(DbContextOptions<AppDbContext> options) :base(options){}

    public  DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Controle.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(e =>{
            // Campo ID da transação
            e.HasKey(t => t.Id);
            // Campo Descrição
            e.Property(t => t.Descricao).IsRequired().HasMaxLength(100);
            // Campo Valor
            e.Property(t => t.Valor).IsRequired().HasColumnType("decimal(10,2)");
            // Campo Tipo
            e.Property(t => t.Tipo).IsRequired().HasMaxLength(20);
            // Campo Categoria
            e.Property(t => t.Categoria).IsRequired().HasMaxLength(20);
            // Campo Data
            e.Property(t=>t.Data).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
