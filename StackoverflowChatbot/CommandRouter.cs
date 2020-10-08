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

		private static readonly IDictionary<string, Type> nativeCommands = new Dictionary<string, Type>();

		public CommandRouter(IRoomService roomService, int roomId, ActionScheduler actionScheduler)
		{
			this.priorityProcessor = new PriorityProcessor(roomService, roomId);
			this.actionScheduler = actionScheduler;
			this.processors = Array.Empty<ICommandProcessor>();

			this.ReloadCommands();
		}

		internal async void RouteCommand(EventData message)
		{
			try
			{
				var commandName = message.CommandName.ToLower();
				var parameters = message.CommandParameters.Split(" ").Select(HttpUtility.HtmlDecode).ToArray();
				if (nativeCommands.ContainsKey(commandName))
				{
					//Instance the command, and let it execute.
					var command = (ICommand)Activator.CreateInstance(nativeCommands[commandName]);
					var action = command.ProcessMessage(message, parameters);
					if (action != null)
					{
						await action.Execute(this.actionScheduler);
						//We need to send the response back.
						Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {command.GetType().AssemblyQualifiedName}: {commandName}");
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
				var exceptionMsg = $"    Well thanks, {message.Username}. You broke me. \r\n\r\n" + e.ToString();
				var codified = string.Join("\r\n    ", exceptionMsg.Split("\r\n"));
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
