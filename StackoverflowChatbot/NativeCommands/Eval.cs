using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;
using CSScriptLib;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using Newtonsoft.Json;

namespace StackoverflowChatbot.NativeCommands
{
	public class Eval : ICommand
	{
		public string? ProcessMessage(EventData eventContext, string[] parameters)
		{
			switch (parameters.First())
			{
				case "using":
					return this.AddUsings(parameters.Skip(1));
			}

			//This is probably hilariously unsafe but who cares
			var reconstitutedSource = string.Join(" ", parameters.Select(p => HttpUtility.HtmlDecode(p)));
			


			try
			{
				var totalblock = this.BuildCode(reconstitutedSource);
				dynamic css = CSScript.Evaluator.LoadCode(totalblock);

				var result = css.Execute();
				return $"cs> " + result.ToString(); //YOLO
			}
			catch (CompilerException compError)
			{
				return $"Script compilation errror, dumdum: \r\n   {compError.Message} ";
				//return "pls" + compError.ToString();
			}
			catch (IllegalSnippetException illegal)
			{
				return "🙃";
			}


		}

		private string AddUsings(IEnumerable<string> usings)
		{
            this.UsingList.AddRange(usings);
            return $"Added {usings.Count()} usings. We now have {this.UsingList.Count} in total.";
		}

		private string BuildCode(string source)
		{
			if (this.IllegalCalls.Any(x => source.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0))
				throw new IllegalSnippetException();
			if (!(source.Contains("\n") || source.Contains(";")))
			{
				source = $"return {source};";
			}
			return cssBase.Replace("<usings>",
				string.Join(Environment.NewLine, this.UsingList.Select(x => $"using {x};"))).Replace("<body>",source);
		}

        public List<string> UsingList = new List<string>();

		private const string cssBase = @"<usings>
                             public class Script
                             {
                                 public dynamic Execute()
                                 {
                                     <body>
                                 }
                             }";

        //This is utter garbage
        private readonly string[] IllegalCalls =  {"Environment.Exit","Process","Assembly","Csscript", "File.", "Filestream"};

		public string CommandName() => "cs";

		public string? CommandDescription() => "Enables you to compile and run cs snippets";
	}

	public class IllegalSnippetException: Exception
	{

	}
}
