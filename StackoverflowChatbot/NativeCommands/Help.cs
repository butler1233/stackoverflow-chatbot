using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.CommandProcessors;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Returns information about a specific command or a list of commands.
	/// </summary>
	[UsedImplicitly]
	internal class Help: BaseCommand
	{
		private readonly PriorityProcessor priorityProcessor;
		private readonly ICommandStore commandStore;

		public Help(ICommandStore commandStore, PriorityProcessor priorityProcessor)
		{
			this.commandStore = commandStore;
			this.priorityProcessor = priorityProcessor;
		}

		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters)
		{
			if (parameters?.Length > 0)
			{
				if (this.priorityProcessor.TryGetNativeCommands(parameters[0], out var commandType))
				{
					//We have a command which lines up with what they wanted.
					var command = this.CreateCommandInstance(commandType!);
					return new SendMessage($"`{command.CommandName()}`: *{command.CommandDescription()}*");
				}

				return new SendMessage($"Couldn't find any help for '{parameters[0]}'.");
			}

			//Return a big list of commands.
			var returnable = "All 'native' commands (you can get more by asking me `help <command>`): ";
			returnable += string.Join(", ", this.priorityProcessor.NativeKeys);
			return new SendMessage(returnable.TrimEnd(char.Parse(",")));

		}

		internal override string CommandName() => "help";

		internal override string? CommandDescription() => "Details what commands are available";

		private BaseCommand CreateCommandInstance(Type commandType)
		{
			var a = commandType.GetConstructors()
				.Where(e =>
					e.GetParameters()
					 .Select(p => p.ParameterType)
					 .Contains(typeof(ICommandStore)))
				.Any();
			var b = commandType.GetConstructors()
				.Where(e =>
					e.GetParameters()
					 .Select(p => p.ParameterType)
					 .Contains(typeof(PriorityProcessor)))
				.Any();

			if (a && b)
				return (BaseCommand)Activator.CreateInstance(commandType, this.commandStore, this.priorityProcessor)!;
			if (a)
				return (BaseCommand)Activator.CreateInstance(commandType, this.commandStore)!;
			if (b)
				return (BaseCommand)Activator.CreateInstance(commandType, this.priorityProcessor)!;

			return (BaseCommand)Activator.CreateInstance(commandType)!;
		}
	}
}
