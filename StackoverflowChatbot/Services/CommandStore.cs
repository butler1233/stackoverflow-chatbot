using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.Services
{
	public class CommandStore: ICommandStore
	{
		private const string CollectionName = "Commands";
		private readonly IRepositoryService repositoryService;

		public CommandStore(IRepositoryService repositoryService) =>
			this.repositoryService = repositoryService;

		public Task<string?> AddCommand(CustomCommand command, CancellationToken cancellationToken) =>
			this.repositoryService.Add(CollectionName, command, cancellationToken);

		public Task<HashSet<CustomCommand>> GetCommands(CancellationToken cancellationToken) =>
			this.repositoryService.GetList<CustomCommand>(CollectionName, cancellationToken);

		public void ClearCommands() => this.repositoryService.Clear(CollectionName);
	}
}
