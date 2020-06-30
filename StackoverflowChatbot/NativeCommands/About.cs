using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.NativeCommands
{
	internal class About : ICommand
	{
		public string? ProcessMessage(EventData eventContext, string[] parameters)
		{
			return
				"    Lee Botler: A bot for C# which probably won't work. \r\n    Written by CaptainObvious, based originally on Sandy, by SquirrelKiller. ";
		}

		public string CommandName() => "about";

		public string? CommandDescription() => "Tells you about the bot.";
	}
}
