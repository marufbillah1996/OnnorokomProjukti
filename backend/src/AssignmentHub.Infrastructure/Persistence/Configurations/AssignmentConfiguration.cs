using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentHub.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(a => a.DeadlineUtc);
        builder.HasIndex(a => new { a.ClassSubjectId, a.Status });

        builder.HasMany(a => a.Submissions)
            .WithOne(s => s.Assignment)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // The Postgres-native "xmin" optimistic concurrency token is configured conditionally in
        // AppDbContext.OnModelCreating (only when the active provider is Npgsql), not here — it's
        // real Postgres system-column behavior with no SQLite equivalent, and this configuration
        // class also runs against the SQLite in-memory provider used by the integration test host.
    }
}
