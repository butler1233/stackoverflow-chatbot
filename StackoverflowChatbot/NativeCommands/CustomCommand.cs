using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;

namespace StackoverflowChatbot.NativeCommands
{
	[FirestoreData]
	public class CustomCommand
	{
		[FirestoreProperty(Name = "name")]
		public string? Name { get; set; }
		[FirestoreProperty(Name = "parameter")]
		public string? Parameter { get; set; }
	}
}
