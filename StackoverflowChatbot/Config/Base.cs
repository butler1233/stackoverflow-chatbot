using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.Config
{
	internal class Base
	{

		/// <summary>
		/// Trigger phrases for the bot
		/// </summary>
		public List<string> Triggers { get; set; }

		/// <summary>
		/// users who are allowed to control the bot
		/// </summary>
		public List<int> Controllers { get; set; }

		/// <summary>
		/// Token for logging in yo discord and doing things.
		/// </summary>
		public string DiscordToken { get; set; }

		public Dictionary<int,string> StackToDiscordMap { get; set; }

		public Dictionary<string,int> DiscordToStackMap { get; set; }

		public string DiscordInviteLink { get; set; }
	}
}
