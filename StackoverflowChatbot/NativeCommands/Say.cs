using JetBrains.Annotations;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Just returns what you tell it to say.
	/// </summary>
	[UsedImplicitly]
	internal class Say: BaseCommand
	{
		internal override IAction? ProcessMessageInternal(EventData eventContext, string[] parameters) => new SendMessage(string.Join(" ", parameters));

		internal override string CommandName() => "say";

		internal override string? CommandDescription() => "Says whatever you tell him to say";
	}
}
