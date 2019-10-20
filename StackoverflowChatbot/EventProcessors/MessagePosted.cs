using SharpExchange.Chat.Events;

namespace StackoverflowChatbot.EventProcessors
{
	internal class MessagePosted: ProcessorBase
	{
		public override EventType Event { get; } = EventType.MessagePosted;
	}
}