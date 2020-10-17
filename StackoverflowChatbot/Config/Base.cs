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
	}
}
