using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Tell: ICommand
	{
		public string? ProcessMessage(EventData eventContext, string[] parameters)
		{
			return $"@{parameters[0]}, {string.Join(" ", parameters.Skip(1))}";
		}

		public string CommandName() => "tell";

		public string? CommandDescription() => "Says whatever you tell him to say";
	}
}
