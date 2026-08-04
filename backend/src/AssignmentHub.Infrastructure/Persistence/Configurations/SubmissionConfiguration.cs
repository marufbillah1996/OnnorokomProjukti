using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentHub.Infrastructure.Persistence.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");

        builder.Property(s => s.Content).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

        // Exactly one submission row per student per assignment — the update endpoint
        // modifies this row rather than creating a new one.
        builder.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
    }
}
