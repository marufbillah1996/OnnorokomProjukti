using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentHub.Infrastructure.Persistence.Configurations;

public class ClassSubjectConfiguration : IEntityTypeConfiguration<ClassSubject>
{
    public void Configure(EntityTypeBuilder<ClassSubject> builder)
    {
        builder.ToTable("ClassSubjects");

        builder.HasIndex(cs => new { cs.ClassId, cs.SubjectId }).IsUnique();

        builder.HasOne(cs => cs.Class)
            .WithMany(c => c.ClassSubjects)
            .HasForeignKey(cs => cs.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Subject)
            .WithMany(s => s.ClassSubjects)
            .HasForeignKey(cs => cs.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teacher relationship configured from the User side (UserConfiguration.TeachingAssignments)
        // to avoid two HasForeignKey mappings for the same FK column.

        builder.HasMany(cs => cs.Assignments)
            .WithOne(a => a.ClassSubject)
            .HasForeignKey(a => a.ClassSubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
