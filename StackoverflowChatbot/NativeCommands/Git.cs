using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	internal class Git : BaseCommand
	{
		internal override Task<IAction?> ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters) => Task.FromResult<IAction?>(new SendMessage("You can find me on Github: [butler1233/stackoverflow-chatbot](https://github.com/butler1233/stackoverflow-chatbot) Contribute to me :)"));

		internal override string CommandName() => "git";

		internal override string? CommandDescription() => "Spits out where you can find my source code.";
	}
}
