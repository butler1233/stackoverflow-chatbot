using System;
using System.Collections.Generic;
using System.Text;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.CommandProcessors;

namespace StackoverflowChatbot
{
	internal class CommandRouter
	{
		private readonly PriorityProcessor priorityProcessor;
		private readonly IRoomService roomService;
		private readonly ActionScheduler actionScheduler;
		private readonly int roomId;
		private readonly IReadOnlyCollection<CommandProcessor> processors;
		public CommandRouter(IRoomService roomService, int roomId, ActionScheduler actionScheduler)
		{
			this.roomService = roomService;
			this.priorityProcessor = new PriorityProcessor(this.roomService, roomId);
			this.roomId = roomId;
			this.actionScheduler = actionScheduler;
		}
		internal async void RouteCommand(EventData message)
		{
			if (this.priorityProcessor.ProcessCommand(message, out var action))
			{
				_ = await this.actionScheduler.CreateMessageAsync(action.Message);
			}
		}
	}
}
