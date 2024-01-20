using ImportDataToDB.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Repository
{
    public class MyDbContext : DbContext
    {
        public DbSet<SchoolYear> SchoolYears { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=StudentResultDB;User Id=sa;Password=12345;Integrated Security=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define relationships in the model
            modelBuilder.Entity<Student>()
                .HasOne(s => s.SchoolYear)
                .WithMany(sy => sy.Students)
                .HasForeignKey(s => s.SchoolYearId);

            modelBuilder.Entity<Score>()
                .HasOne(sc => sc.Student)
                .WithMany(st => st.Scores)
                .HasForeignKey(sc => sc.StudentId);

            modelBuilder.Entity<Score>()
                .HasOne(sc => sc.Subject)
                .WithMany(sub => sub.Scores)
                .HasForeignKey(sc => sc.SubjectId);
        }
    }
}
