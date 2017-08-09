using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Shoe
{
	public class AzureManager
	{

		private static AzureManager instance;
		private MobileServiceClient client;
        private IMobileServiceTable<ShoeModel> shoedb;

		private AzureManager()
		{
			this.client = new MobileServiceClient("http://shoe.azurewebsites.net");
            this.shoedb = this.client.GetTable<ShoeModel>();
		}

		public MobileServiceClient AzureClient
		{
			get { return client; }
		}

		public static AzureManager AzureManagerInstance
		{
			get
			{
				if (instance == null)
				{
					instance = new AzureManager();
				}

				return instance;
			}
		}

        public async Task<List<ShoeModel>> GetHotDogInformation()
		{
            return await this.shoedb.ToListAsync();
		}
		
        public async Task PostHotDogInformation(ShoeModel ShoeModel)
		{
            await this.shoedb.InsertAsync(ShoeModel);
		}
        public async Task UpdateHotDogInformation(ShoeModel notHotDogModel)
		{
            await this.shoedb.UpdateAsync(notHotDogModel);
		}
        public async Task DeleteHotDogInformation(ShoeModel notHotDogModel)
		{
            await this.shoedb.DeleteAsync(notHotDogModel);
		}

	}
}