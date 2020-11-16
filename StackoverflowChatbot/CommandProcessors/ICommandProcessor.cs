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
		bool ProcessCommand(EventData data, out IAction? action);
	}
}