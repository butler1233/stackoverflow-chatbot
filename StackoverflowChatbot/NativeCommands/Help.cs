using System;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Help: ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters)
		{
			if (parameters.Length > 0)
			{
				if (CommandRouter.nativeCommands.ContainsKey(parameters[0]))
				{
					//We have a command which lines up with what they wanted.
					var command = (ICommand)Activator.CreateInstance(CommandRouter.nativeCommands[parameters[0]]);
					return new SendMessage($"`{command.CommandName()}`: *{command.CommandDescription()}*");
				}
				else
				{
					return new SendMessage($"Couldn't find any help for '{parameters[0]}'.");
				}
			}
			else
			{
				//Return a big list of commands.
				var returnable = "All 'native' commands (you can get more by asking me `help <command>`): ";
				foreach (var nativeCommand in CommandRouter.nativeCommands)
				{
					returnable += $"{nativeCommand.Key}, ";
				}

				return new SendMessage(returnable.TrimEnd(char.Parse(",")));
			}

		}

		public string CommandName() => "help";

		public string? CommandDescription() => "Details what commands are available";
	}
}
