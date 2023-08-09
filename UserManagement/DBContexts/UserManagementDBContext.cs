using UserManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.DBContexts
{
    public class UserManagementDBContext : DbContext
    {
        public DbSet<MasterUser> MasterUser { get; set; }

        public UserManagementDBContext(DbContextOptions<UserManagementDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Use Fluent API to configure  

            // Map entities to tables  
            modelBuilder.Entity<MasterUser>().ToTable("master_user");

            // Configure Primary Keys  
            modelBuilder.Entity<MasterUser>().HasKey(ma => ma.id).HasName("PRIMARY");

            // Configure columns  
            modelBuilder.Entity<MasterUser>().Property(ma => ma.id).HasColumnType("bigint(20)").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<MasterUser>().Property(ma => ma.fullname).HasColumnType("varchar(100)").IsRequired();
            modelBuilder.Entity<MasterUser>().Property(ma => ma.created_by).HasColumnType("varchar(100)").IsRequired();
            modelBuilder.Entity<MasterUser>().Property(ma => ma.created_date).HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<MasterUser>().Property(ma => ma.modified_by).HasColumnType("varchar(100)").IsRequired(false);
            modelBuilder.Entity<MasterUser>().Property(ma => ma.modified_date).HasColumnType("datetime").IsRequired(false);
            modelBuilder.Entity<MasterUser>().Property(ma => ma.is_active).HasColumnType("tinyint(1)").IsRequired();
        }
    }
}