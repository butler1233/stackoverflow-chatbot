using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CSScriptLib;
using JetBrains.Annotations;
using StackoverflowChatbot.Actions;
using StackoverflowChatbot.ChatEvents.StackOverflow;

namespace StackoverflowChatbot.NativeCommands
{
	/// <summary>
	/// Evaluates C#-Code and returns the result.
	/// </summary>
	[UsedImplicitly]
	public class Eval: BaseCommand
	{
		internal override IAction ProcessMessageInternal(ChatMessageEventData eventContext, string[]? parameters)
		{
			if (parameters == null)
				return new SendMessage("Yeah well I'm not even gonna try to compile that.");

			switch (parameters.First())
			{
				case "using":
					return new SendMessage(AddUsings(parameters.Skip(1)));
			}

			//This is probably hilariously unsafe but who cares
			var reconstitutedSource = string.Join(" ", parameters.Select(HttpUtility.HtmlDecode));


			try
			{
				var totalblock = BuildCode(reconstitutedSource);
				dynamic css = CSScript.Evaluator.LoadCode(totalblock);

				var result = css.Execute();
				return new SendMessage("cs> " + result.ToString()); //YOLO
			}
			catch (CompilerException compError)
			{
				return new SendMessage($"Script compilation error, dumdum: \r\n   {compError.Message} ");
				//return "pls" + compError.ToString();
			}
			catch (IllegalSnippetException)
			{
				return new SendMessage("🙃");
			}
		}

		private string AddUsings(IEnumerable<string> usings)
		{
			var collection = usings as string[] ?? usings.ToArray();
			UsingList.AddRange(collection);
			return $"Added {collection.Length} usings. We now have {UsingList.Count} in total.";
		}

		private string BuildCode(string source)
		{
			if (_illegalCalls.Any(x => source.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0))
				throw new IllegalSnippetException();
			if (!(source.Contains("\n") || source.Contains(";")))
			{
				source = $"return {source};";
			}
			return CssBase.Replace("<usings>",
				string.Join(Environment.NewLine, UsingList.Select(x => $"using {x};"))).Replace("<body>", source);
		}

		public List<string> UsingList = new List<string>();

		private const string CssBase = @"<usings>
                             public class Script
                             {
                                 public dynamic Execute()
                                 {
                                     <body>
                                 }
                             }";

		//This is utter garbage
		private readonly string[] _illegalCalls = { "Environment.Exit", "Process", "Assembly", "Csscript", "File.", "Filestream" };

		internal override string CommandName() => "cs";

		internal override string CommandDescription() => "Enables you to compile and run cs snippets";
	}

	public class IllegalSnippetException: Exception
	{

	}
}
