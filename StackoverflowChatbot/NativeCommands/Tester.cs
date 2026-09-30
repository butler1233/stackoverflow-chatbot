using System.Threading.Tasks;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// See if the bot works.
	/// </summary>
	[UsedImplicitly]
	internal class Tester: BaseCommand
	{
		internal override Task<IAction> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) => Task.FromResult<IAction>(new SendMessage("Testes. Heh."));

		internal override string CommandName() => "test";

		internal override string? CommandDescription() => null;
		internal override bool NeedsAdmin() => true;
	}
}
