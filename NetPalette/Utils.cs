using SkiaSharp;

namespace NetPalette;

	public static class Utils
{
    public static SKColor AlphaBlend(SKColor color, SKColor backColor)
    {
        byte alpha = color.Alpha;
        byte invAlpha = (byte)(255 - alpha);
        byte r = (byte)((color.Red * alpha + backColor.Red * invAlpha) / 255);
        byte g = (byte)((color.Green * alpha + backColor.Green * invAlpha) / 255);
        byte b = (byte)((color.Blue * alpha + backColor.Blue * invAlpha) / 255);
        return new SKColor(r, g, b);
    }

    public static float ComputeLuminance(SKColor color)
    {
        float R = LinearizeColorComponent(color.Red / 0xFF);
        float G = LinearizeColorComponent(color.Green / 0xFF);
        float B = LinearizeColorComponent(color.Blue / 0xFF);
        return 0.2126f * R + 0.7152f * G + 0.0722f * B;
    }

    public static float LinearizeColorComponent(float component)
    {
        if (component <= 0.03928)
        {
            return component / 12.92f;
        }
        return (float)Math.Pow((component + 0.055) / 1.055, 2.4);
    }

    public static SKColor MakeLighter(this SKColor color, float factor)
    {
        return new SKColor(
            (byte)Math.Min(0xFF, color.Red + 0xFF * factor),
            (byte)Math.Min(0xFF, color.Green + 0xFF * factor),
            (byte)Math.Min(0xFF, color.Blue + 0xFF * factor),
            color.Alpha);
    }

    public static SKColor MakeDarker(this SKColor color, float factor)
    {
        return new SKColor(
                (byte)Math.Max(0, color.Red - 0xFF * factor),
                (byte)Math.Max(0, color.Green - 0xFF * factor),
                (byte)Math.Max(0, color.Blue - 0xFF * factor),
                color.Alpha);
    }

    public static SKColor MakeSaturated(this SKColor color, float factor)
    {
        color.ToHsl(out var h, out var s, out var l);
        s = Math.Min(1, s + factor);
        return SKColor.FromHsl(h, s, l);
    }

    public static SKColor MakeDesaturated(this SKColor color, float factor)
    {
        color.ToHsl(out var h, out var s, out var l);
        s = Math.Max(0, s - factor);
        return SKColor.FromHsl(h, s, l);
    }

}

public record PaletteColor(SKColor Color, int Population);


enum ColorComponent
{
    Red,
    Green,
    Blue,
}