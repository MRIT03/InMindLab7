using System.ComponentModel.DataAnnotations;

namespace InMindLab7.Entities;

public class Class
{
    [Key]
    public int ClassId { get; set; }

    [Required]
    public int StudentId { get; set; }
    public Student Student { get; set; }

    [Required]
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; }

    [Required]
    public int CourseId { get; set; }
    public Course Course { get; set; }

     public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}