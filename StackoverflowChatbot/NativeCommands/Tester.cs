using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Tester : ICommand	
	{
		public string? ProcessMessage(EventData eventContext, string[] parameters) => "Testes. Heh.";

		public string CommandName() => "test";

		public string? CommandDescription() => null;
	}
}
