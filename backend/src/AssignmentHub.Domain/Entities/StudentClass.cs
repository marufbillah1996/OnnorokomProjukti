namespace AssignmentHub.Domain.Entities;

/// <summary>
/// Join entity enrolling a Student into a Class. Composite primary key (StudentId, ClassId)
/// configured in the EF Core configuration — no surrogate Id needed for a pure enrollment fact.
/// </summary>
public class StudentClass
{
    public Guid StudentId { get; set; }
    public User? Student { get; set; }

    public Guid ClassId { get; set; }
    public Class? Class { get; set; }

    public DateTime EnrolledAt { get; set; }
}
