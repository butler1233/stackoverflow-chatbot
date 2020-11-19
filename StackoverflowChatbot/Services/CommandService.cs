using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.Services
{
	public class CommandService: ICommandService
	{
		private readonly IRepositoryService repositoryService;

		public CommandService(IRepositoryService repositoryService) =>
			this.repositoryService = repositoryService;

		private async Task<CollectionReference> Commands()
		{
			var database = await this.repositoryService.Database();
			return database.Collection("Commands");
		}

		public async Task<string> AddCommand(string name, string parameter, CancellationToken cancellationToken)
		{
			var commands = await this.Commands();
			var reference = await commands.AddAsync(new { name, parameter }, cancellationToken);
			return reference.Id;
		}

		public async Task<IList<CustomCommand>> GetCommands(CancellationToken cancellationToken)
		{
			var commands = await this.Commands();
			var reference = commands.StreamAsync(cancellationToken);
			var commandList = new List<CustomCommand>();
			await foreach(var document in reference)
			{
				var command = document.ConvertTo<CustomCommand>();
				commandList.Add(command);
			}
			return commandList;
		}
	}
}
