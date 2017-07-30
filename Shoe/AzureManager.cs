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

        public async Task<List<ShoeModel>> GetShoeInformation()
		{
            return await this.shoedb.ToListAsync();
		}
	}
}