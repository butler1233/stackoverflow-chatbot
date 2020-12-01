using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.Services
{
	public interface ICommandStore
	{
		Task<string?> AddCommand(CustomCommand command, CancellationToken cancellationToken = default);
		Task<HashSet<CustomCommand>> GetCommands(CancellationToken cancellationToken = default);
		void ClearCommands();
	}
}
