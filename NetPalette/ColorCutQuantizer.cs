using SkiaSharp;

namespace NetPalette;

public class ColorCutQuantizer
{
	public ColorCutQuantizer(SKBitmap encodedImage, int maxColors, List<IPaletteFilter> filters, SKRect region)
	{
		_encodedImage = encodedImage;
		_filters = filters;
		_maxColors = maxColors;
		_region = region;

		QuantizedColors = QuantizeColors();
	}

	SKBitmap _encodedImage;
    int _maxColors;
	List<IPaletteFilter> _filters;
	SKRect _region;
	public List<PaletteColor> QuantizedColors { get; private set; }

	public List<PaletteColor> GetImagePixels(SKBitmap bitmap,  SKRect? region)
	{
		int width = bitmap.Width;
		int height = bitmap.Height;
		int rowStride = width * 4;
		int rowStart = 0;
		int rowEnd = 0;
		int colStart = 0;
		int colEnd = 0;


		if (region is not null && region.Value != SKRect.Empty)
		{
			rowStart = (int)region.Value.Top;
			rowEnd = (int)region.Value.Bottom;
			colStart = (int)region.Value.Left;
			colEnd = (int)region.Value.Right;
		}
		else
		{
			rowStart = 0;
			rowEnd = height;
			colStart = 0;
			colEnd = width;
		}

		List<PaletteColor> colors = new List<PaletteColor>();
		int row = 0;
		int col = 0;
		foreach(var color in bitmap.Pixels)
        {
			if (row >= rowStart && row < rowEnd && col >= colStart && col < colEnd)
			{
				colors.Add(new PaletteColor(color, 1));
			}
			col++;
			if (col >= width)
			{
				col = 0;
				row++;
			}

        }
		return colors;
	}

	bool ShouldIgnoreColor(SKColor color)
	{
		foreach (var filter in _filters)
		{
			if (!filter.IsAllowed(color))
			{
				return true;
			}
		}
		return false;
	}

	List<PaletteColor> QuantizeColors()
	{
		const int quantizeWordWidth = 5;
		const int quantizeChannelWidth = 8;
		const int quantizeShift = quantizeChannelWidth - quantizeWordWidth;
		const int quantizeWordMask = ((1 << quantizeWordWidth) - 1) << quantizeShift;

		List<PaletteColor> paletteColors = new List<PaletteColor>();
		List<PaletteColor> pixels = GetImagePixels(_encodedImage, _region);
		ColorHistogram hist = new ColorHistogram();
		SKColor? currentColor = null;
		ColorCount? currentColorCount = null;

		foreach (var pixel in pixels)
		{
			SKColor quantizedColor = new SKColor(
				(byte)(pixel.Color.Red & quantizeWordMask),
				(byte)(pixel.Color.Green & quantizeWordMask),
				(byte)(pixel.Color.Blue & quantizeWordMask),
				pixel.Color.Alpha
			);
			SKColor colorKey = new SKColor(quantizedColor.Red, quantizedColor.Green, quantizedColor.Blue, 255);
			if (quantizedColor.Alpha == 0)
			{
				continue;
			}
			if (currentColor != colorKey)
			{
				currentColor = colorKey;
				currentColorCount = hist[colorKey];
				if(currentColorCount == null)
                {
                    hist[colorKey] = currentColorCount = new ColorCount();
                }
			}

            currentColorCount.Count += 1;
		}
		hist.RemoveWhere(x => ShouldIgnoreColor(x));
		if (hist.Hist.Count() <= _maxColors)
		{
			paletteColors.Clear();
			foreach (var color in hist.Keys)
			{
				paletteColors.Add(new PaletteColor(color, hist[color].Count));
			}
		}
		else
		{
			paletteColors.Clear();
			paletteColors.AddRange(QuantizePixels(_maxColors, hist));
		}
		return paletteColors;
	}

	List<PaletteColor> QuantizePixels(int maxColors, ColorHistogram histogram)
	{

		PriorityQueue<ColorVolumeBox, int> priorityQueue = new PriorityQueue<ColorVolumeBox, int>(new VolumeComparator());

		var newColorVolumeBox = new ColorVolumeBox(0, histogram.Hist.Count() - 1, histogram, histogram.Keys.ToList());
		priorityQueue.Enqueue(newColorVolumeBox, newColorVolumeBox.Volume);
		SplitBoxes(priorityQueue, maxColors);
		return GenerateAverageColors(priorityQueue);
	}

	void SplitBoxes(PriorityQueue<ColorVolumeBox, int> queue, int maxSize)
    {
        while (queue.Count < maxSize)
        {
            var colorVolumeBox = queue.Dequeue();
            if (colorVolumeBox.CanSplit())
            {
                var split = colorVolumeBox.SplitBox();
                queue.Enqueue(split, split.Volume);
				queue.Enqueue(colorVolumeBox, colorVolumeBox.Volume);
            }
        }
    }

	List<PaletteColor> GenerateAverageColors(PriorityQueue<ColorVolumeBox, int> colorVolumeBoxes)
		{
        List<PaletteColor> colors = new List<PaletteColor>();
        foreach (var colorVolumeBox in colorVolumeBoxes.UnorderedItems.ToList())
        {
            var paletteColor = colorVolumeBox.Element.GetAverageColor();
            if (!ShouldIgnoreColor(paletteColor.Color))
            {
                colors.Add(paletteColor);
            }
        }
        return colors;
    }


	class VolumeComparator : IComparer<int>
	{
		public int Compare(int x, int y)
		{
			return y.CompareTo(x);
		}
	}
}