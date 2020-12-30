using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using StackoverflowChatbot.Helpers;

namespace StackoverflowChatbot.NativeCommands
{
	public enum Option { Alias, Type, Method, Content, Path }
	public enum ResponseType { Text, Image }
	public enum Method {  Get, Post }
	public enum ContentType {  Json, Form, Multi }
	public class DynamicCommand
	{
		private readonly IDictionary<Option, string?> _options;

		public DynamicCommand()
		{
			ApiAddress = new Uri("http://localhost");
			_options = DefaultOptions();
		}

		public DynamicCommand(Uri apiAddress, int expectedArgsCount, IDictionary<Option, string?> options)
		{
			ApiAddress = apiAddress;
			ExpectedArgsCount = expectedArgsCount;
			_options = options;
		}

		public Uri ApiAddress { get; private set; }
		public int ExpectedArgsCount { get; set; }
		public string? Alias => _options[Option.Alias];
		public string? JsonPath => _options[Option.Path];
		public ResponseType ResponseType => ParseResponseType(_options[Option.Type]);
		public Method Method => ParseMethod(_options[Option.Method]);
		public ContentType ContentType => ParseContentType(_options[Option.Content]);

		public static bool TryParse(string text, out DynamicCommand? cmd)
		{
			try
			{
				var components = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
				// we expect the api component to come first
				var api = components.First();
				var result = Uri.TryCreate(api, UriKind.Absolute, out var uri) &&
					(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
				// TODO should we remove additional if block?
				if (!result)
				{
					cmd = null;
					return false;
				}
				var argsCount = Regex.Matches(api, "({\\d+})").Count;
				var optionString = HttpUtility.HtmlDecode(components.Last());
				var options = ParseOptions(optionString);
				cmd = result ? new DynamicCommand(uri!, argsCount, options) : null;
				return result;

			}
			catch (Exception)
			{
				cmd = null;
				return false;
			}
		}

		private static Dictionary<Option, string?> ParseOptions(string text)
		{
			var dictionary = DefaultOptions();
			var combinedOptions = CliHelper.CombineOption(text.Split(' ', StringSplitOptions.RemoveEmptyEntries), ' ');
			var queue = new Queue<string?>(combinedOptions);
			while (queue.Count > 0)
			{
				var entry = queue.Dequeue();
				if (entry == "-t" || entry == "--type")
					dictionary[Option.Type] = queue.Dequeue();
				else if (entry == "-a" || entry == "--alias")
					dictionary[Option.Alias] = queue.Dequeue();
				else if (entry == "-m" || entry == "--method")
					dictionary[Option.Method] = queue.Dequeue();
				else if (entry == "-c" || entry == "--content")
					dictionary[Option.Content] = queue.Dequeue();
				else if (entry == "-p" || entry == "--path")
					dictionary[Option.Path] = queue.Dequeue();
			}
			return dictionary;
		}

		private static ResponseType ParseResponseType(string? value)
		{
			return value switch
			{
				"text" => ResponseType.Text,
				"image" => ResponseType.Image,
				_ => ResponseType.Image
			};
		}

		private static Method ParseMethod(string? value)
		{
			return value switch
			{
				"get" => Method.Get,
				"post" => Method.Post,
				_ => Method.Get
			};
		}

		private static ContentType ParseContentType(string? value)
		{
			return value switch
			{
				"json" => ContentType.Json,
				"form" => ContentType.Form,
				"multi" => ContentType.Multi,
				_ => ContentType.Json
			};
		}

		private static Dictionary<Option, string?> DefaultOptions()
		{
			return new Dictionary<Option, string?>
			{
				[Option.Alias] = null,
				[Option.Type] = null,
				[Option.Method] = null,
				[Option.Content] = null,
				[Option.Path] = null,
			};
		}
	}
}
