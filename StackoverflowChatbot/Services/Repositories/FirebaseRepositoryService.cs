using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Google.Cloud.Storage.V1;

namespace StackoverflowChatbot.Services.Repositories
{
	public class FirebaseRepositoryService : IRepositoryService
	{
		private bool _authenticated;
		private readonly string _projectId;
		private FirestoreDb? _database;

		public FirebaseRepositoryService(string projectId) => _projectId = projectId;

		private static Task<string> GetJsonCredentialService() =>
			File.ReadAllTextAsync("so-chatbot-firestore-key.json");

		private void Authenticate(string projectId, string jsonData)
		{
			if (_authenticated)
				return;

			var credential = GoogleCredential.FromJson(jsonData);
			var storage = StorageClient.Create(credential);
			var buckets = storage.ListBuckets(projectId);
			Console.WriteLine("Listing all authenticated buckets...");
			foreach (var bucket in buckets)
			{
				Console.WriteLine(bucket.Name);
			}
			_authenticated = true;
		}

		private async Task<FirestoreDb> Database()
		{
			if (_database != null)
				return _database;

			var jsonCredential = await GetJsonCredentialService();
			Authenticate(_projectId, jsonCredential);
			var builder = new FirestoreClientBuilder
			{
				JsonCredentials = jsonCredential
			};
			_database = await FirestoreDb.CreateAsync(_projectId, await builder.BuildAsync());
			return _database;
		}

		private async Task<CollectionReference> Collection(string name)
		{
			var database = await Database();
			return database.Collection(name);
		}

		public async Task<HashSet<T>> GetList<T>(string name, CancellationToken cancellationToken)
		{
			var collection = await Collection(name);
			var snapshot = await collection.GetSnapshotAsync(cancellationToken);
			return new HashSet<T>(snapshot.Documents.Select(e => e.ConvertTo<T>()));
		}

		public async Task<string?> Add<T>(string name, T value, CancellationToken cancellationToken)
		{
			var collection = await Collection(name);
			var reference = await collection.AddAsync(value, cancellationToken);
			return reference?.Id;
		}

		public void Clear(string collectionName) => throw new NotImplementedException(nameof(Clear));
		public void ClearAll() => throw new NotImplementedException(nameof(ClearAll));

		// NOTE for testing only
		public async Task Stupid()
		{
			var jsonCredential = await GetJsonCredentialService();
			Authenticate(_projectId, jsonCredential);
			var builder = new FirestoreClientBuilder
			{
				JsonCredentials = jsonCredential
			};
			var db = await FirestoreDb.CreateAsync(_projectId, await builder.BuildAsync());
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
