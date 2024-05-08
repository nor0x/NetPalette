<img src="https://raw.githubusercontent.com/nor0x/NetPalette/main/Art/packageicon.png" width="320px" />

# NetPalette 🎨
[![.NET](https://github.com/nor0x/NetPalette/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nor0x/NetPalette/actions/workflows/dotnet.yml)
[![](https://img.shields.io/nuget/v/NetPalette)](https://www.nuget.org/packages/NetPalette)
[![](https://img.shields.io/nuget/dt/NetPalette)](https://www.nuget.org/packages/NetPalette)

A Library for generating color palettes from images - powered by SkiaSharp so it can be used in any modern .NET UI framework. Inspired by the [Android Palette API](https://developer.android.com/develop/ui/views/graphics/palette-colors)

## Demo
There is a .NET MAUI app in [/Samples](https://github.com/nor0x/NetPalette/tree/main/Samples/NetPalette.Sample.Maui) which demonstrates palette generation for random Unsplash images. The library is not limited to MAUI, it can be used in any .NET UI framework like Uno, Avalonia, WinUI, WPF, etc. any framework that supports SkiaSharp.

https://github.com/nor0x/NetPalette/assets/3210391/dea64c88-354f-458f-bf04-9e915d801a43

## Usage
### Palette Generation

To generate a palette from a bitmap image, you can use the FromBitmap method of the PaletteGenerator class. This method takes in various parameters to customize the palette generation process

```csharp
using SkiaSharp;

// Load your bitmap image
SKBitmap bitmap = SKBitmap.Decode("path/to/your/image.png");

// Define region of interest if necessary - this is optional
SKRect region = new SKRect(0, 0, bitmap.Width, bitmap.Height); // Entire image

// Optionally define palette filters - this is optional
List<IPaletteFilter> filters = new List<IPaletteFilter> { new AnyPaletteFilter() };

// Optionally define palette targets - this is optional
List<PaletteTarget> targets = new List<PaletteTarget> { PaletteTarget.Vibrant, PaletteTarget.DarkMuted };

// Generate the palette
PaletteGenerator paletteGenerator = PaletteGenerator.FromBitmap(bitmap, maximumColorCount: 16, region, filters, targets, fillMissingTargets: true);

// Access the generated palette colors
SKColor vibrantColor = paletteGenerator.VibrantColor.Color;
SKColor darkMutedColor = paletteGenerator.DarkMutedColor.Color;

// Iterate over all quantized colors
foreach (SKColor color in paletteGenerator.Colors)
{
    // Do something with each color
}
```

### Accessing Generated Palette Colors
You can access the generated palette colors using the properties provided by the PaletteGenerator instance.

```csharp
// Accessing specific palette colors
SKColor vibrantColor = paletteGenerator.VibrantColor.Color;
SKColor lightVibrantColor = paletteGenerator.LightVibrantColor.Color;
SKColor darkVibrantColor = paletteGenerator.DarkVibrantColor.Color;
SKColor mutedColor = paletteGenerator.MutedColor.Color;
SKColor lightMutedColor = paletteGenerator.LightMutedColor.Color;
SKColor darkMutedColor = paletteGenerator.DarkMutedColor.Color;
SKColor dominantColor = paletteGenerator.DominantColor.Color;

// Iterate over palette colors
foreach (SKColor color in paletteGenerator.PaletteColors)
{
    // Do something with each palette color
}

// Iterate over all quantized colors
foreach (SKColor color in paletteGenerator.Colors)
{
	// Do something with each color
}
```

## API
Create a PaletteGenerator instance using the FromBitmap method of the PaletteGenerator class. This method takes in the following parameters:


- `encodedImage` (SKBitmap): The bitmap image from which to generate the palette.
  
- `maximumColorCount` (int, optional): The maximum number of colors to include in the palette. Default is 16.
  
- `region` (SKRect, optional): The region of interest within the image. Default is the entire image.
  
- `filters` (List<IPaletteFilter>, optional): Optional palette filters to apply during palette generation.
  
- `targets` (List<PaletteTarget>, optional): Optional palette targets to specify which colors to select from the generated palette.
  
- `fillMissingBaseTargets` (bool, optional): Specifies whether to fill missing base targets in the selected swatches. Default is false.

#### Returns

A `PaletteGenerator` instance containing the generated palette.

#### Exceptions

- `ArgumentOutOfRangeException`: Thrown when the specified region is outside the bounds of the image.

- `ArgumentException`: Thrown when the image byte data doesn't match the image size or has invalid encoding.

### not finalized yet
please note that the API is not finalized yet and might change in the future. If you have any suggestions or feature requests, feel free to open an issue or a pull request.


## Credits
most of the code is based on the [palette_generator](https://pub.dev/packages/palette_generator) package from the Flutter SDK - credits to the original authors of that code. Great kudos also to the [SkiaSharp](https://github.com/mono/skiasharp) contributors for providing such a great library 👏
