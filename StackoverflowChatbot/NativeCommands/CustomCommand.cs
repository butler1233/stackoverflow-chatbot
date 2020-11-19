using System;
using System.Collections.Generic;
using System.Text;
using Google.Cloud.Firestore;

namespace StackoverflowChatbot.NativeCommands
{
	[FirestoreData]
	public class CustomCommand
	{
		// Not really sure if Firestore needs a default ctor
		public CustomCommand() { }
		public CustomCommand(string name, string parameter)
		{
			this.Name = name;
			this.Parameter = parameter;
		}
		[FirestoreProperty(Name = "name")]
		public string? Name { get; set; }
		[FirestoreProperty(Name = "parameter")]
		public string? Parameter { get; set; }
	}
}
