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
		private readonly ICommandProcessor _commandProcessor;
		private readonly ICommandStore _commandStore;

		public Help(ICommandStore commandStore, ICommandProcessor commandProcessor)
		{
			_commandStore = commandStore;
			_commandProcessor = commandProcessor;
		}

		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters)
		{
			if (parameters?.Length > 0)
			{
				if (_commandProcessor.TryGetNativeCommands(parameters[0], out var commandType))
				{
					//We have a command which lines up with what they wanted.
					var command = CreateCommandInstance(commandType!);
					return new SendMessage($"`{command.CommandName()}`: *{command.CommandDescription()}*");
				}

				return new SendMessage($"Couldn't find any help for '{parameters[0]}'.");
			}

			//Return a big list of commands.
			var returnable = "All 'native' commands (you can get more by asking me `help <command>`): ";
			returnable += string.Join(", ", _commandProcessor.NativeKeys);
			return new SendMessage(returnable.TrimEnd(char.Parse(",")));

		}

		internal override string CommandName() => "help";

		internal override string? CommandDescription() => "Details what commands are available";

		private BaseCommand CreateCommandInstance(Type commandType)
		{
			var parameterTypes = commandType
				.GetConstructors()
				.First()
				.GetParameters()
				.Select(e => e.ParameterType);

			var parameterValues = new List<object>();
			foreach (var param in parameterTypes)
			{
				if (param == typeof(ICommandStore))
					parameterValues.Add(_commandStore);
				else if (param == typeof(ICommandProcessor))
					parameterValues.Add(_commandProcessor);
			}

			if (parameterValues.Any())
				return (BaseCommand)Activator.CreateInstance(commandType, parameterValues)!;

			return (BaseCommand)Activator.CreateInstance(commandType)!;
		}
	}
}
