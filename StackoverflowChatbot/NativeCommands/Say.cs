using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Say: ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters) => new SendMessage(string.Join(" ", parameters));

		public string CommandName() => "say";

		public string? CommandDescription() => "Says whatever you tell him to say";
		public bool NeedsAdmin() => false;
	}
}
