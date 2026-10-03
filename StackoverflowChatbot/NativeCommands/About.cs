using System.Threading.Tasks;
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
		internal override Task<IAction> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) =>
			Task.FromResult<IAction>(new SendMessage(
				"    Lee Botler: A bot for C# which probably won't work. \r\n    Written by CaptainObvious, based originally on Sandy, by SquirrelKiller. "));

		internal override string CommandName() => "about";

		internal override string CommandDescription() => "Tells you about the bot.";
	}
}
