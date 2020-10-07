using System.Linq;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Tell: ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters) => new SendMessage($"@{parameters[0]}, {string.Join(" ", parameters.Skip(1))}");

		public string CommandName() => "tell";

		public string? CommandDescription() => "Says whatever you tell him to say";
	}
}
