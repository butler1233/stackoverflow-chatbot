using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StackoverflowChatbot.Services
{
	public interface IRepositoryService
	{
		Task<List<T>> GetList<T>(string name, CancellationToken cancellationToken = default);
		Task<string?> Add<T>(string name, T value, CancellationToken cancellationToken = default);
	}
}
