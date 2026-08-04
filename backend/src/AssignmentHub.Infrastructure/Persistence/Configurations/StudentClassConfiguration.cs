using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentHub.Infrastructure.Persistence.Configurations;

public class StudentClassConfiguration : IEntityTypeConfiguration<StudentClass>
{
    public void Configure(EntityTypeBuilder<StudentClass> builder)
    {
        builder.ToTable("StudentClasses");

        builder.HasKey(sc => new { sc.StudentId, sc.ClassId });

        // Student side configured here; Class side (Enrollments) configured here too since
        // StudentClass has no BaseEntity/Id and is owned equally by both parents.
        builder.HasOne(sc => sc.Class)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(sc => sc.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Student navigation (User.Enrollments) is configured from UserConfiguration to avoid
        // a duplicate FK mapping for StudentId.
    }
}
