using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Shoe.Model;
using Newtonsoft.Json;
using System.Linq;

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

		static byte[] GetImageAsByteArray(MediaFile file)
		{
			var stream = file.GetStream();
			BinaryReader binaryReader = new BinaryReader(stream);
			return binaryReader.ReadBytes((int)stream.Length);
		}

		async Task MakePredictionRequest(MediaFile file)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add("Prediction-Key", "a07f0964f70b4d8580776bd4aaa47660");

			string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/2dcf6f01-36f8-44a9-bb23-1bb514da9011/image?iterationId=c5dc476d-5194-4ce6-897e-b5a98ee32e66";

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

					TagLabel.Text = (max >= 0.5) ? "Nmd" : "Not Nmd";

				}

				//Get rid of file once we have finished using it
				file.Dispose();
			}
		}
	}
}