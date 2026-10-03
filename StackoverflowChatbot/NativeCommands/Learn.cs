using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	internal class Learn: BaseCommand
	{
		private readonly ICommandStore _commandStore;

		public Learn(ICommandStore commandStore) => _commandStore = commandStore;

		internal override async Task<IAction> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters)
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

			try
			{
				await _commandStore.AddCommand(command);
			}
			catch (Exception ex)
			{
				Console.Write(ex);
			}

			return new SendMessage($"Learned the command {name}");
		}

		internal override string CommandName() => "learn";

		internal override string CommandDescription() => "Learns ";
	}
}
