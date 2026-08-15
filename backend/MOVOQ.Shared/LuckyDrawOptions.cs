namespace MOVOQ.Shared;

/// <summary>Server-side rules for buying lucky draw tickets and running a draw.</summary>
public static class LuckyDrawOptions
{
    /// <summary>Tickets a single purchase may buy, whatever the daily limit allows in total.</summary>
    public const int MaxTicketsPerPurchase = 20;

    /// <summary>Characters in a ticket number, drawn from an alphabet with no lookalike glyphs.</summary>
    public const int TicketNumberLength = 8;

    /// <summary>How the server describes a winner chosen by its own cryptographic random source.</summary>
    public const string RandomSelectionMethod = "Random (server, cryptographic)";

    /// <summary>Entries returned with a draw's detail view before the list is truncated.</summary>
    public const int MaxEntriesReturned = 500;
}
