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
					//await action.Execute(this.actionScheduler);
					//Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {message.CommandName}");

					//Instance the command, and let it execute.
					var command = (ICommand)Activator.CreateInstance(nativeCommands[commandText]);

					//If it's a admin command and the user isn't an admin, tell them to sod off.
					if (command.NeedsAdmin() && !Config.Manager.Config().Controllers.Contains(message.UserId))
					{
						var response = new SendMessage($":{message.MessageId} YOU'RE NOT MY MOM/DAD *(you don't have permssion to execure that command)*");
						response.Execute(this.actionScheduler);
						Console.WriteLine($"[{message.RoomId}] {message.Username} attempted (unsuccessfully) to invoke {command.GetType().AssemblyQualifiedName}: {commandText}");
					}
					else
					{
						var action = command.ProcessMessage(message, parameters);
						if (action != null)
						{
							await action.Execute(this.actionScheduler);
							//We need to send the response back.
							
						}
						Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {command.GetType().AssemblyQualifiedName}: {commandText}");
					}

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
