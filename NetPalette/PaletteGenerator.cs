using SkiaSharp;
using System.Diagnostics;

namespace NetPalette;

public class PaletteGenerator
{
	public PaletteGenerator(List<PaletteColor> colors, List<PaletteTarget> targets, bool fillMissingBaseTargets)
	{
		this._colors = colors;
		this._targets = targets;
		_selectedSwatches = new Dictionary<PaletteTarget, PaletteColor>();
		_dominantColor = new PaletteColor(SKColors.Magenta, 0);
	
		var stopwatch = new Stopwatch();
		SortSwatches();
		SelectSwatches();
		if (fillMissingBaseTargets)
		{
			FillMissingBaseSwatches();
		}
	}

	/// <summary>
	/// Generates a color palette from a bitmap image.
	/// </summary>
	/// <param name="encodedImage">The bitmap image from which to generate the palette.</param>
	/// <param name="maximumColorCount">The maximum number of colors to include in the palette. Default is 16.</param>
	/// <param name="region">The region of interest within the image. Default is the entire image.</param>
	/// <param name="filters">Optional palette filters to apply during palette generation.</param>
	/// <param name="targets">Optional palette targets to specify which colors to select from the generated palette.</param>
	/// <param name="fillMissingBaseTargets">Specifies whether to fill missing base targets in the selected swatches. Default is false.</param>
	/// <returns>A PaletteGenerator instance containing the generated palette.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the specified region is outside the bounds of the image.</exception>
	/// <exception cref="ArgumentException">Thrown when the image byte data doesn't match the image size or has invalid encoding.</exception>

	public static PaletteGenerator FromBitmap(SKBitmap encodedImage, int maximumColorCount = 16, SKRect region = default, List<IPaletteFilter> filters = null, List<PaletteTarget> targets = null, bool fillMissingBaseTargets = false)
	{
		if (region.Left < 0.0 || region.Top < 0.0 || region.Right > encodedImage.Width || region.Bottom > encodedImage.Height)
		{
			throw new ArgumentOutOfRangeException("Region is outside the image");
		}

		if(region.IsEmpty)
		{
			region = new SKRect(0, 0, encodedImage.Width, encodedImage.Height);
		}

		if (encodedImage.Bytes.Length / 4 != encodedImage.Width * encodedImage.Height)
		{
			throw new ArgumentException("Image byte data doesn't match the image size, or has invalid encoding. The encoding must be RGBA with 8 bits per channel.");
		}
		if (filters is null)
		{
			filters = new List<IPaletteFilter> { new AnyPaletteFilter() };
		}
		if (targets is null)
		{
			targets = PaletteTarget.BaseTargets;
		}

		var quantizer = new ColorCutQuantizer(encodedImage, maximumColorCount, filters, region);
		var colors = quantizer.QuantizedColors;
		return new PaletteGenerator(colors, targets, fillMissingBaseTargets);
	}

	int _defaultCalculateNumberColors = 16;

	List<PaletteColor> _colors;
	List<PaletteTarget> _targets;
	Dictionary<PaletteTarget, PaletteColor> _selectedSwatches;
	PaletteColor _dominantColor;

	public PaletteColor VibrantColor => _selectedSwatches[PaletteTarget.Vibrant];
	public PaletteColor LightVibrantColor => _selectedSwatches[PaletteTarget.LightVibrant];
	public PaletteColor DarkVibrantColor => _selectedSwatches[PaletteTarget.DarkVibrant];
	public PaletteColor MutedColor => _selectedSwatches[PaletteTarget.Muted];
	public PaletteColor LightMutedColor => _selectedSwatches[PaletteTarget.LightMuted];
	public PaletteColor DarkMutedColor => _selectedSwatches[PaletteTarget.DarkMuted];
	public PaletteColor DominantColor => _dominantColor ?? _colors[0];


	/// <summary>
	/// All Quantized colors
	/// </summary>
	public IEnumerable<SKColor> Colors
	{
		get
		{
			foreach (var paletteColor in _colors)
			{
				yield return paletteColor.Color;
			}
		}
	}	

	/// <summary>
	/// Colors from the selected swatches
	/// </summary>
	public IEnumerable<SKColor> PaletteColors
	{
		get
		{
			foreach (var paletteColor in _selectedSwatches.Values)
			{
				yield return paletteColor.Color;
			}
		}
	}

	public Dictionary<PaletteTarget, PaletteColor> SelectedSwatches => _selectedSwatches;

	void SortSwatches()
    {
        if (_colors.Count == 0)
        {
            _dominantColor = null;
            return;
        }
        _colors.Sort((a, b) => b.Population.CompareTo(a.Population));
        _dominantColor = _colors[0];
    }

