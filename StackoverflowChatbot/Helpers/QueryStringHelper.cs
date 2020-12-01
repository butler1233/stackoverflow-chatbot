using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using StackoverflowChatbot.Extensions;

namespace StackoverflowChatbot.Helpers
{
	public static class QueryStringHelper
	{
		public static T ToObject<T>(Uri uri)
			where T : class, new()
		{
			if (IsRelativeUri(uri))
			{
				var dummyUrl = new Uri("http://localhost");
				uri = new Uri(dummyUrl, uri);
			}
			return ToObject<T>(uri.Query);
		}

		public static T ToObject<T>(string queryString)
			where T : class, new()
		{
			var dictionary = ToDictionary(queryString);
			return dictionary.ToObject<T>();
		}

		public static IDictionary<string, string> ToDictionary(string queryString)
		{
			var keyValue = HttpUtility.ParseQueryString(queryString);
			return ToDictionary(keyValue);
		}

		private static bool IsRelativeUri(Uri uri) =>
			!Uri.TryCreate(uri.OriginalString, UriKind.Absolute, out var _);

		private static IDictionary<string, string> ToDictionary(NameValueCollection nameValue) =>
			nameValue
				.AllKeys
				.Where(e => !string.IsNullOrEmpty(e))
				.ToDictionary(key => key, value => nameValue[value]);
	}
}
