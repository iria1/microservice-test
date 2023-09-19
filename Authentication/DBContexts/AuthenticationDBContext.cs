using Microsoft.EntityFrameworkCore;
using System;

namespace Authentication.DBContexts
{
    public class AuthenticationDBContext : DbContext
    {
        public DbSet<MasterAuth> MasterAuth { get; set; }
        public DbSet<MasterRefreshToken> MasterRefreshToken { get; set; }

        public AuthenticationDBContext(DbContextOptions<AuthenticationDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Use Fluent API to configure  

            // Map entities to tables  
            modelBuilder.Entity<MasterAuth>().ToTable("master_auth");

            // Configure Primary Keys  
            modelBuilder.Entity<MasterAuth>().HasKey(ma => ma.id).HasName("PRIMARY");

            // Configure columns  
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.id).HasColumnType("bigint(20)").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.user_id).HasColumnType("bigint(20)").IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.username).HasColumnType("varchar(100)").IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.password).HasColumnType("varchar(64)").IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.created_by).HasColumnType("varchar(100)").HasDefaultValue("system").IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.created_date).HasColumnType("datetime").HasDefaultValue(DateTime.Now).IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.modified_by).HasColumnType("varchar(100)").IsRequired(false);
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.modified_date).HasColumnType("datetime").IsRequired(false);
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.is_active).HasColumnType("tinyint(1)").HasDefaultValue(1).IsRequired();

            modelBuilder.Entity<MasterRefreshToken>().ToTable("master_refresh_token");

            modelBuilder.Entity<MasterRefreshToken>().HasKey(ma => ma.id).HasName("PRIMARY");

            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.id).HasColumnType("bigint(20)").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.user_id).HasColumnType("bigint(20)").IsRequired();
            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.token).HasColumnType("text").IsRequired();
            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.created_by).HasColumnType("varchar(100)").HasDefaultValue("system").IsRequired();
            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.created_date).HasColumnType("datetime").HasDefaultValue(DateTime.Now).IsRequired();
            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.modified_by).HasColumnType("varchar(100)").IsRequired(false);
            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.modified_date).HasColumnType("datetime").IsRequired(false);
            modelBuilder.Entity<MasterRefreshToken>().Property(ma => ma.is_active).HasColumnType("tinyint(1)").HasDefaultValue(1).IsRequired();
        }
    }

    public class MasterAuth
    {
        public long id { get; set; }

        public long user_id { get; set; }

        public string username { get; set; }

        public string password { get; set; }

        public string created_by { get; set; }

        public DateTime created_date { get; set; }

        public string modified_by { get; set; }

        public DateTime? modified_date { get; set; }

        public bool is_active { get; set; }
    }

    public class MasterRefreshToken
    {
        public long id { get; set; }

        public long user_id { get; set; }

        public string token { get; set; }

        public string created_by { get; set; }

        public DateTime created_date { get; set; }

        public string modified_by { get; set; }

        public DateTime? modified_date { get; set; }

        public bool is_active { get; set; }
    }
}