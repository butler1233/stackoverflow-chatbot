using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SharpExchange.Chat.Actions;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.CommandProcessors
{
	/// <summary>
	/// Processes commands that trigger basic functionality that always happens before any other processors.
	/// </summary>
	internal class PriorityProcessor
	{
		private const int NumberOfRequiredSummons = 3;

		private readonly IRoomService roomService;
		private readonly int roomId;
		private static readonly Dictionary<int, HashSet<int>> PeopleWhoSummoned = new Dictionary<int, HashSet<int>>();

		public PriorityProcessor(IRoomService roomService, int roomId)
		{
			this.roomService = roomService;
			this.roomId = roomId;
		}

		/// <summary>
		/// Process the event if a suitable command is found.
		/// </summary>
		/// <returns>Whether or not the event was processed.</returns>
		internal bool ProcessCommand(EventData data, out SendMessage action)
		{
			var command = data.Command;

			if (command.StartsWith("say"))
			{
				action = NewMessageAction(command.Replace("say ", ""));
				return true;
			}

			if (command.StartsWith("leave"))
			{
				// Can be told to leave other rooms.
				this.roomService.LeaveRoom(IsSingleNumber(command.Replace("leave", "").Trim(), out var room) ? room : this.roomId);
				action = NewMessageAction(room > 0 ? $"Leaving room {room}!" : "Bye!");
				return true;
			}

			if (command.StartsWith("join"))
			{
				action = this.JoinRoomCommand(data);
				return true;
			}

			if(command == "shutdown")
			{
				_ = System.Threading.Tasks.Task.Run(async () =>
				  {
					  await System.Threading.Tasks.Task.Delay(1000);
					  System.Diagnostics.Process.GetCurrentProcess().Kill();
				  });
				action = NewMessageAction("Bye");
				return true;
			}

			action = null;
			return false;
		}

		private SendMessage JoinRoomCommand(EventData data)
		{
			if (!int.TryParse(data.Command.Replace("join ", ""), out var room))
			{
				return NewMessageAction($"Couldn't find a valid room number.");
			}

			if (!PeopleWhoSummoned.ContainsKey(room))
			{
				PeopleWhoSummoned.Add(room, new HashSet<int>());
			}

			if(data.UserId == Worker.AdminId)
			{
				var joinedByAdmin = this.roomService.JoinRoom(room);
				return NewMessageAction(joinedByAdmin ? $"I joined room {room}, Boss." : $"Couldn't join room {room}, guess I'm already there!");
			}

			if (PeopleWhoSummoned[room].Count < NumberOfRequiredSummons)
			{
				_ = PeopleWhoSummoned[room].Add(data.UserId);
				return NewMessageAction($"{NumberOfRequiredSummons - PeopleWhoSummoned[room].Count} more and I'll join room {room}");
			}

			var joined = this.roomService.JoinRoom(room);
			return NewMessageAction(joined ? $"I joined room {room}." : $"Couldn't join room {room}, guess I'm already there!");
		}

		private static SendMessage NewMessageAction(string message) => new SendMessage(message);

		private static bool IsSingleNumber(string v, out int room)
		{
			if(int.TryParse(v, out var number))
			{
				room = number;
				return true;
			}

			room = -1;
			return false;
		}
	}
}
