using System.Threading.Tasks;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class EditMessageAction: IAction
	{
		private readonly int messageId;
		private readonly string newContent;

		public EditMessageAction(int messageId, string newContent)
		{
			this.messageId = messageId;
			this.newContent = newContent;
		}

		public async Task Execute(ActionScheduler scheduler) => await scheduler.EditMessageAsync(messageId, newContent);
	}
}
