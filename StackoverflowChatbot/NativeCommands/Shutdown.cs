using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Shutdown : ICommand
	{
		public string? ProcessMessage(EventData eventContext, string[] parameters)
		{
			if (Config.Manager.Config().Controllers.Contains(eventContext.UserId))
			{
				Task.Run(() =>
				{
					Task.Delay(1000);
					Environment.Exit(0);
				});
				return "byeeeee";
			}
			else
			{
				return "YOU'RE NOT THE BOSS OF ME";
			}
		}

		public string CommandName() => "shutdown";

		public string? CommandDescription() => "Shutdown the bot (if you're allowed to)";
	}
}
