using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.CommandProcessors
{
	internal abstract class CommandProcessor
	{
		internal abstract string Trigger { get; }

		internal bool KnowsCommand(string trigger) => string.Equals(trigger, this.Trigger, StringComparison.OrdinalIgnoreCase);

		internal abstract string ProcessCommand(string command);

	}
}
