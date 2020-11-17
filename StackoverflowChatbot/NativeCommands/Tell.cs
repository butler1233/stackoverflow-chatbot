using System.Linq;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Tells someone something.
	/// </summary>
	[UsedImplicitly]
	internal class Tell: BaseCommand
	{
		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters) => new SendMessage(parameters?.Any() == true ? $"@{parameters[0]}, {string.Join(" ", parameters.Skip(1))}" : "No.");

		internal override string CommandName() => "tell";

		internal override string? CommandDescription() => "Says whatever you tell him to say";
	}
}
