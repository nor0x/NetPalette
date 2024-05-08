using SkiaSharp;

namespace NetPalette;

public interface IPaletteFilter
{
    bool IsAllowed(SKColor color);
}
public class AvoidRedBlackWhitePaletteFilter : IPaletteFilter
{
    public bool IsAllowed(SKColor color)
    {
        return !IsBlack(color) && !IsWhite(color) && !IsNearRedILine(color);
    }

    bool IsBlack(SKColor color)
    {
        return color.Red == 0 && color.Green == 0 && color.Blue == 0;
    }

    bool IsWhite(SKColor color)
    {
        return color.Red == 255 && color.Green == 255 && color.Blue == 255;
    }

    bool IsNearRedILine(SKColor color)
    {
        return color.Red > 128 && color.Green < 128 && color.Blue < 128;
    }
}

public class AnyPaletteFilter : IPaletteFilter
{
    public bool IsAllowed(SKColor color)
    {
        return true;
    }
}