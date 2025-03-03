using System.ComponentModel.DataAnnotations;

namespace InMindLab7.Entities;

public class Student
{
    [Key]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }

    public string Email { get; set; }

    public string ProfilePictureBlobName { get; set; }
    // Navigation property: one student can enroll in many classes
    public ICollection<Class> Classes { get; set; }
}
