using System;
using System.Threading.Tasks;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.Actions
{
	public interface IAction
	{
		/// <summary>
		/// Call this in case of an unknown command.
		/// </summary>
		public static Func<ChatMessageEventData, ActionScheduler, Task> ExecuteDefaultAction =
			async (data, scheduler) => await scheduler.CreateReplyAsync("Sorry, I don't know that one.", data.MessageId);

		/// <summary>
		/// Execute this command.
		/// </summary>
		/// <param name="scheduler">Used to schedule possible messages.</param>
		/// <returns>Awaitable <seealso cref="Task"/> that runs until the command is sent.</returns>
		Task Execute(ActionScheduler scheduler);
	}
}