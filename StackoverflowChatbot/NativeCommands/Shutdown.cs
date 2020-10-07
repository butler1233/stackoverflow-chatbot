using System;
using System.Linq;
using System.Threading.Tasks;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Shutdown: ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters)
		{
			if (Config.Manager.Config().Controllers.Contains(eventContext.UserId))
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
				return new SendMessage("YOU'RE NOT THE BOSS OF ME");
			}
		}

		public string CommandName() => "shutdown";

		public string? CommandDescription() => "Shutdown the bot (if you're allowed to)";
	}
}
