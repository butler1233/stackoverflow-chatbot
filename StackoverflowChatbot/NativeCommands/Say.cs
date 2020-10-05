using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Say: ICommand
	{
		public string? ProcessMessage(EventData eventContext, string[] parameters)
		{
			return string.Join(" ",parameters);
		}

		public string CommandName() => "say";

		public string? CommandDescription() => "Says whatever you tell him to say";
	}
}
