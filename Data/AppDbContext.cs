using Microsoft.EntityFrameworkCore;
using Controle.Models;

namespace Controle.Data;

public class AppDbContext : DbContext{
    public AppDbContext(){}
    public AppDbContext(DbContextOptions<AppDbContext> options) :base(options){}

    public  DbSet<Student> Students => Set<Student>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Controle.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(e =>{
            e.HasKey(s=>s.Id);
            e.Property(s=>s.Name).IsRequired().HasMaxLength(120);
            e.Property(s=>s.Email).IsRequired().HasMaxLength(100);
            e.HasIndex(s=>s.Email).IsUnique(); // email único
        });
    }
}