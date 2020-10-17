using System;
using System.Linq;
using System.Threading.Tasks;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Config;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Shutdown: ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters)
		{
			if (Manager.Config().Controllers.Contains(eventContext.UserId))
			{
				Task.Run(() =>
				{
					Task.Delay(1000);
					Environment.Exit(0);
				});
				return new SendMessage("Byeeeee");
			}
			else
			{
				//In theory this is redundant as the ICommand interface and teh CommandRouter process adminosity.
				return new SendMessage("YOU'RE NOT THE BOSS OF ME");
			}
		}

		public string CommandName() => "shutdown";

		public string? CommandDescription() => "Shutdown the bot (if you're allowed to)";
		public bool NeedsAdmin() => true;
	}
}
