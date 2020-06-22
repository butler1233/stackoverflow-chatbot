using System.Collections.Generic;
using StackoverflowChatbot.Actions;

namespace StackoverflowChatbot.CommandProcessors
{
	/// <summary>
	/// Processes commands that trigger basic functionality that always happens before any other processors.
	/// </summary>
	internal class PriorityProcessor: ICommandProcessor
	{
		private const int NumberOfRequiredSummons = 3;

		private readonly IRoomService roomService;
		private readonly int roomId;

		// Key: Room; Value: Set of users who summoned to this room;
		private static readonly Dictionary<int, HashSet<int>> peopleWhoSummoned = new Dictionary<int, HashSet<int>>();

		public PriorityProcessor(IRoomService roomService, int roomId)
		{
			this.roomService = roomService;
			this.roomId = roomId;
		}

		/// <summary>
		/// Process the event if a suitable command is found.
		/// </summary>
		/// <returns>Whether or not the event was processed.</returns>
		public bool ProcessCommand(EventData data, out IAction action)
		{
			var command = data.Command;

			if (command.StartsWith("say"))
			{
				action = NewMessageAction(command.Replace("say ", ""));
				return true;
			}

			if (command.StartsWith("leave"))
			{
				action = this.LeaveRoomCommand(command);
				return true;
			}

			if (command.StartsWith("join"))
			{
				action = this.JoinRoomCommand(data);
				return true;
			}

			if (command == "shutdown")
			{
				action = ShutdownCommand();
				return true;
			}

			action = null;
			return false;
		}

		private IAction LeaveRoomCommand(string command)
		{
			// Can be told to leave other rooms.
			this.roomService.LeaveRoom(IsSingleNumber(command.Replace("leave", "").Trim(), out var room) ? room : this.roomId);
			return NewMessageAction(room > 0 ? $"Leaving room {room}!" : "Bye!");
		}

		private static IAction ShutdownCommand()
		{
			_ = System.Threading.Tasks.Task.Run(async () =>
			{
				await System.Threading.Tasks.Task.Delay(1000);
				System.Diagnostics.Process.GetCurrentProcess().Kill();
			});
			return NewMessageAction("Bye");
		}

		private SendMessage JoinRoomCommand(EventData data)
		{
			if (!int.TryParse(data.Command.Replace("join ", ""), out var room))
			{
				return NewMessageAction($"Couldn't find a valid room number.");
			}

			if (!peopleWhoSummoned.ContainsKey(room))
			{
				peopleWhoSummoned.Add(room, new HashSet<int>());
			}

			if (data.UserId == Worker.AdminId)
			{
				var joinedByAdmin = this.roomService.JoinRoom(room);
				return NewMessageAction(joinedByAdmin ? $"I joined room {room}, Boss." : $"Couldn't join room {room}, guess I'm already there!");
			}

			if (peopleWhoSummoned[room].Count < NumberOfRequiredSummons)
			{
				_ = peopleWhoSummoned[room].Add(data.UserId);
				return NewMessageAction($"{NumberOfRequiredSummons - peopleWhoSummoned[room].Count} more and I'll join room {room}");
			}

			var joined = this.roomService.JoinRoom(room);
			return NewMessageAction(joined ? $"I joined room {room}." : $"Couldn't join room {room}, guess I'm already there!");
		}

		private static SendMessage NewMessageAction(string message) => new SendMessage(message);

		private static bool IsSingleNumber(string v, out int room)
		{
			if (int.TryParse(v, out var number))
			{
				room = number;
				return true;
			}

			room = -1;
			return false;
		}
	}
}
