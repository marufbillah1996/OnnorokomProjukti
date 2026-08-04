using AssignmentHub.Domain.Common;
using AssignmentHub.Domain.Enums;

namespace AssignmentHub.Domain.Entities;

public class User : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Populated when Role == Teacher: the ClassSubject rows this user teaches.
    public ICollection<ClassSubject> TeachingAssignments { get; set; } = new List<ClassSubject>();

    // Populated when Role == Student: the classes this user is enrolled in.
    public ICollection<StudentClass> Enrollments { get; set; } = new List<StudentClass>();

    // Populated when Role == Teacher: assignments this user authored.
    public ICollection<Assignment> CreatedAssignments { get; set; } = new List<Assignment>();

    // Populated when Role == Student: submissions this user has made.
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
