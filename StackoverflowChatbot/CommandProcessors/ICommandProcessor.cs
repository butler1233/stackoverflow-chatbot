using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.CommandProcessors
{
	internal interface ICommandProcessor
	{
		bool ProcessCommand(EventData data, out IAction action);
	}
}