using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonkeyCache.SQLite;

namespace StackoverflowChatbot.Services.Repositories
{
	public class MonkeyCacheRepositoryService: IRepositoryService
	{
		public MonkeyCacheRepositoryService(string applicationId) =>
			Barrel.ApplicationId = applicationId;//Barrel.Current.EmptyAll();

		public async Task<string?> Add<T>(string name, T value, CancellationToken cancellationToken)
		{
			var list = await this.GetList<T>(name, cancellationToken);
			list.Add(value);
			Barrel.Current.Add(name, list, Timeout.InfiniteTimeSpan);
			return null;
		}

		public Task<List<T>> GetList<T>(string name, CancellationToken cancellationToken)
		{
			var data = Barrel.Current.Get<List<T>>(name);
			return Task.FromResult(data);
		}
	}
}
