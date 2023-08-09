﻿using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreMySQL.DBContexts
{
    public class MyDBContext : DbContext
    {
        public DbSet<MasterAuth> MasterAuth { get; set; }

        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
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
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.password).HasColumnType("varchar(100)").IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.created_by).HasColumnType("varchar(100)").IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.created_date).HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.modified_by).HasColumnType("varchar(100)").IsRequired(false);
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.modified_date).HasColumnType("datetime").IsRequired(false);
            modelBuilder.Entity<MasterAuth>().Property(ma => ma.is_active).HasColumnType("tinyint(1)").IsRequired();
        }
    }
}