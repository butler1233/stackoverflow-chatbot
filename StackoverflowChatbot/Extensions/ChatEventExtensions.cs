using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Botler.Core.Config;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.Extensions
{
	internal static class ChatEventExtensions
	{
        
		internal static bool ContainsTrigger(this ChatMessageEventData chatEvent) => Manager.Config().Triggers.Any(s => chatEvent.Content.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) == 0); //Fairly certain this could be written better but I'm tired.
	}
}
