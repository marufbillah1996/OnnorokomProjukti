using AssignmentHub.Domain.Common;

namespace AssignmentHub.Domain.Entities;

public class Subject : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
}
