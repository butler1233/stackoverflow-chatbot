using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.NativeCommands
{
	public interface ICommand
	{

		/// <summary>
		/// The actual process your command performs. Do whatever you like in here. It's strongly advised to return *something*, although if you return null, which you can do, there will simply be no response from the bot. 
		/// </summary>
		/// <param name="eventContext"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
#nullable enable
		string? ProcessMessage(EventData eventContext, string[] parameters);
#nullable disable

		/// <summary>
		/// The name of the command. This will be the word that users use to invoke your command. Anything that comes after this will be passed in the Parameters field. 
		/// </summary>
		/// <returns></returns>
		string CommandName();

		/// <summary>
		/// Will show up when users list commands. Lets them know exactly what a command does if you feel like providing details. Fine if you don't, but a bit of a dick move to not provide documentation.
		/// </summary>
		/// <returns></returns>
		string? CommandDescription();

	}
}
