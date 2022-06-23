using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.CommandProcessors
{
	internal interface ICommandProcessor
	{
		/// <summary>
		/// Try to process the command into an action.
		/// </summary>
		/// <param name="data">The command.</param>
		/// <param name="action">Executable action on success, otherwise null.</param>
		/// <returns>Whether or not this processor could process the command.</returns>
		bool ProcessNativeCommand(ChatMessageEventData data, out IAction? action);

		Task<IAction?> ProcessDynamicCommandAsync(ChatMessageEventData data);

		bool TryGetNativeCommands(string key, out Type? value);

		IEnumerable<string> NativeKeys { get; }
	}
}