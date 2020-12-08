using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	internal class Leave: BaseCommand
	{
		private readonly IRoomService _roomService;

		public Leave(IRoomService roomService) => _roomService = roomService;

		internal override IAction ProcessMessageInternal(EventData eventContext, string[]? parameters)
		{
			if (parameters != null && parameters.Length > 1)
			{
				return new SendMessage("Too many parameters for leave room"); 
			}

			if (parameters == null || parameters.Length == 0)
			{
				_roomService.LeaveRoom(-1);
				return new SendMessage("Bye!");
			}

			if (int.TryParse(parameters[0], out var number))
			{
				_roomService.LeaveRoom(number);
				return new SendMessage($"Leaving room {number}!");
			}

			return new SendMessage("The parameter must be a room number");
		}

		internal override string CommandName() => "leave";

		internal override string CommandDescription() => "Leaves the specified room. If no parameter given, leaves the current room";
	}
}
