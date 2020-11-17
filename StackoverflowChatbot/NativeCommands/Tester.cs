using JetBrains.Annotations;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// See if the bot works.
	/// </summary>
	[UsedImplicitly]
	internal class Tester: BaseCommand
	{
		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters) => new SendMessage("Testes. Heh.");

		internal override string CommandName() => "test";

		internal override string? CommandDescription() => null;
		internal override bool NeedsAdmin() => true;
	}
}
