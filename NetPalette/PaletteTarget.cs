using SkiaSharp;

namespace NetPalette;

public class PaletteTarget
{
    public float MinimumSaturation { get; set; } = 0.0f;
    public float TargetSaturation { get; set; } = 0.5f;
    public float MaximumSaturation { get; set; } = 1.0f;
    public float MinimumLightness { get; set; } = 0.0f;
    public float TargetLightness { get; set; } = 0.5f;
    public float MaximumLightness { get; set; } = 1.0f;
    public bool IsExclusive { get; set; } = true;

    public string Name { get; set; } = "Custom";

    public float SaturationWeight { get; set; } = _weightSaturation;
    public float LightnessWeight { get; set; } = _weightLightness;
    public float PopulationWeight { get; set; } = _weightPopulation;

    public static float _targetDarkLightness = 0.26f;
    public static float _maxDarkLightness = 0.45f;

    public static float _minLightLightness = 0.55f;
    public static float _targetLightLightness = 0.74f;

    public static float _minNormalLightness = 0.3f;
    public static float _maxNormalLightness = 0.7f;

    public static float _targetMutedSaturation = 0.3f;
    public static float _maxMutedSaturation = 0.4f;

    public static float _targetVibrantSaturation = 1.0f;
    public static float _minVibrantSaturation = 0.35f;

    public static float _weightSaturation = 0.24f;
    public static float _weightLightness = 0.52f;
    public static float _weightPopulation = 0.24f;

    public static PaletteTarget LightVibrant = new PaletteTarget
    {
        TargetLightness = _targetLightLightness,
        MinimumLightness = _minLightLightness,
        MinimumSaturation = _minVibrantSaturation,
        TargetSaturation = _targetVibrantSaturation,
        Name = "LightVibrant"
    };

    public static PaletteTarget Vibrant = new PaletteTarget
    {
        MinimumLightness = _minNormalLightness,
        MaximumLightness = _maxNormalLightness,
        MinimumSaturation = _minVibrantSaturation,
        TargetSaturation = _targetVibrantSaturation,
        Name = "Vibrant"
    };

    public static PaletteTarget DarkVibrant = new PaletteTarget
    {
        TargetLightness = _targetDarkLightness,
        MaximumLightness = _maxDarkLightness,
        MinimumSaturation = _minVibrantSaturation,
        TargetSaturation = _targetVibrantSaturation,
        Name = "DarkVibrant"
    };

    public static PaletteTarget LightMuted = new PaletteTarget
    {
        TargetLightness = _targetLightLightness,
        MinimumLightness = _minLightLightness,
        TargetSaturation = _targetMutedSaturation,
        MaximumSaturation = _maxMutedSaturation,
        Name = "LightMuted"
    };

    public static PaletteTarget Muted = new PaletteTarget
    {
        MinimumLightness = _minNormalLightness,
        MaximumLightness = _maxNormalLightness,
        TargetSaturation = _targetMutedSaturation,
        MaximumSaturation = _maxMutedSaturation,
        Name = "Muted"
    };

    public static PaletteTarget DarkMuted = new PaletteTarget
    {
        TargetLightness = _targetDarkLightness,
        MaximumLightness = _maxDarkLightness,
        TargetSaturation = _targetMutedSaturation,
        MaximumSaturation = _maxMutedSaturation,
        Name = "DarkMuted"
    };

	public static List<PaletteTarget> BaseTargets = new List<PaletteTarget>
    {
        LightVibrant,
        Vibrant,
        DarkVibrant,
        LightMuted,
        Muted,
        DarkMuted
    };

    public void NormalizeWeights()
    {
        float sum = SaturationWeight + LightnessWeight + PopulationWeight;
        if (sum != 0.0)
        {
            SaturationWeight /= sum;
            LightnessWeight /= sum;
            PopulationWeight /= sum;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is PaletteTarget target &&
               MinimumSaturation == target.MinimumSaturation &&
               TargetSaturation == target.TargetSaturation &&
               MaximumSaturation == target.MaximumSaturation &&
               MinimumLightness == target.MinimumLightness &&
               TargetLightness == target.TargetLightness &&
               MaximumLightness == target.MaximumLightness &&
               SaturationWeight == target.SaturationWeight &&
               LightnessWeight == target.LightnessWeight &&
               PopulationWeight == target.PopulationWeight;
    }

    public override int GetHashCode()
    {
        
        return HashCode.Combine(HashCode.Combine(MinimumSaturation, TargetSaturation, MaximumSaturation, MinimumLightness, TargetLightness), MaximumLightness, SaturationWeight, LightnessWeight, PopulationWeight);
    }

    public override string ToString()
    {
        return $"PaletteTarget: MinimumSaturation={MinimumSaturation}, TargetSaturation={TargetSaturation}, MaximumSaturation={MaximumSaturation}, MinimumLightness={MinimumLightness}, TargetLightness={TargetLightness}, MaximumLightness={MaximumLightness}, SaturationWeight={SaturationWeight}, LightnessWeight={LightnessWeight}, PopulationWeight={PopulationWeight}";
    }

    public static bool operator ==(PaletteTarget left, PaletteTarget right)
    {
        return EqualityComparer<PaletteTarget>.Default.Equals(left, right);
    }

    public static bool operator !=(PaletteTarget left, PaletteTarget right)
    {
        return !(left == right);
    }

    public static float CalculateContrast(SKColor foreground, SKColor background)
    {
        if (background.Alpha != 0xff)
        {
            throw new ArgumentException("background can not be translucent: " + background);
        }
        if (foreground.Alpha < 0xff)
        {
            foreground = Utils.AlphaBlend(foreground, background);
        }
        float lightness1 = Utils.ComputeLuminance(foreground) + 0.05f;
        float lightness2 = Utils.ComputeLuminance(background) + 0.05f;
        return Math.Max(lightness1, lightness2) / Math.Min(lightness1, lightness2);
    }

    public static int? CalculateMinimumAlpha(SKColor foreground, SKColor background, float minContrastRatio)
    {
        if (background.Alpha != 0xff)
        {
            throw new ArgumentException("The background cannot be translucent: " + background);
        }

        float contrastCalculator(SKColor fg, SKColor bg, int alpha)
        {
            SKColor testForeground = fg.WithAlpha((byte)alpha);
            return CalculateContrast(testForeground, bg);
        }

        float testRatio = contrastCalculator(foreground, background, 0xff);
        if (testRatio < minContrastRatio)
        {
            return null;
        }
        foreground = foreground.WithAlpha(0xff);
        return BinaryAlphaSearch(foreground, background, minContrastRatio, contrastCalculator);
    }

    public static int BinaryAlphaSearch(SKColor foreground, SKColor background, float minContrastRatio, Func<SKColor, SKColor, int, float> calculator)
    {
        const int minAlphaSearchMaxIterations = 10;
        const int minAlphaSearchPrecision = 1;

        int numIterations = 0;
        int minAlpha = 0;
        int maxAlpha = 0xff;
        while (numIterations <= minAlphaSearchMaxIterations && (maxAlpha - minAlpha) > minAlphaSearchPrecision)
        {
            int testAlpha = (minAlpha + maxAlpha) / 2;
            float testRatio = calculator(foreground, background, testAlpha);
            if (testRatio < minContrastRatio)
            {
                minAlpha = testAlpha;
            }
            else
            {
                maxAlpha = testAlpha;
            }
            numIterations++;
        }
        return maxAlpha;
    }
}