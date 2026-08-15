using System.Security.Cryptography;

namespace MOVOQ.Application.Features.Draws;

/// <summary>
/// Issues the human-readable code printed on a ticket. Uniqueness inside a draw is guaranteed by a unique
/// index, so the generator only has to make collisions vanishingly unlikely.
/// </summary>
internal static class TicketNumbers
{
    // No I, O, 0, or 1: a user reading a code back to support should not have to guess.
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Next(int length)
    {
        return string.Create(length, length, static (span, _) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            }
        });
    }
}
