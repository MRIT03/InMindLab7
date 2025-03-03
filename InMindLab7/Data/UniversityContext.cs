
using InMindLab7.Entities;
using Microsoft.EntityFrameworkCore;

namespace InMindLab7.Data;



public class UniversityContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Class> Classes { get; set; }
    
    public UniversityContext(DbContextOptions<UniversityContext> options)
        : base(options)
    {
    }
}
