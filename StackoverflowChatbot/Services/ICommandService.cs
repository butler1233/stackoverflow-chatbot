using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.Services
{
	public interface ICommandService
	{
		Task<string?> AddCommand(CustomCommand command, CancellationToken cancellationToken = default);
		Task<List<CustomCommand>> GetCommands(CancellationToken cancellationToken = default);
	}
}
