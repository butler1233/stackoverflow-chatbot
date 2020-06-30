namespace StackoverflowChatbot
{
	public interface IRoomService
	{
		/// <summary>
		/// Logs in to Stackoverflow.
		/// </summary>
		bool Login();

		/// <summary>
		/// Joins the given room.
		/// </summary>
		/// <param name="roomNumber">Room to join</param>
		/// <returns>If the room could be joined.</returns>
		bool JoinRoom(int roomNumber);
		void LeaveRoom(int roomNumber);
	}
}