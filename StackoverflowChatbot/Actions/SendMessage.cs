using System.Threading.Tasks;
using SharpExchange.Chat.Actions;

namespace StackoverflowChatbot.Actions
{
	internal class SendMessage: IAction
	{
		internal readonly string Message;

		public SendMessage(string message) => this.Message = message;
		public async Task Execute(ActionScheduler scheduler) => await scheduler.CreateMessageAsync(this.Message);
	}
}
