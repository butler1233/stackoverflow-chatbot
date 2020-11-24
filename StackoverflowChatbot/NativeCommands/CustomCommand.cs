using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

		public bool IsDynamic => this.DynamicCommand != null;

		public override bool Equals(object? obj) => this.Equals(obj as CustomCommand);
		public bool Equals(CustomCommand? other) => other != null && this.Name == other.Name;
		public override int GetHashCode() => HashCode.Combine(this.Name);

		public static bool operator ==(CustomCommand? left, CustomCommand? right) => EqualityComparer<CustomCommand>.Default.Equals(left, right);
		public static bool operator !=(CustomCommand? left, CustomCommand? right) => !(left == right);
	}

	public enum ResponseType { Text, Image }
	public class DynamicCommand
	{
		public DynamicCommand() => this.ApiAddress = new Uri("http://localhost");
		public DynamicCommand(Uri apiAddress, int expectedArgsCount, ResponseType responseType, string[]? args = null)
		{
			this.ApiAddress = apiAddress;
			this.ExpectedArgsCount = expectedArgsCount;
			this.ResponseType = responseType;
			this.Args = args;
		}

		public ResponseType ResponseType { get; set; }
		public Uri ApiAddress { get; private set; }
		public string[]? Args { get; private set; }
		public int ExpectedArgsCount { get; set; }

		public static bool TryParse(string text, out DynamicCommand? cmd)
		{
			var components = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
			var apiString = components.First();
			var responseType = ParseResponseType(components.Last());
			var result = Uri.TryCreate(apiString, UriKind.Absolute, out var uri) &&
				(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
			var argsCount = Regex.Matches(apiString, "(={\\d+})").Count;
			cmd = result ? new DynamicCommand(uri!, argsCount, responseType) : null;
			return result;
		}

		private static ResponseType ParseResponseType(string text)
		{
			if (!text.StartsWith("-t"))
			{
				throw new InvalidOperationException("Missing type e.g.: -t text or -t image");
			}
			var components = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var value = components[1];
			return value switch
			{
				"text" => ResponseType.Text,
				"image" => ResponseType.Image,
				_ => throw new InvalidOperationException($"Unknown type {value}")
			};
		}
	}
}
