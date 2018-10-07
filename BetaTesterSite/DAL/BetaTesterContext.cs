using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetaTesterSite.DAL
{
    public class BetaTesterContext : DbContext
    {
        public virtual DbSet<Identity.User> User { get; set; }
        public virtual DbSet<Identity.Role> Role { get; set; }
        public virtual DbSet<Identity.AspNetUserRoles> AspNetUserRoles { get; set; }

        public BetaTesterContext(DbContextOptionsBuilder<BetaTesterContext> dbContextOptionsBuilder)
            : base(dbContextOptionsBuilder.Options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "BetaTester.db" };
        //    var connectionString = connectionStringBuilder.ToString();
        //    var connection = new SqliteConnection(connectionString);

        //    optionsBuilder.UseSqlite(connection);
        //}

        public BetaTesterContext(DbContextOptions<BetaTesterContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<SimpleTable>().ToTable("SimpleTable");
            modelBuilder.Entity<Identity.Role>().ToTable("Role");

            modelBuilder.Entity<Identity.AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
            });

            modelBuilder.Entity<Identity.User>().ToTable("User");
            modelBuilder.Entity<Identity.User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("UserId");
                entity.Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(255);
                entity.Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(255);
            });
        }
    }
}