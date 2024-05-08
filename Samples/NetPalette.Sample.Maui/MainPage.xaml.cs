using SkiaSharp;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

namespace NetPalette.Sample.Maui
{
	public partial class MainPage : ContentPage
	{
		int count = 0;

		public MainPage()
		{
			InitializeComponent();
		}

		protected override async void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);
			InputImage.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await GetNewImage())
            });
			await GetNewImage();
		}

		async Task GetNewImage()
		{
			using var client = new HttpClient();
			var response = await client.GetAsync("https://source.unsplash.com/random");
			var stream = await response.Content.ReadAsStreamAsync();
			var skBitmap = SKBitmap.Decode(stream);
			
			var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "image.png");
			using var fs = new FileStream(file, FileMode.Create);
			skBitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
			fs.Close();
			InputImage.Source = ImageSource.FromFile(file);

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Debug.WriteLine("Starting Palette Generation");
			var paletteGen = PaletteGenerator.FromBitmap(skBitmap, fillMissingTargets: true);
			stopwatch.Stop();
			Debug.WriteLine($"Palette Generation took {stopwatch.ElapsedMilliseconds }ms");

			try { 
				var dominant = paletteGen.DominantColor;
				DominantColor.BackgroundColor = Color.FromArgb(dominant.Color.ToString());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception occurred for DominantColor: " + ex.Message);
				DominantColor.BackgroundColor = Colors.Magenta;
			}

			try
			{
				var muted = paletteGen.MutedColor;
				MutedColor.BackgroundColor = Color.FromArgb(muted.Color.ToString());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception occurred for MutedColor: " + ex.Message);
				MutedColor.BackgroundColor = Colors.Magenta;
			}

			try
			{
				var vibrant = paletteGen.VibrantColor;
				VibrantColor.BackgroundColor = Color.FromArgb(vibrant.Color.ToString());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception occurred for VibrantColor: " + ex.Message);
				VibrantColor.BackgroundColor = Colors.Magenta;
			}

			try
			{
				var lightVibrant = paletteGen.LightVibrantColor;
				LightVibrantColor.BackgroundColor = Color.FromArgb(lightVibrant.Color.ToString());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception occurred for LightVibrantColor: " + ex.Message);
				LightVibrantColor.BackgroundColor = Colors.Magenta;
			}

			try
			{
				var lightMuted = paletteGen.LightMutedColor;
				LightMutedColor.BackgroundColor = Color.FromArgb(lightMuted.Color.ToString());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception occurred for LightMutedColor: " + ex.Message);
				LightMutedColor.BackgroundColor = Colors.Magenta;
			}

			try
			{
				var darkMuted = paletteGen.DarkMutedColor;
				DarkMutedColor.BackgroundColor = Color.FromArgb(darkMuted.Color.ToString());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception occurred for DarkMutedColor: " + ex.Message);
				DarkMutedColor.BackgroundColor = Colors.Magenta;
			}

			try
			{
				var darkVibrant = paletteGen.DarkVibrantColor;
				DarkVibrantColor.BackgroundColor = Color.FromArgb(darkVibrant.Color.ToString());
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Exception occurred for DarkVibrantColor: " + ex.Message);
				DarkVibrantColor.BackgroundColor = Colors.Magenta;
			}

		}
	}

}
