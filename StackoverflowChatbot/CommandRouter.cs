using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

		private readonly IDictionary<string, Type> nativeCommands;

		public CommandRouter(IRoomService roomService, int roomId, ActionScheduler actionScheduler)
		{
			this.priorityProcessor = new PriorityProcessor(roomService, roomId);
			this.actionScheduler = actionScheduler;
			this.processors = new ICommandProcessor[0];

			this.nativeCommands = new Dictionary<string, Type>();
			this.ReloadCommands();
		}

		internal async void RouteCommand(EventData message)
		{
			var commandParts = message.Content.Split(" "); //We need to split this properly to account for "strings in quotes" being treated proeprly. Soon tho.
			var commandText = commandParts[1].ToLower(); //Part 0 will be the trigger word.
			var parameters = commandParts.Skip(2).ToArray();
			if (this.nativeCommands.ContainsKey(commandText)) //ToLower is bad I know.
			{
				//Instance the command, and let it execute.
				var command = (ICommand) Activator.CreateInstance(this.nativeCommands[commandText]);
				var response = command.ProcessMessage(message, parameters);
				if (response != null)
				{
					//We need to send the response back.
					await this.actionScheduler.CreateReplyAsync(response, message.MessageId);
					Console.WriteLine($"[{message.RoomId}] {message.Username} invoked {command.GetType().Assembly.FullName}.{command.GetType().Name}: {response}");
				}
			}else {

				message

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

		/// <summary>
		/// Loads in all the implementations of ICommand in the domain.
		/// </summary>
		internal void ReloadCommands()
		{
			this.nativeCommands.Clear();
			var commandInterface = typeof(ICommand);
			var implementers = AppDomain.CurrentDomain.GetAssemblies().SelectMany((assembly) =>
				assembly.GetTypes().Where(x => commandInterface.IsAssignableFrom(x) && !x.IsInterface));
			foreach (var implementer in implementers)
			{
				var instance = (ICommand) Activator.CreateInstance(implementer);
				this.nativeCommands.Add(instance.CommandName().ToLower(), implementer);
				Console.WriteLine($"Loaded command {instance.CommandName()} from type {implementer.Name} from {implementer.Assembly.FullName}");
			}
		}
	}
}
