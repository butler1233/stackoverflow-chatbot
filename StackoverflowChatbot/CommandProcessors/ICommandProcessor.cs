using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackoverflowChatbot.Actions;

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
		bool ProcessNativeCommand(EventData data, out IAction? action);

		Task<IAction?> ProcessDynamicCommandAsync(EventData data);

		bool TryGetNativeCommands(string key, out Type? value);

		IEnumerable<string> NativeKeys { get; }
	}
}