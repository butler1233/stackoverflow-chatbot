using System;
using System.Collections.Generic;
using System.Text;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class DiscordIntegration : ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters) {

			throw new NotImplementedException();
		}

		public string CommandName() => "DiscordIntegration";

		public string? CommandDescription() => "Controls integration with discord.";

		public bool NeedsAdmin() => true;
	}
}
