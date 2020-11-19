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
		Task<string> AddCommand(string name, string parameter, CancellationToken cancellationToken = default);
		Task<IList<CustomCommand>> GetCommands(CancellationToken cancellationToken = default);
	}
}
