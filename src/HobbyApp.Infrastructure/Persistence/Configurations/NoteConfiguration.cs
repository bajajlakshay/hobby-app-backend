using HobbyApp.Domain.Entities;
using HobbyApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HobbyApp.Infrastructure.Persistence.Configurations;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(512);

        // Block document and flattened text; jsonb gives us indexable/queryable JSON.
        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(x => x.PlainText)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasMaxLength(32);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Primary access pattern: a user's notes filtered by state.
        builder.HasIndex(x => new { x.UserId, x.IsArchived, x.DeletedAt });
    }
}
