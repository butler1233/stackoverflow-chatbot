using System.Collections.Generic;
using System.Text.RegularExpressions;
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

			if (IsCommand(command, "say", out var commandParameter))
			{
				action = NewMessageAction(commandParameter);
				return true;
			}

			if (IsCommand(command, "leave", out commandParameter))
			{
				action = this.LeaveRoomCommand(commandParameter);
				return true;
			}

			if (IsCommand(command, "join", out _))
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

		private static bool IsCommand(string commandMessage, string commandKeyword, out string commandParameter)
		{
			var match = Regex.Match(commandMessage, $"^{commandKeyword} (.+)");

			if (match.Success)
			{
				commandParameter = match.Groups[1].Value;
				return true;
			}

			commandParameter = string.Empty;
			return false;
		}

		private IAction LeaveRoomCommand(string commandParameter)
		{
			// Can be told to leave other rooms.
			this.roomService.LeaveRoom(IsSingleNumber(commandParameter.Trim(), out var room) ? room : this.roomId);
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

			if (data.SentByController())
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
