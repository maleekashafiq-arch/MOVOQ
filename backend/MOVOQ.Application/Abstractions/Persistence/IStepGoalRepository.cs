using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IStepGoalRepository
{
    void Add(StepGoal goal);
}
