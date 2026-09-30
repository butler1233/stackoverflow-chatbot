using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Tells someone something.
	/// </summary>
	[UsedImplicitly]
	internal class Tell: BaseCommand
	{
		internal override Task<IAction> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) => Task.FromResult<IAction>(new SendMessage(parameters?.Any() == true ? $"@{parameters[0]}, {string.Join(" ", parameters.Skip(1))}" : "No."));

		internal override string CommandName() => "tell";

		internal override string CommandDescription() => "Says whatever you tell him to say";
	}
}
