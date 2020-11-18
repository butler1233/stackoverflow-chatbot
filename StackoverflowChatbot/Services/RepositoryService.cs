using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Google.Cloud.Storage.V1;

namespace StackoverflowChatbot.Services
{
	public class RepositoryService : IRepositoryService
	{
		private bool authenticated;
		private readonly string projectId;
		private FirestoreDb? database;

		public RepositoryService(string projectId) => this.projectId = projectId;

		private static Task<string> GetJsonCredentialService() =>
			File.ReadAllTextAsync("so-chatbot-firestore-key.json");

		private void Authenticate(string projectId, string jsonData)
		{
			if (this.authenticated)
				return;

			var credential = GoogleCredential.FromJson(jsonData);
			var storage = StorageClient.Create(credential);
			var buckets = storage.ListBuckets(projectId);
			Console.WriteLine("Listing all authenticated buckets...");
			foreach (var bucket in buckets)
			{
				Console.WriteLine(bucket.Name);
			}
			this.authenticated = true;
		}

		public async Task<FirestoreDb> Database()
		{
			if (this.database != null)
				return this.database;

			var jsonCredential = await GetJsonCredentialService();
			this.Authenticate(this.projectId, jsonCredential);
			var builder = new FirestoreClientBuilder
			{
				JsonCredentials = jsonCredential
			};
			this.database = FirestoreDb.Create(this.projectId, builder.Build());
			return this.database;
		}

		public async Task Stupid()
		{
			var jsonCredential = await GetJsonCredentialService();
			this.Authenticate(this.projectId, jsonCredential);
			var builder = new FirestoreClientBuilder();
			builder.JsonCredentials = jsonCredential;
			var db = FirestoreDb.Create(this.projectId, builder.Build());
			var collection = db.Collection("Commands");

			var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var snapshot = await collection.GetSnapshotAsync(cts.Token);
			foreach(var document in snapshot.Documents)
			{
				var name = document.GetValue<string>("name");
				var value = document.GetValue<string>("parameter");
				Console.WriteLine($"name: {name}, value: {value}");
			}
		}
	}
}
