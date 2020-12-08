using System;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.NativeCommands
{
	public abstract class BaseCommand
	{
		internal IAction? ProcessMessage(EventData data, string[]? parameters)
		{
			//If it's a admin command and the user isn't an admin, tell them to sod off.
			if (NeedsAdmin() && !StackoverflowChatbot.Config.Manager.Config().Controllers.Contains(data.UserId))
			{
				Console.WriteLine($"[{data.RoomId}] {data.Username} attempted (unsuccessfully) to invoke {GetType().AssemblyQualifiedName}: {data.Command}");
				return new SendMessage($":{data.MessageId} YOU'RE NOT MY MOM/DAD *(you don't have permission to execute that this)*");
			}
			return ProcessMessageInternal(data, parameters);
		}

		/// <summary>
		/// The actual process your command performs. Do whatever you like in here. It's strongly advised to return *something*, although if you return null, which you can do, there will simply be no response from the bot. 
		/// </summary>
		/// <param name="eventContext"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal abstract IAction? ProcessMessageInternal(EventData eventContext, string[]? parameters);

		/// <summary>
		/// The name of the command. This will be the word that users use to invoke your command. Anything that comes after this will be passed in the Parameters field. 
		/// </summary>
		/// <returns></returns>
		internal abstract string CommandName();

		/// <summary>
		/// Will show up when users list commands. Lets them know exactly what a command does if you feel like providing details. Fine if you don't, but a bit of a dick move to not provide documentation.
		/// </summary>
		/// <returns></returns>
		internal abstract string? CommandDescription();

		/// <summary>
		/// If this command is only usable (and visible to) controllers. Although obviously if a help is run by a controller then everyone will see it. But if they're not a controller then they can't do it.
		/// </summary>
		internal virtual bool NeedsAdmin() => false;

	}
}
