using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace StackoverflowChatbot.NativeCommands
{
	[FirestoreData]
	public class CustomCommand: IEquatable<CustomCommand?>
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
		public DynamicCommand? DynamicCommand { get; set; }
		public bool IsDynamic { get; set; }
		public int ExpectedDynamicCommandArgs { get; set; }

		public override bool Equals(object? obj) => this.Equals(obj as CustomCommand);
		public bool Equals(CustomCommand? other) => other != null && this.Name == other.Name && this.Parameter == other.Parameter && this.IsDynamic == other.IsDynamic;
		public override int GetHashCode() => HashCode.Combine(this.Name, this.Parameter, this.IsDynamic);

		public static bool operator ==(CustomCommand? left, CustomCommand? right) => EqualityComparer<CustomCommand>.Default.Equals(left, right);
		public static bool operator !=(CustomCommand? left, CustomCommand? right) => !(left == right);
	}
}
