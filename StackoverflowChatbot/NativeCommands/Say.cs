using System.Threading.Tasks;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Just returns what you tell it to say.
	/// </summary>
	[UsedImplicitly]
	internal class Say: BaseCommand
	{
		internal override Task<IAction> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) => Task.FromResult<IAction>(new SendMessage(parameters != null ? string.Join(" ", parameters) : ""));

		internal override string CommandName() => "say";

		internal override string CommandDescription() => "Says whatever you tell him to say";
	}
}
