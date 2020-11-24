using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StackoverflowChatbot.Services
{
	public interface IRepositoryService
	{
		Task<HashSet<T>> GetList<T>(string collectionName, CancellationToken cancellationToken = default);
		Task<string?> Add<T>(string collectionName, T value, CancellationToken cancellationToken = default);
		void Clear(string collectionName);
		void ClearAll();
	}
}
