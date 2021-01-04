using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	internal class Learn: BaseCommand
	{
		private readonly ICommandStore _commandStore;

		public Learn(ICommandStore commandStore) => _commandStore = commandStore;

		internal override IAction ProcessMessageInternal(EventData eventContext, string[]? parameters)
		{
			if (parameters == null || parameters.Length < 2)
			{
				return new SendMessage("Missing args");
			}

			var name = parameters[0];
			var args = parameters[1];
			var command = new CustomCommand(name, args);
			if (DynamicCommand.TryParse(args, out var dynamicCommand))
			{
				command.IsDynamic = true;
				command.ExpectedDynamicCommandArgs = dynamicCommand!.ExpectedArgsCount;
			}

			_ = _commandStore.AddCommand(command)
				.ContinueWith(t =>
				{
					if (!t.IsFaulted)
						return;
					Exception? exception = t.Exception;
					while (exception is AggregateException aggregateException)
						exception = aggregateException.InnerException;
					Console.Write(exception);
				});
			return new SendMessage($"Learned the command {name}");
		}

		internal override string CommandName() => "learn";

		internal override string CommandDescription() => "Learns ";
	}
}
