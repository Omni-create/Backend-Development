using Microsoft.EntityFrameworkCore;
using System;

namespace Backend_Dev.DataAccess.Database
{
    public class DBConnect : DbContext
    {
        public DBConnect(DbContextOptions<DBConnect> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost;Database=BackendDevDb;Trusted_Connection=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add your entity configurations here
        }
    }
}