using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackoverflowChatbot.ChatEvents.StackOverflow;
using StackoverflowChatbot.Config;

namespace StackoverflowChatbot.Extensions
{
	internal static class ChatEventExtensions
	{
        
		internal static bool ContainsTrigger(this ChatMessageEventData chatEvent) => Manager.Config().Triggers.Any(s => chatEvent.Content.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) == 0); //Fairly certain this could be written better but I'm tired.
	}
}
