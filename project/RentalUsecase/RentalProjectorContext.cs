using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace RentalUsecase
{
    public class RentalProjectorContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                // .UseLazyLoadingProxies()
#if DEBUG
                .UseMySql("Server=localhost;Port=3306;Username=root;Password=root;Database=snapshots");
#else
                .UseMySql("Server=rental_projection;Port=3306;Username=root;Password=root;Database=snapshots");
#endif
        }
        public DbSet<本Entity> 本一覧 { get; set; }
        public DbSet<書籍Entity> 書籍一覧 { get; set; }
        public DbSet<利用者Entity> 利用者一覧 { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.ApplyConfiguration(new 利用者EntityConfiguration());
        }
    }
}
