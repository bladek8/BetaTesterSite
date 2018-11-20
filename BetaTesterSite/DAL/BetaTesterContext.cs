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
        public virtual DbSet<PolicyRole> PolicyRole { get; set; }
        public virtual DbSet<Policy> Policy { get; set; }
        public virtual DbSet<Phase> Phase { get; set; }
        public virtual DbSet<PhasesIndexView> PhasesIndexView { get; set; }
        public virtual DbSet<UserPhaseFav> UserPhaseFav { get; set; }
        public virtual DbSet<PhaseRate> PhaseRate { get; set; }

        public BetaTesterContext(DbContextOptionsBuilder<BetaTesterContext> dbContextOptionsBuilder)
            : base(dbContextOptionsBuilder.Options)
        {
        }

        public BetaTesterContext(DbContextOptions<BetaTesterContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PhaseRate>().ToTable("PhaseRate");
            modelBuilder.Entity<UserPhaseFav>().ToTable("UserPhaseFav");
            modelBuilder.Entity<PhasesIndexView>().ToTable("PhasesIndexView");
            modelBuilder.Entity<Phase>(entity =>
            {

                //entity.Property(e => e.Fav).HasDefaultValueSql("((0))");
                entity.Property(e => e.Played).HasDefaultValueSql("((0))");
                entity.Property(e => e.Dies).HasDefaultValueSql("((0))");
                entity.Property(e => e.Completed).HasDefaultValueSql("((0))");
                //entity.Property(e => e.Rate).HasDefaultValueSql("((0))");
                entity.Property(e => e.Tested).HasDefaultValueSql("((0))");
            });
            modelBuilder.Entity<Policy>().ToTable("Policy");
            modelBuilder.Entity<PolicyRole>().ToTable("PolicyRole");
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