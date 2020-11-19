using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace StackoverflowChatbot.Relay
{
	internal class DiscordMessageModel
	{
		public string MessageContent { get; set; }

		public Embed Embed { get; set; } = null;

	}
}
