using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPalette;

public class ColorHistogram
{
    public Dictionary<int, Dictionary<int, Dictionary<int, ColorCount>>> Hist = new();
    public LinkedList<SKColor> Keys = new();

    public ColorCount? this[SKColor color]
    {
        get
        {
            if (!Hist.TryGetValue(color.Red, out var redMap))
            {
                return null;
            }
            if (!redMap.TryGetValue(color.Blue, out var blueMap))
            {
                return null;
            }
            return blueMap.TryGetValue(color.Green, out var count) ? count : null;
        }
        set
        {
        if (value is null)
        {
            RemoveWhere(c => c == color);
        }
        else
        {
            Add(color, value);
        }
        }
    }

    Dictionary<int, Dictionary<int, ColorCount>> GetRedMap(int red)
    {
        if (!Hist.TryGetValue(red, out var redMap))
        {
            Hist[red] = redMap = new();
        }
        return redMap;
    }

    Dictionary<int, ColorCount> GetBlueMap(int red, int blue)
    {
        var redMap = GetRedMap(red);
        if (!redMap.TryGetValue(blue, out var blueMap))
        {
            redMap[blue] = blueMap = new();
        }
        return blueMap;
    }

    public void Add(SKColor key, ColorCount value)
    {
        var red = key.Red;
        var blue = key.Blue;
        var green = key.Green;

        var blueMap = GetBlueMap(red, blue);
        blueMap[green] = value;

        Keys.AddLast(key);
    }

    public void RemoveWhere(Func<SKColor, bool> predicate)
    {
        foreach (var key in Keys)
        {
            if (predicate(key))
            {
                Hist[key.Red]?[key.Blue]?.Remove(key.Green);
            }
        }
        var toRemove = Keys.Where(predicate).ToArray();
        foreach (var key in toRemove)
        {
            Keys.Remove(key);
        }
    }
}


public class ColorCount
{
    public int Count { get; set; }
}