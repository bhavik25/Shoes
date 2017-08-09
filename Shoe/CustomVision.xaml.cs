using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Geolocator;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Shoe.Model;
using Xamarin.Forms;

namespace Shoe
{
	public partial class CustomVision : ContentPage
	{
		public CustomVision()
		{
			InitializeComponent();
		}

		private async void loadCamera(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				await DisplayAlert("No Camera", ":( No camera available.", "OK");
				return;
			}

			MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
			{
				PhotoSize = PhotoSize.Medium,
				Directory = "Sample",
				Name = $"{DateTime.UtcNow}.jpg"
			});

			if (file == null)
				return;

			image.Source = ImageSource.FromStream(() =>
			{
				return file.GetStream();
			});


			await MakePredictionRequest(file);
		}

		async Task postLocationAsync()
		{

			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 50;

            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10));

            ShoeModel model = new ShoeModel()
			{
				Longitude = (float)position.Longitude,
				Latitude = (float)position.Latitude

			};

			await AzureManager.AzureManagerInstance.PostHotDogInformation(model);
		}

		static byte[] GetImageAsByteArray(MediaFile file)
		{
			var stream = file.GetStream();
			BinaryReader binaryReader = new BinaryReader(stream);
			return binaryReader.ReadBytes((int)stream.Length);
		}

		async Task MakePredictionRequest(MediaFile file)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add("Prediction-Key", "bf91ca6d1e614b6ead6193761c9ede2d");

			string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/bcb49c89-078b-4a5c-a127-7027e82f974e/image?iterationId=3ced66ed-a8ef-46c7-a28a-db3d9b0118d0";

			HttpResponseMessage response;

			byte[] byteData = GetImageAsByteArray(file);

			using (var content = new ByteArrayContent(byteData))
			{

				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				response = await client.PostAsync(url, content);


				if (response.IsSuccessStatusCode)
				{
					var responseString = await response.Content.ReadAsStringAsync();

					EvaluationModel responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);

					double max = responseModel.Predictions.Max(m => m.Probability);

					TagLabel.Text = (max >= 0.5) ? "NMD" : "NOT NMD";

				}

				//Get rid of file once we have finished using it
				file.Dispose();
			}
		}
	}
}


