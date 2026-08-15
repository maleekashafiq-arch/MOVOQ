using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class StepGoalRepository(MovoqDbContext db) : IStepGoalRepository
{
    public void Add(StepGoal goal) => db.StepGoals.Add(goal);
}
