using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public CommandRouter(ICommandStore commandService, IHttpService httpService, ICommandFactory commandFactory, ActionScheduler actionScheduler)
		{
			_priorityProcessor = new PriorityProcessor(commandService, httpService, commandFactory);
			_actionScheduler = actionScheduler;

			// Populate these with dynamic commands (from a db or something) once that is a thing
			_processors = Array.Empty<ICommandProcessor>();
		}

		internal async void RouteCommand(EventData message)
		{
			//Do other thuings
			try
			{
				var invokableAction = await FindInvokableAction(message);
				if (invokableAction != null)
				{
					await invokableAction.Execute(_actionScheduler);
					Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {message.CommandName}");
				}
				else
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

		private async Task<IAction?> FindInvokableAction(EventData message)
		{
			if (_priorityProcessor.ProcessNativeCommand(message, out var action) ||
			    _processors.Any(p => p.ProcessNativeCommand(message, out action)))
			{
				return action;
			}

			action = await _priorityProcessor.ProcessDynamicCommandAsync(message);
			if (action != null)
			{
				return action;
			}

			foreach (var processor in _processors)
			{
				action = await processor.ProcessDynamicCommandAsync(message);
				if (action != null)
				{
					return action;
				}
			}

			return null;
		}
	}
}
