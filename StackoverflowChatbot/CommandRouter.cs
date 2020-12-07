using System;
using System.Collections.Generic;
using System.Linq;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.CommandProcessors;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot
{
	internal class CommandRouter
	{
		private readonly ICommandProcessor _priorityProcessor;
		private readonly ActionScheduler _actionScheduler;
		private readonly IReadOnlyCollection<ICommandProcessor> _processors;

		public CommandRouter(IRoomService roomService, ICommandStore commandService, IHttpService httpService, int roomId, ActionScheduler actionScheduler)
		{
			_priorityProcessor = new PriorityProcessor(roomService, commandService, httpService, roomId);
			_actionScheduler = actionScheduler;

			// Populate these with dynamic commands (from a db or something) once that is a thing
			_processors = Array.Empty<ICommandProcessor>();
		}

		internal async void RouteCommand(EventData message)
		{
			//Do other thuings
			try
			{
				if (_priorityProcessor.ProcessCommand(message, out var action) ||
					_processors.Any(p => p.ProcessCommand(message, out action)))
				{
					await action!.Execute(_actionScheduler);
					Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {message.CommandName}");
				}
				else
				{
					// TODO refactor this!
					action = await _priorityProcessor.ProcessCommandAsync(message);
					if (action == null)
					{
						foreach(var processor in _processors)
						{
							action = await processor.ProcessCommandAsync(message);
							if (action != null)
							{
								await action!.Execute(_actionScheduler);
								Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {message.CommandName}");
								break;
							}
						}
					}
					else
					{
						await action!.Execute(_actionScheduler);
						Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {message.CommandName}");
					}
				}
				if (action == null)
				{
					await IAction.ExecuteDefaultAction(message, _actionScheduler);
				}
			}
			catch (Exception e)
			{
				var exceptionMsg = $"    Well thanks, {message.Username}. You broke me. \r\n\r\n" + e;
				var codified = string.Join("\r\n    ", exceptionMsg.Split("\r\n"));
				await _actionScheduler.CreateMessageAsync(codified);
			}

		}
	}
}
