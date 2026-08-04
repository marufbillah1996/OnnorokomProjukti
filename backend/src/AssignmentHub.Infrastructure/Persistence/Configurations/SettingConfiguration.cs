using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssignmentHub.Infrastructure.Persistence.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");

        builder.Property(s => s.SettingKey).IsRequired().HasMaxLength(100);
        builder.Property(s => s.SettingValue).IsRequired().HasMaxLength(2000);

        builder.HasIndex(s => s.SettingKey).IsUnique();
    }
}
