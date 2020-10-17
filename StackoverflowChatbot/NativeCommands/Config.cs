using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSScriptLib;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Config;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Config : ICommand
	{
		public IAction? ProcessMessage(EventData eventContext, string[] parameters)
		{
			if (parameters.Any()){
				switch (parameters[0].ToLower())
				{
					case "save":
                        Manager.SaveConfig();
                        return new SendMessage($"I have (hopefully) saved my config.");
					case "admin":
						if (parameters.Length == 3) //"admin", [add|remove], [adminId]
						{
							var add = parameters[1].Equals("add");
							if (!int.TryParse(parameters[2], out var adminId))
							{
								return new SendMessage($":{eventContext.MessageId} I can only deal with numeric user IDs, not names.");
							}

							return this.AddRemoveController(add, adminId);
						}
						return new SendMessage($":{eventContext.MessageId} You must supply exactly 3 parameters to config. `{this.CommandName()} admin [add|remove] [adminId]`");

					default:
                        return new SendMessage($":{eventContext.MessageId} You're an idiot.");
				}
			}
			else
			{
				return new SendMessage($":{eventContext.MessageId} Well done genius. What do you want me to do with that?");
			}
		}

		public IAction AddRemoveController(bool add, int controllerId)
		{
			if (add)
			{
                if (Manager.Config().Controllers.Contains(controllerId))
                {
	                return new SendMessage($"User is already an admin.");
				}
                Manager.Config().Controllers.Add(controllerId); 
				return new SendMessage($"Added user to admins. Save config to persist beyond this run");
			}
			else
			{
				if (Manager.Config().Controllers.Contains(controllerId))
				{
					Manager.Config().Controllers.Remove(controllerId);
					return new SendMessage($"User has had admin abilities revoked.");
				}
				return new SendMessage($"Can't un-admin a user who already isn't an admin. *this wording is weird but it's saturday afternoon*");
			}
		}

		public string CommandName() => "conf";

		public string? CommandDescription() => "Control the bot config. If you don't know what you're doing here you probably shouldn't be doing it.";

		public bool NeedsAdmin() => true;
	}
}
