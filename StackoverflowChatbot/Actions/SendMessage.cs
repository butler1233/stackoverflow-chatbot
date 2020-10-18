using System.Threading.Tasks;
using SharpExchange.Chat.Actions;

namespace StackoverflowChatbot.Actions
{
	internal class SendMessage: IAction
	{
		private readonly string message;

		public SendMessage(string message) => this.message = message;
		public async Task Execute(ActionScheduler scheduler) => await scheduler.CreateMessageAsync(this.message);
	}
}
