using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonkeyCache.SQLite;

namespace StackoverflowChatbot.Services.Repositories
{
	public class MonkeyCacheRepositoryService : IRepositoryService
	{
		public MonkeyCacheRepositoryService(string applicationId) =>
			Barrel.ApplicationId = applicationId;

		public async Task<string?> Add<T>(string name, T value, CancellationToken cancellationToken)
		{
			var list = await this.GetList<T>(name, cancellationToken) ?? new HashSet<T>();
			list.Add(value);
			Barrel.Current.Add(name, list, Timeout.InfiniteTimeSpan);
			// TODO return the inserted id?
			return null;
		}

		public Task<HashSet<T>> GetList<T>(string name, CancellationToken cancellationToken)
		{
			var data = Barrel.Current.Get<HashSet<T>>(name);
			return Task.FromResult(data);
		}

		public void Clear(string collectionName) => Barrel.Current.Empty(collectionName);

		public void ClearAll() => Barrel.Current.EmptyAll();
	}
}
