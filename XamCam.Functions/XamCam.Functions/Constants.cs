﻿using System;

namespace XamCam.Functions
{
	public static class Constants
	{
		public static class IotHubConfig
		{
            //BLOB STORAGE STRINGS
            public static string BlobURLAndKey = Environment.GetEnvironmentVariable("BlobURLAndKey");
            public static string DirectUploadBlobURLAndKey = Environment.GetEnvironmentVariable("DirectUploadBlobURLAndKey");

            //COSMOS DB STRING
            public const string CosmosDBEndPoint = "https://xamcamcosmosdb.documents.azure.com:443/";
            public static string CosmosDBMyKey = Environment.GetEnvironmentVariable("CosmosDBMyKey");

            //CONSTANTS NEEDED FOR AZURE AD
            public static string tenantId = Environment.GetEnvironmentVariable("tenantId");
            public static string GrantType = Environment.GetEnvironmentVariable("GrantType");
            public static string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
            public static string ClientID = Environment.GetEnvironmentVariable("ClientID");
            public static string RequestedResource = "https://rest.media.azure.net";
            public static string MediaServiceRestEndpoint = "https://xamcammediaservice.restv2.westus.media.azure.net/api/";

            //BLOB STORAGE ACCOUNT
            public static string StorageAccountName = "xamcamstorage";
            public static string StorageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
            public static string BlobStorageAccountConnection = Environment.GetEnvironmentVariable("BlobStorageAccountConnection");

            //WEBHOOK AND SIGNING KEY
            public static string WebHookEndpoint = Environment.GetEnvironmentVariable("WebHookEndpoint");
            public static string WebHookSigningKey = Environment.GetEnvironmentVariable("WebHookSigningKey");

            //AMS CONSTANTS WITH URL
            public static string AMSUrlWithSlash = "http://xamcammediaservice.streaming.mediaservices.windows.net/";

            public static string HostName = "HomeCam-IoT.azure-devices.net";
			public static string IotHubD2CEndpoint = "messages/events";
            public static string ConnectionString = Environment.GetEnvironmentVariable("IoTHubConnectionString");
                //$"HostName={HostName};SharedAccessKeyName=iothubowner;SharedAccessKey=SF7KzXbqc+zq0l7YyVtvyQI2KR9OsGrEqzaXLWwq86c=";
            public static int MAX_DEVICE_LIST = 50;
		}
	}
}