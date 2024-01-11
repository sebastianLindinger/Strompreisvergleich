using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace StromDbLib;
public class StromDbContext : DbContext
{
    public StromDbContext()
    {
    }

    public StromDbContext(DbContextOptions<StromDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Strompreis> Strompreis { get; set; }
    public virtual DbSet<Stromverbrauch> Stromverbrauch { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=Strom.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
