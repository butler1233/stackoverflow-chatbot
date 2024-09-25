using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Botler.Core.Config;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Shuts down the bot.
	/// </summary>
	[UsedImplicitly]
	internal class Shutdown: BaseCommand
	{
		internal override IAction ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters)
		{
			//In theory this is redundant as the BaseCommand interface and the CommandRouter process adminosity.
			if (!Manager.Config().Controllers.Contains(eventContext.UserId))
				return new SendMessage("YOU'RE NOT THE BOSS OF ME");

			var thread = new Thread(() =>
			{
				Thread.Sleep(2500);
				Environment.Exit(0);
			});
			thread.Start(); //This should work a bit better.
			if (eventContext.RoomId == 1)
			{
				return new SendMessage("`I'll be back...`");
			}
			return new SendMessage("`I'll be back...` (*but in the sandbox so you'll have to invite me back here in a minute*)");
		}

		internal override string CommandName() => "shutdown";

		internal override string CommandDescription() => "Shutdown the bot (if you're allowed to)";
		internal override bool NeedsAdmin() => true;
	}
}
