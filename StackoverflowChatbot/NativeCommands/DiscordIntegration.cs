using System;
using System.Collections.Generic;
using System.Text;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class DiscordIntegration : BaseCommand
	{
		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters) {

			throw new NotImplementedException();
		}

		internal override string CommandName() => "DiscordIntegration";

		internal override string? CommandDescription() => "Controls integration with discord.";

		internal override bool NeedsAdmin() => true;
	}
}
