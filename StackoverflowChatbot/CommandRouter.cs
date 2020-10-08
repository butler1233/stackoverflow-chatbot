using System;
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

			// Populate these with dynamic commands (from a db or something) once that is a thing
			this.processors = Array.Empty<ICommandProcessor>();
		}

		internal async void RouteCommand(EventData message)
		{
			try
			{
				if (this.priorityProcessor.ProcessCommand(message, out var action) ||
					this.processors.Any(p => p.ProcessCommand(message, out action)))
				{
					await action.Execute(this.actionScheduler);
					Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {message.CommandName}");
				}
				else
				{
					await IAction.ExecuteDefaultAction(message, this.actionScheduler);
				}
			}
			catch (Exception e)
			{
				var exceptionMsg = $"    Well thanks, {message.Username}. You broke me. \r\n\r\n" + e.ToString();
				var codified = string.Join("\r\n    ", exceptionMsg.Split("\r\n"));
				await this.actionScheduler.CreateMessageAsync(codified);
			}

		}
	}
}
