using Microsoft.EntityFrameworkCore;
using MockMindAI.Api.Models;

namespace MockMindAI.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<InterviewAttempt> InterviewAttempts => Set<InterviewAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasIndex(student => student.Email)
            .IsUnique();

        modelBuilder.Entity<Student>()
            .Property(student => student.Email)
            .HasMaxLength(200);

        modelBuilder.Entity<InterviewAttempt>()
            .HasOne(attempt => attempt.Student)
            .WithMany(student => student.InterviewAttempts)
            .HasForeignKey(attempt => attempt.StudentId);
    }
}
