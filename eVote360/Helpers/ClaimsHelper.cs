using System.Security.Claims;

namespace eVote360.Helpers;

public static class ClaimsHelper
{
    public static int GetUserId(
        ClaimsPrincipal user)
    {
        return int.Parse(
            user.FindFirstValue(
                ClaimTypes.NameIdentifier)!);
    }

    public static int? GetPartidoId(
        ClaimsPrincipal user)
    {
        var value =
            user.FindFirstValue(
                "PartidoPoliticoId");

        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.Parse(value);
    }
}