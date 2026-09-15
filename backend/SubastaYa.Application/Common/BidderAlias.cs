namespace SubastaYa.Application.Common;

// Seudónimo estable para el historial y el detalle: nunca exponemos el userName completo.
public static class BidderAlias
{
    public static string? FromUserName(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var visible = userName.Length <= 3 ? userName : userName[..3];

        return visible + new string('*', Math.Max(3, userName.Length - visible.Length));
    }
}
