using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.CommandProcessors;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot
{
	internal class CommandRouter
	{
		private readonly ICommandProcessor priorityProcessor;
		private readonly ActionScheduler actionScheduler;
		private readonly IReadOnlyCollection<ICommandProcessor> processors;

		internal static readonly IDictionary<string, Type> nativeCommands = new Dictionary<string, Type>();

		public CommandRouter(IRoomService roomService, int roomId, ActionScheduler actionScheduler)
		{
			this.priorityProcessor = new PriorityProcessor(roomService, roomId);
			this.actionScheduler = actionScheduler;
			this.processors = new ICommandProcessor[0];

			this.ReloadCommands();
		}

		internal async void RouteCommand(EventData message)
		{
			try
			{
				var commandParts = message.Content.Split(" "); //We need to split this properly to account for "strings in quotes" being treated properly. Soon tho.
				var commandText = commandParts[1].ToLower(); //Part 0 will be the trigger word.
				var parameters = commandParts.Skip(2).Select(c => HttpUtility.HtmlDecode(c)).ToArray();
				if (nativeCommands.ContainsKey(commandText))
				{
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
			catch (Exception e)
			{
				string exceptionMsg = $"    Well thanks, {message.Username}. You broke me. \r\n\r\n" + e.ToString();
				string codified = string.Join("\r\n    ", exceptionMsg.Split("\r\n"));
				await this.actionScheduler.CreateMessageAsync(codified);
			}

		}

		/// <summary>
		/// Loads in all the implementations of ICommand in the domain.
		/// </summary>
		internal void ReloadCommands()
		{
			nativeCommands.Clear();
			var commandInterface = typeof(ICommand);
			var implementers = AppDomain.CurrentDomain.GetAssemblies().SelectMany((assembly) =>
				assembly.GetTypes().Where(x => commandInterface.IsAssignableFrom(x) && !x.IsInterface));
			foreach (var implementer in implementers)
			{
				var instance = (ICommand)Activator.CreateInstance(implementer);
				nativeCommands.Add(instance.CommandName().ToLower(), implementer);
				Console.WriteLine($"Loaded command {instance.CommandName()} from type {implementer.Name} from {implementer.Assembly.FullName}");
			}
		}
	}
}
