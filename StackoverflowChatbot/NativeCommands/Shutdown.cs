using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Config;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Shuts down the bot.
	/// </summary>
	[UsedImplicitly]
	internal class Shutdown: BaseCommand
	{
		internal override IAction? ProcessMessageInternal(EventData eventContext, string[] parameters)
		{
			if (Manager.Config().Controllers.Contains(eventContext.UserId))
			{
				var thread = new Thread(() =>
				{
                    Thread.Sleep(2500);
                    Environment.Exit(0);
				});
                thread.Start(); //This should work a bit better.
				return new SendMessage("`I'll be back...`");
			}
			else
			{
				//In theory this is redundant as the BaseCommand interface and the CommandRouter process adminosity.
				return new SendMessage("YOU'RE NOT THE BOSS OF ME");
			}
		}

		internal override string CommandName() => "shutdown";

		internal override string? CommandDescription() => "Shutdown the bot (if you're allowed to)";
		internal override bool NeedsAdmin() => true;
	}
}
