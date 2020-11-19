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
		public List<int> Controllers { get; set; }

		/// <summary>
		/// Token for logging in yo discord and doing things.
		/// </summary>
		public string DiscordToken { get; set; }

		/// <summary>
		/// Project ID in Firebase
		/// </summary>
		public string FirebaseProjectId { get; set; }

		/// <summary>
		/// DO NOT SPECIFY THIS IN YOUR CONFIG.JSON - it will be inferred fromt he other one
		/// </summary>
		[Newtonsoft.Json.JsonIgnore]
		[JsonIgnore]
		public Dictionary<int,string> StackToDiscordMap { get; set; }

		/// <summary>
		/// Your mappins here will be reversed for th eother one.
		/// </summary>
		public Dictionary<string,int> DiscordToStackMap { get; set; }

		public Dictionary<string,ulong> DiscordChannelNamesToIds { get; set; }

		public string DiscordInviteLink { get; set; }

		public List<int> IgnoredUsers { get; set; }

	}
}
