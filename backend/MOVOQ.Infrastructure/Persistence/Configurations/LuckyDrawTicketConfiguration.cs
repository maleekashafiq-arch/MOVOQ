using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class LuckyDrawTicketConfiguration : IEntityTypeConfiguration<LuckyDrawTicket>
{
    public void Configure(EntityTypeBuilder<LuckyDrawTicket> builder)
    {
        builder.ToTable("lucky_draw_tickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => new { x.LuckyDrawId, x.TicketNumber }).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.LuckyDrawTickets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PointsLedger)
            .WithMany()
            .HasForeignKey(x => x.PointsLedgerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
