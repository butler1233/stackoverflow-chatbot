using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.Actions
{
	internal class SendMessage
	{
		internal readonly string Message;

		public SendMessage(string message) => this.Message = message;
	}
}
