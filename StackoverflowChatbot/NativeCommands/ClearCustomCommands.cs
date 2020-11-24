using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	public class ClearCustomCommands: BaseCommand
	{
		private readonly ICommandStore commandStore;
		public ClearCustomCommands(ICommandStore commandStore) => this.commandStore = commandStore;
		internal override string? CommandDescription() => "Clears all the command learned";
		internal override string CommandName() => "clear_commands";
		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters)
		{
			this.commandStore.ClearCommands();
			return new SendMessage("All learned commands cleared.");
		}
	}
}
