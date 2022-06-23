using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	public class ClearCustomCommands: BaseCommand
	{
		private readonly ICommandStore _commandStore;
		public ClearCustomCommands(ICommandStore commandStore) => _commandStore = commandStore;
		internal override string? CommandDescription() => "Clears all the command learned";
		internal override string CommandName() => "clear_commands";
		internal override IAction? ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters)
		{
			_commandStore.ClearCommands();
			return new SendMessage("All learned commands cleared.");
		}
	}
}
