using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.Services;

namespace StackoverflowChatbot.NativeCommands
{
	[UsedImplicitly]
	internal class Join: BaseCommand
	{
		private readonly IRoomService _roomService;
		// Key: Room; Value: Set of users who summoned to this room;
		private readonly Dictionary<int, HashSet<int>> _peopleWhoSummoned = new Dictionary<int, HashSet<int>>();
		private const int NumberOfRequiredSummons = 3;

		public Join(IRoomService roomService) => _roomService = roomService;

		internal override IAction ProcessMessageInternal(EventData data, string[]? parameters)
		{
			if (parameters == null || !int.TryParse(parameters[0], out var room) || parameters.Length < 1 || parameters.Length > 1)
			{
				return new SendMessage("Couldn't find a valid room number.");
			}

			if (!_peopleWhoSummoned.ContainsKey(room))
			{
				_peopleWhoSummoned.Add(room, new HashSet<int>());
			}

			if (data.SentByController())
			{
				var joinedByAdmin = _roomService.JoinRoom(room);
				return new SendMessage(joinedByAdmin ? $"I joined room {room}, Boss." : $"Couldn't join room {room}, guess I'm already there!");
			}

			if (_peopleWhoSummoned[room].Count < NumberOfRequiredSummons)
			{
				_ = _peopleWhoSummoned[room].Add(data.UserId);
				return new SendMessage($"{NumberOfRequiredSummons - _peopleWhoSummoned[room].Count} more and I'll join room {room}");
			}

			var joined = _roomService.JoinRoom(room);
			return new SendMessage(joined ? $"I joined room {room}." : $"Couldn't join room {room}, guess I'm already there!");
		}

		internal override string CommandName() => "join";

		internal override string CommandDescription() => $"{NumberOfRequiredSummons} summons and I'll join room";
	}
}
