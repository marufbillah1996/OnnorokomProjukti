using AssignmentHub.Domain.Common;

namespace AssignmentHub.Domain.Entities;

public class Class : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
    public ICollection<StudentClass> Enrollments { get; set; } = new List<StudentClass>();
}
