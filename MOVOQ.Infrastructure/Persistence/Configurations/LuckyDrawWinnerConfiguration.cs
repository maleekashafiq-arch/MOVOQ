using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class LuckyDrawWinnerConfiguration : IEntityTypeConfiguration<LuckyDrawWinner>
{
    public void Configure(EntityTypeBuilder<LuckyDrawWinner> builder)
    {
        // SQLite (dev/tests) does not allow subqueries in CHECK constraints; enforce winner UserId = ticket UserId in Application (and SQL Server CHECK in a provider-specific migration when needed).
        builder.ToTable("lucky_draw_winners");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.LuckyDrawId).IsUnique();
        builder.HasIndex(x => x.LuckyDrawTicketId).IsUnique();

        builder.Property(x => x.SelectionMethod).HasMaxLength(128);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasOne(x => x.LuckyDraw)
            .WithOne(x => x.Winner)
            .HasForeignKey<LuckyDrawWinner>(x => x.LuckyDrawId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LuckyDrawTicket)
            .WithOne(x => x.Winner)
            .HasForeignKey<LuckyDrawWinner>(x => x.LuckyDrawTicketId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Append-only winner facts: no soft-delete filter (rows must remain visible for compliance).
    }
}
