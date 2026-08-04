using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent startup seed routine. Populates a handful of demo users, academic
/// structure, assignments and submissions so every dashboard/role has data to show
/// immediately after a fresh migration. Safe to call on every startup — it is a no-op
/// once any user row exists.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // ----- Users -----
        var admin = new User
        {
            Name = "System Admin",
            Email = "admin@assignmenthub.local",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true
        };

        var teacher1 = new User
        {
            Name = "Alice Rahman",
            Email = "teacher1@assignmenthub.local",
            PasswordHash = passwordHasher.Hash("Teacher@123"),
            Role = UserRole.Teacher,
            IsActive = true
        };

        var teacher2 = new User
        {
            Name = "Bilal Hasan",
            Email = "teacher2@assignmenthub.local",
            PasswordHash = passwordHasher.Hash("Teacher@123"),
            Role = UserRole.Teacher,
            IsActive = true
        };

        var student1 = new User
        {
            Name = "Chandni Akter",
            Email = "student1@assignmenthub.local",
            PasswordHash = passwordHasher.Hash("Student@123"),
            Role = UserRole.Student,
            IsActive = true
        };

        var student2 = new User
        {
            Name = "Dipa Chowdhury",
            Email = "student2@assignmenthub.local",
            PasswordHash = passwordHasher.Hash("Student@123"),
            Role = UserRole.Student,
            IsActive = true
        };

        var student3 = new User
        {
            Name = "Emon Talukder",
            Email = "student3@assignmenthub.local",
            PasswordHash = passwordHasher.Hash("Student@123"),
            Role = UserRole.Student,
            IsActive = true
        };

        await context.Users.AddRangeAsync(admin, teacher1, teacher2, student1, student2, student3);
        await context.SaveChangesAsync();

        var teacher1Id = teacher1.Id;
        var teacher2Id = teacher2.Id;
        var student1Id = student1.Id;
        var student2Id = student2.Id;
        var student3Id = student3.Id;

        // ----- Classes -----
        var class9 = new Class { Name = "Class 9" };
        var class10 = new Class { Name = "Class 10" };

        await context.Classes.AddRangeAsync(class9, class10);
        await context.SaveChangesAsync();

        var class9Id = class9.Id;
        var class10Id = class10.Id;

        // ----- Subjects -----
        var mathematics = new Subject { Name = "Mathematics", Code = "MATH" };
        var english = new Subject { Name = "English", Code = "ENG" };
        var physics = new Subject { Name = "Physics", Code = "PHY" };

        await context.Subjects.AddRangeAsync(mathematics, english, physics);
        await context.SaveChangesAsync();

        var mathematicsId = mathematics.Id;
        var englishId = english.Id;
        var physicsId = physics.Id;

        // ----- ClassSubjects (teacher assignments) -----
        var class9Math = new ClassSubject { ClassId = class9Id, SubjectId = mathematicsId, TeacherId = teacher1Id };
        var class9English = new ClassSubject { ClassId = class9Id, SubjectId = englishId, TeacherId = teacher2Id };
        var class10Physics = new ClassSubject { ClassId = class10Id, SubjectId = physicsId, TeacherId = teacher1Id };

        await context.ClassSubjects.AddRangeAsync(class9Math, class9English, class10Physics);
        await context.SaveChangesAsync();

        var class9MathId = class9Math.Id;
        var class9EnglishId = class9English.Id;
        var class10PhysicsId = class10Physics.Id;

        // ----- StudentClasses (enrollments) -----
        var enrollments = new[]
        {
            new StudentClass { StudentId = student1Id, ClassId = class9Id, EnrolledAt = DateTime.UtcNow },
            new StudentClass { StudentId = student2Id, ClassId = class9Id, EnrolledAt = DateTime.UtcNow },
            new StudentClass { StudentId = student3Id, ClassId = class10Id, EnrolledAt = DateTime.UtcNow }
        };

        await context.StudentClasses.AddRangeAsync(enrollments);
        await context.SaveChangesAsync();

        // ----- Assignments -----
        var algebraBasics = new Assignment
        {
            Title = "Algebra Basics",
            Description = "Solve the linear equations worksheet.",
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            Status = AssignmentStatus.Published,
            ClassSubjectId = class9MathId,
            CreatedByTeacherId = teacher1Id
        };

        var quadraticEquations = new Assignment
        {
            Title = "Quadratic Equations",
            Description = "Chapter 4 practice problems.",
            DeadlineUtc = DateTime.UtcNow.AddDays(-2),
            MaxMarks = 50,
            Status = AssignmentStatus.Published,
            ClassSubjectId = class9MathId,
            CreatedByTeacherId = teacher1Id
        };

        var essayDraft = new Assignment
        {
            Title = "Essay Draft",
            Description = "First draft of the persuasive essay.",
            DeadlineUtc = DateTime.UtcNow.AddDays(10),
            MaxMarks = 100,
            Status = AssignmentStatus.Draft,
            ClassSubjectId = class9EnglishId,
            CreatedByTeacherId = teacher2Id
        };

        var newtonsLawsQuiz = new Assignment
        {
            Title = "Newton's Laws Quiz",
            Description = "Short quiz on the three laws of motion.",
            DeadlineUtc = DateTime.UtcNow.AddDays(5),
            MaxMarks = 30,
            Status = AssignmentStatus.Published,
            ClassSubjectId = class10PhysicsId,
            CreatedByTeacherId = teacher1Id
        };

        await context.Assignments.AddRangeAsync(algebraBasics, quadraticEquations, essayDraft, newtonsLawsQuiz);
        await context.SaveChangesAsync();

        var algebraBasicsId = algebraBasics.Id;
        var quadraticEquationsId = quadraticEquations.Id;
        var newtonsLawsQuizId = newtonsLawsQuiz.Id;

        // ----- Submissions -----
        var student1AlgebraSubmission = new Submission
        {
            AssignmentId = algebraBasicsId,
            StudentId = student1Id,
            Content = "2x+3=7 => x=2 ...(full worked answer)",
            SubmittedAt = DateTime.UtcNow.AddDays(-1),
            Status = SubmissionStatus.Pending,
            MarksAwarded = null,
            Feedback = null
        };

        var student2AlgebraSubmission = new Submission
        {
            AssignmentId = algebraBasicsId,
            StudentId = student2Id,
            Content = "My worked solution...",
            SubmittedAt = DateTime.UtcNow.AddDays(-1),
            Status = SubmissionStatus.Graded,
            MarksAwarded = 85,
            Feedback = "Good work, minor sign error in Q3."
        };

        var student1QuadraticSubmission = new Submission
        {
            AssignmentId = quadraticEquationsId,
            StudentId = student1Id,
            Content = "Completed all 10 problems.",
            SubmittedAt = DateTime.UtcNow.AddDays(-5),
            Status = SubmissionStatus.Returned,
            MarksAwarded = null,
            Feedback = "Please redo problems 4 and 7 and resubmit."
        };

        var student3NewtonSubmission = new Submission
        {
            AssignmentId = newtonsLawsQuizId,
            StudentId = student3Id,
            Content = "Answers: 1) ... 2) ... 3) ...",
            SubmittedAt = DateTime.UtcNow.AddHours(-6),
            Status = SubmissionStatus.Pending,
            MarksAwarded = null,
            Feedback = null
        };

        await context.Submissions.AddRangeAsync(
            student1AlgebraSubmission,
            student2AlgebraSubmission,
            student1QuadraticSubmission,
            student3NewtonSubmission);
        await context.SaveChangesAsync();

        var student2AlgebraSubmissionId = student2AlgebraSubmission.Id;
        var student1QuadraticSubmissionId = student1QuadraticSubmission.Id;

        // ----- GradeAuditLogs -----
        var algebraGradeAudit = new GradeAuditLog
        {
            SubmissionId = student2AlgebraSubmissionId,
            GradedByTeacherId = teacher1Id,
            PreviousStatus = SubmissionStatus.Pending,
            NewStatus = SubmissionStatus.Graded,
            PreviousMarks = null,
            NewMarks = 85,
            GradedAt = DateTime.UtcNow.AddHours(-12)
        };

        var quadraticReturnAudit = new GradeAuditLog
        {
            SubmissionId = student1QuadraticSubmissionId,
            GradedByTeacherId = teacher1Id,
            PreviousStatus = SubmissionStatus.Pending,
            NewStatus = SubmissionStatus.Returned,
            PreviousMarks = null,
            NewMarks = null,
            GradedAt = DateTime.UtcNow.AddDays(-4)
        };

        await context.GradeAuditLogs.AddRangeAsync(algebraGradeAudit, quadraticReturnAudit);
        await context.SaveChangesAsync();
    }
}
