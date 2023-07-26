using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StackoverflowChatbot.Config
{
	internal class Base
	{

		/// <summary>
		/// Trigger phrases for the bot
		/// </summary>
		public List<string> Triggers { get; set; } = null!;

		/// <summary>
		/// users who are allowed to control the bot
		/// </summary>
		public List<int> Controllers { get; set; } = null!;

		/// <summary>
		/// Token for logging in yo discord and doing things.
		/// </summary>
		public string DiscordToken { get; set; } = null!;

		/// <summary>
		/// Project ID in Firebase
		/// </summary>
		public string FirebaseProjectId { get; set; } = null!;

		/// <summary>
		/// DO NOT SPECIFY THIS IN YOUR CONFIG.JSON - it will be inferred from the other one
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		[JsonIgnore]
		public Dictionary<int, string> StackToDiscordMap { get; set; } = null!;

		/// <summary>
		/// Your mappins here will be reversed for the other one.
		/// </summary>
		public Dictionary<string, int> DiscordToStackMap { get; set; } = null!;

		public Dictionary<string, ulong> DiscordChannelNamesToIds { get; set; } = null!;

		public string DiscordInviteLink { get; set; } = null!;

		public List<int> IgnoredUsers { get; set; } = null!;

		public string SqliteFilename { get; set; } = "C:/bot-sqlite.db";

		/// <summary>
		/// Contains a list of rooms that will automatically get joined on startup
		/// </summary>
		public List<int> AutoJoinRoomIds { get; set; } = new();

	}
}
