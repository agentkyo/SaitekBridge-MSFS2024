namespace SaitekBridge.Panels;

/// <summary>
/// Encodes text for the 5-digit windows of the Saitek panels. Each cell is a raw digit 0-9;
/// 0x0F blanks it and OR-ing 0xD0 lights the decimal point after it.
/// </summary>
public static class SevenSegment
{
    public const int Width = 5;
    public const byte Blank = 0x0F;
    public const byte Minus = 0x0E;
    private const byte DotFlag = 0xD0;

    /// <summary>Right-aligns <paramref name="text"/> (digits, '-', '.', ' ') into 5 cells; extra leading characters are cut.</summary>
    public static void Write(Span<byte> cells, string text)
    {
        var encoded = new List<byte>(Width + 1);
        foreach (var c in text)
        {
            if (c == '.' && encoded.Count > 0) encoded[^1] = (byte)(encoded[^1] | DotFlag);
            else if (c is >= '0' and <= '9') encoded.Add((byte)(c - '0'));
            else if (c == '-') encoded.Add(Minus);
            else encoded.Add(Blank);
        }
        while (encoded.Count < Width) encoded.Insert(0, Blank);
        encoded.Skip(encoded.Count - Width).ToArray().CopyTo(cells);
    }

    public static void Clear(Span<byte> cells) => cells[..Width].Fill(Blank);
}