	void SelectSwatches()
    {
        var allTargets = new HashSet<PaletteTarget>(_targets.Concat(PaletteTarget.BaseTargets));
        var usedColors = new HashSet<SKColor>();
        foreach (var target in allTargets)
        {
            target.NormalizeWeights();
            var targetScore = GenerateScoredTarget(target, usedColors);
            if (targetScore is not null)
            {
                _selectedSwatches[target] = targetScore;
            }
        }
    }

	void FillMissingBaseSwatches()
	{
		if (!_selectedSwatches.ContainsKey(PaletteTarget.Vibrant))
		{
			if (_selectedSwatches.ContainsKey(PaletteTarget.LightVibrant))
			{
				_selectedSwatches[PaletteTarget.Vibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.LightVibrant].Color.MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkVibrant))
			{
				_selectedSwatches[PaletteTarget.Vibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkVibrant].Color.MakeLighter(0.5f).MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.Muted))
			{
				_selectedSwatches[PaletteTarget.Vibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.Muted].Color.MakeDesaturated(0.5f), 0);
			}
			else if(_selectedSwatches.ContainsKey(PaletteTarget.LightMuted))
			{
				_selectedSwatches[PaletteTarget.Vibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.LightMuted].Color.MakeDarker(0.5f).MakeSaturated(0.5f), 0);
			}
			else if(_selectedSwatches.ContainsKey(PaletteTarget.DarkMuted))
			{
				_selectedSwatches[PaletteTarget.Vibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkMuted].Color.MakeLighter(0.5f).MakeSaturated(0.5f), 0);
			}
		}
		if (!_selectedSwatches.ContainsKey(PaletteTarget.Muted))
		{
			if (_selectedSwatches.ContainsKey(PaletteTarget.LightMuted))
			{
				_selectedSwatches[PaletteTarget.Muted] = new PaletteColor(_selectedSwatches[PaletteTarget.LightMuted].Color.MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkMuted))
			{
				_selectedSwatches[PaletteTarget.Muted] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkMuted].Color.MakeLighter(0.5f).MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.Vibrant))
			{
				_selectedSwatches[PaletteTarget.Muted] = new PaletteColor(_selectedSwatches[PaletteTarget.Vibrant].Color.MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.LightVibrant))
			{
				_selectedSwatches[PaletteTarget.Muted] = new PaletteColor(_selectedSwatches[PaletteTarget.LightVibrant].Color.MakeDarker(0.5f).MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkVibrant))
			{
				_selectedSwatches[PaletteTarget.Muted] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkVibrant].Color.MakeLighter(0.5f).MakeSaturated(0.5f), 0);
			}
		}
		if (!_selectedSwatches.ContainsKey(PaletteTarget.LightMuted))
		{
			if (_selectedSwatches.ContainsKey(PaletteTarget.Muted))
			{
				_selectedSwatches[PaletteTarget.LightMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.Muted].Color.MakeLighter(0.3f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkMuted))
			{
				_selectedSwatches[PaletteTarget.LightMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkMuted].Color.MakeLighter(0.8f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.LightVibrant))
			{
				_selectedSwatches[PaletteTarget.LightMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.LightVibrant].Color.MakeDesaturated(0.5f), 0);
			}
			else if(_selectedSwatches.ContainsKey(PaletteTarget.Vibrant))
			{
				_selectedSwatches[PaletteTarget.LightMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.Vibrant].Color.MakeLighter(0.3f).MakeDesaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkVibrant))
			{
				_selectedSwatches[PaletteTarget.LightMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkVibrant].Color.MakeLighter(0.8f).MakeDesaturated(0.5f), 0);
			}
		}
		if (!_selectedSwatches.ContainsKey(PaletteTarget.DarkMuted))
		{
			if (_selectedSwatches.ContainsKey(PaletteTarget.Muted))
			{
				_selectedSwatches[PaletteTarget.DarkMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.Muted].Color.MakeDarker(0.3f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.LightMuted))
			{
				_selectedSwatches[PaletteTarget.DarkMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.LightMuted].Color.MakeDarker(0.8f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkVibrant))
			{
				_selectedSwatches[PaletteTarget.DarkMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkVibrant].Color.MakeDesaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.Vibrant))
			{
				_selectedSwatches[PaletteTarget.DarkMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.Vibrant].Color.MakeDarker(0.3f).MakeDesaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.LightVibrant))
			{
				_selectedSwatches[PaletteTarget.DarkMuted] = new PaletteColor(_selectedSwatches[PaletteTarget.LightVibrant].Color.MakeDarker(0.8f).MakeDesaturated(0.5f), 0);
			}
		}
		if (!_selectedSwatches.ContainsKey(PaletteTarget.DarkVibrant))
		{
			if (_selectedSwatches.ContainsKey(PaletteTarget.Vibrant))
			{
				_selectedSwatches[PaletteTarget.DarkVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.Vibrant].Color.MakeDarker(0.3f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.LightVibrant))
			{
				_selectedSwatches[PaletteTarget.DarkVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.LightVibrant].Color.MakeDarker(0.8f), 0);
			}
			else if(_selectedSwatches.ContainsKey(PaletteTarget.DarkMuted))
			{
				_selectedSwatches[PaletteTarget.DarkVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkMuted].Color.MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.Muted))
			{
				_selectedSwatches[PaletteTarget.DarkVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.Muted].Color.MakeDarker(0.3f).MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.LightMuted))
			{
				_selectedSwatches[PaletteTarget.DarkVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.LightMuted].Color.MakeDarker(0.8f).MakeSaturated(0.5f), 0);
			}
		}
		if (!_selectedSwatches.ContainsKey(PaletteTarget.LightVibrant))
		{
			if (_selectedSwatches.ContainsKey(PaletteTarget.Vibrant))
			{
				_selectedSwatches[PaletteTarget.LightVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.Vibrant].Color.MakeLighter(0.3f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkVibrant))
			{
				_selectedSwatches[PaletteTarget.LightVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkVibrant].Color.MakeLighter(0.8f), 0);
			}
			else if(_selectedSwatches.ContainsKey(PaletteTarget.LightMuted))
			{
				_selectedSwatches[PaletteTarget.LightVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.LightMuted].Color.MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.Muted))
			{
				_selectedSwatches[PaletteTarget.LightVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.Muted].Color.MakeLighter(0.3f).MakeSaturated(0.5f), 0);
			}
			else if (_selectedSwatches.ContainsKey(PaletteTarget.DarkMuted))
			{
				_selectedSwatches[PaletteTarget.LightVibrant] = new PaletteColor(_selectedSwatches[PaletteTarget.DarkMuted].Color.MakeLighter(0.8f).MakeSaturated(0.5f), 0);
			}
		}
	}

	PaletteColor? GenerateScoredTarget(PaletteTarget target, HashSet<SKColor> usedColors)
    {
        var maxScoreSwatch = GetMaxScoredSwatchForTarget(target, usedColors);
        if (maxScoreSwatch != null && target.IsExclusive)
        {
            usedColors.Add(maxScoreSwatch.Color);
        }
        return maxScoreSwatch;
    }

	PaletteColor? GetMaxScoredSwatchForTarget(PaletteTarget target, HashSet<SKColor> usedColors)
    {
        float maxScore = 0.0f;
        PaletteColor maxScoreSwatch = null;
        foreach (var paletteColor in _colors)
        {
			var shouldScore = ShouldBeScoredForTarget(paletteColor, target, usedColors);
            if (shouldScore)
            {
                var score = GenerateScore(paletteColor, target);
				if (maxScoreSwatch == null || score > maxScore)
				{
					maxScoreSwatch = paletteColor;
					maxScore = score;
				}
            }
        }
        return maxScoreSwatch;
    }

	bool ShouldBeScoredForTarget(PaletteColor paletteColor, PaletteTarget target, HashSet<SKColor> usedColors)
    {
        paletteColor.Color.ToHsl(out var hue, out var saturation, out var lightness);
		
		saturation /= 100;
		lightness /= 100;


        return saturation >= target.MinimumSaturation &&
			saturation <= target.MaximumSaturation &&
            lightness >= target.MinimumLightness &&
            lightness <= target.MaximumLightness &&
			!usedColors.Contains(paletteColor.Color);
    }

	float GenerateScore(PaletteColor paletteColor, PaletteTarget target)
    {
		paletteColor.Color.ToHsl(out var hue, out var saturation, out var lightness);

		saturation /= 100;
		lightness /= 100;

        float saturationScore = 0.0f;
        float valueScore = 0.0f;
        float populationScore = 0.0f;

        if (target.SaturationWeight > 0.0f)
        {
            saturationScore = target.SaturationWeight *
                (1.0f - Math.Abs(saturation - target.TargetSaturation));
        }
        if (target.LightnessWeight > 0.0f)
        {
            valueScore = target.LightnessWeight *
                (1.0f - Math.Abs(lightness - target.TargetLightness));
        }
        if (_dominantColor != null && target.PopulationWeight > 0.0f)
        {
            populationScore = target.PopulationWeight *
                (paletteColor.Population / _dominantColor.Population);
        }

        return saturationScore + valueScore + populationScore;
    }
}