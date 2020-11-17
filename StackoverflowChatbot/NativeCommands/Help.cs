using System;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.CommandProcessors;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Returns information about a specific command or a list of commands.
	/// </summary>
	[UsedImplicitly]
	internal class Help: BaseCommand
	{
		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters)
		{
			if (parameters?.Length > 0)
			{
				if (PriorityProcessor.NativeCommands.TryGetValue(parameters[0], out var commandType))
				{
					//We have a command which lines up with what they wanted.
					var command = (BaseCommand)Activator.CreateInstance(commandType)!;
					return new SendMessage($"`{command.CommandName()}`: *{command.CommandDescription()}*");
				}

				return new SendMessage($"Couldn't find any help for '{parameters[0]}'.");
			}

			//Return a big list of commands.
			var returnable = "All 'native' commands (you can get more by asking me `help <command>`): ";
			returnable += string.Join(", ", PriorityProcessor.NativeCommands.Keys);
			return new SendMessage(returnable.TrimEnd(char.Parse(",")));

		}

		internal override string CommandName() => "help";

		internal override string? CommandDescription() => "Details what commands are available";
	}
}
