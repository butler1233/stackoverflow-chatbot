using System;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.CommandProcessors
{
	internal interface ICommandFactory
	{
		BaseCommand Create(Type commandType, ICommandProcessor priorityProcessor);
	}
}