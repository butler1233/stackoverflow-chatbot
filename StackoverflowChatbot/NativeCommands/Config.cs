using System.Linq;
using Botler.Core.Config;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	internal class Config: BaseCommand
	{
		internal override IAction ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters)
		{
			if (parameters?.Any() != true)
				return new SendMessage(
					$":{eventContext.MessageId} Well done genius. What do you want me to do with that?");

			switch (parameters[0].ToLower())
			{
				case "save":
					Manager.SaveConfig();
					return new SendMessage("I have (hopefully) saved my config.");
				case "admin":
					if (parameters.Length != 3)
						return new SendMessage(
							$":{eventContext.MessageId} You must supply exactly 3 parameters to config. `{CommandName()} admin [add|remove] [adminId]`");

					var add = parameters[1].Equals("add");
					return !int.TryParse(parameters[2], out var adminId)
						? new SendMessage($":{eventContext.MessageId} I can only deal with numeric user IDs, not names.")
						: AddRemoveController(add, adminId);

				default:
					return new SendMessage($":{eventContext.MessageId} You're an idiot.");
			}
		}

		public IAction AddRemoveController(bool add, int controllerId)
		{
			if (add)
			{
				if (Manager.Config().Controllers.Contains(controllerId))
				{
					return new SendMessage("User is already an admin.");
				}
				Manager.Config().Controllers.Add(controllerId);
				return new SendMessage("Added user to admins. Save config to persist beyond this run");
			}

			if (Manager.Config().Controllers.Contains(controllerId))
			{
				Manager.Config().Controllers.Remove(controllerId);
				return new SendMessage("User has had admin abilities revoked.");
			}

			return new SendMessage("Can't un-admin a user who already isn't an admin. *this wording is weird but it's saturday afternoon*");
		}

		internal override string CommandName() => "conf";

		internal override string CommandDescription() => "Control the bot config. If you don't know what you're doing here you probably shouldn't be doing it.";

		internal override bool NeedsAdmin() => true;
	}
}
