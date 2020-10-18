using System.Collections.Generic;

namespace StackoverflowChatbot.Config
{
	internal class Base
	{

		/// <summary>
		/// Trigger phrases for the bot
		/// </summary>
		public List<string> Triggers { get; set; } = null!;

		/// <summary>
		/// users who are allowed to control the bot
		/// </summary>
		public List<int> Controllers { get; set; } = null!;
	}
}
