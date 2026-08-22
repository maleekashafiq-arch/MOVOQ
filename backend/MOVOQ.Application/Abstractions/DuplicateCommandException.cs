namespace MOVOQ.Application.Abstractions;

/// <summary>
/// The database unique index on <c>processed_commands.CommandId</c> rejected a second insert.
/// Callers should return the stored state with <c>Duplicate = true</c> instead of failing the request.
/// </summary>
public sealed class DuplicateCommandException : Exception
{
    public DuplicateCommandException()
        : base("This command was already applied.")
    {
    }
}