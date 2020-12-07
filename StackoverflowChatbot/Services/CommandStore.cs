using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.Services
{
	public class CommandStore: ICommandStore
	{
		private const string CollectionName = "Commands";
		private readonly IRepositoryService _repositoryService;

		public CommandStore(IRepositoryService repositoryService) =>
			_repositoryService = repositoryService;

		public Task<string?> AddCommand(CustomCommand command, CancellationToken cancellationToken) =>
			_repositoryService.Add(CollectionName, command, cancellationToken);

		public Task<HashSet<CustomCommand>> GetCommands(CancellationToken cancellationToken) =>
			_repositoryService.GetList<CustomCommand>(CollectionName, cancellationToken);

		public void ClearCommands() => _repositoryService.Clear(CollectionName);
	}
}
