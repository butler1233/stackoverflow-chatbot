using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.Services
{
	public class CommandService: ICommandService
	{
		private readonly IRepositoryService repositoryService;

		public CommandService(IRepositoryService repositoryService) =>
			this.repositoryService = repositoryService;

		public Task<string?> AddCommand(CustomCommand command, CancellationToken cancellationToken) =>
			this.repositoryService.Add("Commands", command, cancellationToken);

		public Task<List<CustomCommand>> GetCommands(CancellationToken cancellationToken) =>
			this.repositoryService.GetList<CustomCommand>("Commands", cancellationToken);
	}
}
