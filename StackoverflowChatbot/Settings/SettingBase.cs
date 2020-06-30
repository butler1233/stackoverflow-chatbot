using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.Settings
{
	public class SettingBase
	{
		/// <summary>
		/// Trigger words the bot will respond to
		/// </summary>
		public List<string> Triggers { get; set; }

	}
}
