using HobbyApp.Domain.Entities;
using HobbyApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobbyApp.Infrastructure.Persistence.Configurations;

internal sealed class TrackerConfiguration : IEntityTypeConfiguration<Tracker>
{
    public void Configure(EntityTypeBuilder<Tracker> builder)
    {
        builder.HasKey(x => x.Id);

        // Ids come from the client, so the database must not generate them.
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(16);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.IsArchived });
    }
}

internal sealed class TrackerEntryConfiguration : IEntityTypeConfiguration<TrackerEntry>
{
    public void Configure(EntityTypeBuilder<TrackerEntry> builder)
    {
        builder.HasKey(x => new { x.TrackerId, x.Day });

        builder.HasOne(x => x.Tracker)
            .WithMany(t => t.Entries)
            .HasForeignKey(x => x.TrackerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Range scans for the day/week/month views.
        builder.HasIndex(x => new { x.UserId, x.Day });
    }
}
