using System.Collections.Generic;
using System.Linq;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.CommandProcessors;

namespace StackoverflowChatbot
{
	internal class CommandRouter
	{
		private readonly ICommandProcessor priorityProcessor;
		private readonly ActionScheduler actionScheduler;
		private readonly IReadOnlyCollection<ICommandProcessor> processors;
		public CommandRouter(IRoomService roomService, int roomId, ActionScheduler actionScheduler)
		{
			this.priorityProcessor = new PriorityProcessor(roomService, roomId);
			this.actionScheduler = actionScheduler;
		}
		internal async void RouteCommand(EventData message)
		{
			if (this.priorityProcessor.ProcessCommand(message, out var action) ||
				this.processors.Any(p => p.ProcessCommand(message, out action)))
			{
				await action.Execute(this.actionScheduler);
			}
			else
			{
				await IAction.ExecuteDefaultAction(message, this.actionScheduler);
			}
		}
	}
}
