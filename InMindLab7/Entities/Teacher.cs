using System.ComponentModel.DataAnnotations;

namespace InMindLab7.Entities;

public class Teacher
{
    [Key]
    public int TeacherId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    // Navigation property: one teacher can have many classes
    public ICollection<Class> Classes { get; set; }
}