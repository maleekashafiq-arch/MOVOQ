using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class StepGoalConfiguration : IEntityTypeConfiguration<StepGoal>
{
    public void Configure(EntityTypeBuilder<StepGoal> builder)
    {
        builder.ToTable("step_goals");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.IsActive });

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
