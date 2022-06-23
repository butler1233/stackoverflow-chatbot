using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Returns information about this bot.
	/// </summary>
	[UsedImplicitly]
	internal class About: BaseCommand
	{
		internal override IAction ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) =>
			new SendMessage(
				"    Lee Botler: A bot for C# which probably won't work. \r\n    Written by CaptainObvious, based originally on Sandy, by SquirrelKiller. ");

		internal override string CommandName() => "about";

		internal override string CommandDescription() => "Tells you about the bot.";
	}
}
