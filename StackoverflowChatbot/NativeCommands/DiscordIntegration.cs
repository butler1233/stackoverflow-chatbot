using System;
using System.Collections.Generic;
using System.Text;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	internal class DiscordIntegration : BaseCommand
	{
		internal override IAction ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) {

			throw new NotImplementedException();
		}

		internal override string CommandName() => "DiscordIntegration";

		internal override string CommandDescription() => "Controls integration with discord.";

		internal override bool NeedsAdmin() => true;
	}
}
