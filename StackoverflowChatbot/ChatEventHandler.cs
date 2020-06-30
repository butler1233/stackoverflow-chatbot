using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;

namespace StackoverflowChatbot
{
	internal class ChatEventHandler: ChatEventDataProcessor
	{
		public override EventType Event { get; } = EventType.MessagePosted | EventType.MessageEdited;

		public override void ProcessEventData(JToken data)
		{
			if (!IsValid(data)) return;

			var eventData = EventData.FromJson(data);
			// Send to command processor. / Maybe some preliminary master-processor first?
		}

		private static bool IsValid(JToken data)
		{
			return data.Value<string>("content") != null &&
				(Program.Settings.Triggers.Any(x => data.Value<string>("content").StartsWith(x)));
		}

		public override string ToString() => base.ToString();
	}
}
