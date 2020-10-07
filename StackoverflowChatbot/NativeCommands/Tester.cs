using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Tester: ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters) => new SendMessage("Testes. Heh.");

		public string CommandName() => "test";

		public string? CommandDescription() => null;
	}
}
