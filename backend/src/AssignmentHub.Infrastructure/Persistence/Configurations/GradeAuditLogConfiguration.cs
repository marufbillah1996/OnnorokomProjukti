using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentHub.Infrastructure.Persistence.Configurations;

public class GradeAuditLogConfiguration : IEntityTypeConfiguration<GradeAuditLog>
{
    public void Configure(EntityTypeBuilder<GradeAuditLog> builder)
    {
        builder.ToTable("GradeAuditLogs");

        builder.Property(g => g.PreviousStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(g => g.NewStatus).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(g => g.Submission)
            .WithMany()
            .HasForeignKey(g => g.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.GradedByTeacher)
            .WithMany()
            .HasForeignKey(g => g.GradedByTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.SubmissionId);
    }
}
