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
		private readonly ICommandProcessor _commandProcessor;
		private readonly ICommandFactory _commandFactory;

		public Help(ICommandProcessor commandProcessor, ICommandFactory commandFactory)
		{
			_commandProcessor = commandProcessor;
			_commandFactory = commandFactory;
		}

		internal override IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters)
		{
			if (parameters?.Length > 0)
			{
				if (_commandProcessor.TryGetNativeCommands(parameters[0], out var commandType))
				{
					//We have a command which lines up with what they wanted.
					var command = _commandFactory.Create(commandType!, _commandProcessor);
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
	}
}
