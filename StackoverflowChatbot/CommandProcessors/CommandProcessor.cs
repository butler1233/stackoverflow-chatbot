using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.CommandProcessors
{
	internal abstract class CommandProcessor
	{
		internal abstract string Trigger { get; }

		internal bool KnowsCommand(string trigger) => trigger.ToLower().Equals(this.Trigger);

		internal abstract string ProcessCommand(string command);

	}
}
