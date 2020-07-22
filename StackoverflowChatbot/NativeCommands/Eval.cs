using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;
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
			//IVE NEVER USED CSHARP CODE PROVIDER BEFORE I DON@T KNOW WHAT I@M DOING
			var reconstitutedSource = string.Join(" ", parameters.Select(p => HttpUtility.HtmlDecode(p)));
			var result = this.Compile(reconstitutedSource);
			return result;
		}

		private string Compile(string source)
		{
			try
			{
				var compiledScript = CSharpScript.Create(source);
				var result = compiledScript.RunAsync().Result;
				return $"cs> " + result.ReturnValue.ToString(); //YOLO
			}
			catch (CompilationErrorException compError)
			{
				return $"Script compilation errror, dumdum: \r\n    " + string.Join("\r\n    ",
					compError.Diagnostics.Select(x => $"[{x.Severity}] {x.ToString()} "));
				
			}
			

		}


		public string CommandName() => "cs";

		public string? CommandDescription() => "Enables you to compile and run cs snippets";
	}
}
