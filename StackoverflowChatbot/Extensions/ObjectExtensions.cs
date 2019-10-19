using System;
using System.Collections.Generic;
using System.Text;

namespace StackoverflowChatbot.Extensions
{
	internal static class ObjectExtensions
	{
		public static T As<T>(object o) => (T)o;
	}
}
