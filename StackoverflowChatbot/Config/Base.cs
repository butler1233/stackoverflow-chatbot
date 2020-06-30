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
		public string[] Triggers { get; set; }

		/// <summary>
		/// users who are allowed to control the bot
		/// </summary>
		public int[] Controllers { get; set; }
	}
}
