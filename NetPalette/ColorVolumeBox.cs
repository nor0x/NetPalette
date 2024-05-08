using SkiaSharp;

namespace NetPalette;

internal class ColorVolumeBox
{
	public int Volume { get; private set; }
	public int Count { get; private set; }
	public int MinRed { get; private set; }
	public int MaxRed { get; private set; }
	public int MinGreen { get; private set; }
	public int MaxGreen { get; private set; }
	public int MinBlue { get; private set; }
	public int MaxBlue { get; private set; }

	int _population;
	int _lowerIndex;
	int _upperIndex;

	ColorHistogram _histogram;
	List<SKColor> _colors;

	public ColorVolumeBox(int volume, int count, int minRed, int maxRed, int minGreen, int maxGreen, int minBlue, int maxBlue)
	{
		Volume = volume;
		Count = count;
		MinRed = minRed;
		MaxRed = maxRed;
		MinGreen = minGreen;
		MaxGreen = maxGreen;
		MinBlue = minBlue;
		MaxBlue = maxBlue;
	}

	public ColorVolumeBox(int volume, int count, ColorHistogram histogram, List<SKColor> colors)
	{
		Volume = volume;
		Count = count;
		_histogram = histogram;
		_colors = colors;
		_lowerIndex = 0;
		_upperIndex = colors.Count - 1;
	}

    public ColorVolumeBox(ColorHistogram histogram, List<SKColor> colors, int lowerIndex, int upperIndex)
	{
		_histogram = histogram;
		_colors = colors;
		_lowerIndex = lowerIndex;
		_upperIndex = upperIndex;
		FitMinimumBox();
	}

	public void SplitBox(out ColorVolumeBox lower, out ColorVolumeBox upper)
	{
		int splitPoint = FindSplitPoint();
		lower = new ColorVolumeBox(Volume, Count, MinRed, MaxRed, MinGreen, MaxGreen, MinBlue, splitPoint);
		upper = new ColorVolumeBox(Volume, Count, MinRed, MaxRed, MinGreen, MaxGreen, splitPoint, MaxBlue);
	}

	public (ColorVolumeBox lower, ColorVolumeBox upper) SplitAndReturnBox()
    {
        int splitPoint = FindSplitPoint();
        ColorVolumeBox lower = new ColorVolumeBox(Volume, Count, MinRed, MaxRed, MinGreen, MaxGreen, MinBlue, splitPoint);
        ColorVolumeBox upper = new ColorVolumeBox(Volume, Count, MinRed, MaxRed, MinGreen, MaxGreen, splitPoint, MaxBlue);
        return (lower, upper);
    }

	public bool CanSplit()
	{
		return GetColorCount() > 1;
	}

	int GetColorCount()
	{
		return 1 + _upperIndex - _lowerIndex;
	}

	void FitMinimumBox()
	{
		int minRed = 256;
		int minGreen = 256;
		int minBlue = 256;
		int maxRed = -1;
		int maxGreen = -1;
		int maxBlue = -1;
		int count = 0;
		for (int i = _lowerIndex; i <= _upperIndex; i++)
		{
			SKColor color = _colors[i];
			count += _histogram[color].Count;
			if (color.Red > maxRed)
			{
				maxRed = color.Red;
			}
			if (color.Red < minRed)
			{
				minRed = color.Red;
			}
			if (color.Green > maxGreen)
			{
				maxGreen = color.Green;
			}
			if (color.Green < minGreen)
			{
				minGreen = color.Green;
			}
			if (color.Blue > maxBlue)
			{
				maxBlue = color.Blue;
			}
			if (color.Blue < minBlue)
			{
				minBlue = color.Blue;
			}
		}
		MinRed = minRed;
		MaxRed = maxRed;
		MinGreen = minGreen;
		MaxGreen = maxGreen;
		MinBlue = minBlue;
		MaxBlue = maxBlue;
		_population = count;
	}

	int FindSplitPoint()
    {
        ColorComponent longestDimension = GetLongestColorDimension();
        int CompareColors(SKColor a, SKColor b)
        {
            int MakeValue(int first, int second, int third)
            {
                return first << 16 | second << 8 | third;
            }

            switch (longestDimension)
            {
                case ColorComponent.Red:
                    int arValue = MakeValue(a.Red, a.Green, a.Blue);
                    int brValue = MakeValue(b.Red, b.Green, b.Blue);
                    return arValue.CompareTo(brValue);
                case ColorComponent.Green:
                    int agValue = MakeValue(a.Green, a.Red, a.Blue);
                    int bgValue = MakeValue(b.Green, b.Red, b.Blue);
                    return agValue.CompareTo(bgValue);
                case ColorComponent.Blue:
                    int abValue = MakeValue(a.Blue, a.Green, a.Red);
                    int bbValue = MakeValue(b.Blue, b.Green, b.Red);
                    return abValue.CompareTo(bbValue);
            }
			return 0;
        }

        List<SKColor> colorSubset = _colors.GetRange(_lowerIndex, _upperIndex - _lowerIndex + 1);
        colorSubset.Sort(CompareColors);
        _colors.RemoveRange(_lowerIndex, _upperIndex - _lowerIndex + 1);
        _colors.InsertRange(_lowerIndex, colorSubset);
        int median = (_population / 2);
        for (int i = 0, count = 0; i <= colorSubset.Count; i++)
        {
            count += _histogram[colorSubset[i]].Count;
            if (count >= median)
            {
                return Math.Min(_upperIndex - 1, i + _lowerIndex);
            }
        }
        return _lowerIndex;
    }

	public PaletteColor GetAverageColor()
    {
        int redSum = 0;
        int greenSum = 0;
        int blueSum = 0;
        int totalPopulation = 0;
        for (int i = _lowerIndex; i <= _upperIndex; i++)
        {
            SKColor color = _colors[i];
            int colorPopulation = _histogram[color].Count;
            totalPopulation += colorPopulation;
            redSum += colorPopulation * color.Red;
            greenSum += colorPopulation * color.Green;
            blueSum += colorPopulation * color.Blue;
        }
        int redMean = (redSum / totalPopulation);
        int greenMean = (greenSum / totalPopulation);
        int blueMean = (blueSum / totalPopulation);
        return new PaletteColor(new SKColor((byte)redMean, (byte)greenMean, (byte)blueMean), totalPopulation);
    }

	public ColorVolumeBox SplitBox()
	{
		if (!CanSplit())
		{
			throw new InvalidOperationException("Can't split a box with only 1 color");
		}

		int splitPoint = FindSplitPoint();
        ColorVolumeBox newBox = new ColorVolumeBox(_histogram, _colors, splitPoint + 1, _upperIndex);
		_upperIndex = splitPoint;
		FitMinimumBox();
		return newBox;
	}

	ColorComponent GetLongestColorDimension()
    {
        int redLength = MaxRed - MinRed;
        int greenLength = MaxGreen - MinGreen;
        int blueLength = MaxBlue - MinBlue;

        if (redLength >= greenLength && redLength >= blueLength)
        {
            return ColorComponent.Red;
        }
        else if (greenLength >= redLength && greenLength >= blueLength)
        {
            return ColorComponent.Green;
        }
        else
        {
            return ColorComponent.Blue;
        }
    }
}