using SharpExchange.Chat.Events;

namespace StackoverflowChatbot.EventProcessors
{
	internal class MessageEdited: ProcessorBase
	{
		public override EventType Event { get; } = EventType.MessageEdited;
	}
}
