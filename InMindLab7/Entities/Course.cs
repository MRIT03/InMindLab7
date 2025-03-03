using System.ComponentModel.DataAnnotations;

namespace InMindLab7.Entities;

public class Course
{
    [Key]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    // Navigation property: one course can be offered in many classes
    public ICollection<Class> Classes { get; set; }
}