using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using SharpExchange.Chat.Events;

namespace StackoverflowChatbot.EventProcessors
{
	internal abstract class ProcessorBase: ChatEventDataProcessor, IChatEventHandler<EventData>
	{
		public event Action<EventData> OnEvent;

		public override void ProcessEventData(JToken data)
		{
			if (!IsValid(data)) return;
			this.OnEvent?.Invoke(EventData.FromJson(data));
		}

		private static bool IsValid(JToken data)
		{
			return data.Value<string>("content") != null &&
				data.Value<int>("user_id").Equals(Worker.AdminId) &&
				(
				data.Value<string>("content").StartsWith("Sandy, ")
				|| data.Value<string>("content").StartsWith("@Sandy, ")
				|| data.Value<string>("content").StartsWith("S, ")
				|| data.Value<string>("content").StartsWith("@S, ")
				);
		}
	}
}
