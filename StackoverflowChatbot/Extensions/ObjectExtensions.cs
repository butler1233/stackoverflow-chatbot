using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StackoverflowChatbot.Extensions
{
	internal static class ObjectExtensions
	{
		public static T As<T>(object o) => (T)o;

		public static T ToObject<T>(this IDictionary<string, object> source)
			where T : class, new() => ToObject<T, object>(source);

		public static T ToObject<T>(this IDictionary<string, string> source)
			where T : class, new() => ToObject<T, string>(source);

		private static TReturnType ToObject<TReturnType, TValue>(IDictionary<string, TValue> source)
			where TReturnType : class, new()
		{
			var obj = new TReturnType();
			var type = obj.GetType();
			foreach (var item in source)
			{
				type.GetProperty(item.Key)?
					.SetValue(obj, item.Value, null);
			}
			return obj;
		}

		public static IDictionary<string, object?> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
		{
			return source.GetType().GetProperties(bindingAttr).ToDictionary
			(
				propInfo => propInfo.Name,
				propInfo => propInfo.GetValue(source, null)
			);
		}

		public static IDictionary<string, string?> AsStringDictionary(this object source, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
		{
			var properties = source.GetType().GetProperties(bindingAttr);
			var dictionary = new Dictionary<string, string?>();
			foreach (var prop in properties)
			{
				var key = prop.Name;
				var value = prop.GetValue(source, null) as string;
				dictionary[key] = value;
			}
			return dictionary;
		}
	}
}
