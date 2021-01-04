using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using StackoverflowChatbot.NativeCommands;

namespace StackoverflowChatbot.CommandProcessors
{
	internal class CommandFactory: ICommandFactory
	{
		private readonly IServiceCollection _serviceCollection;

		public CommandFactory(IServiceCollection serviceCollection) => _serviceCollection = serviceCollection;

		public BaseCommand Create(Type commandType, ICommandProcessor priorityProcessor)
		{
			if (_serviceCollection.All(x => x.ServiceType != commandType))
			{
				_serviceCollection.AddSingleton(commandType, commandType);
			}

			var commandProcessorDescription = _serviceCollection.FirstOrDefault(x => x.ServiceType.Name == "ICommandProcessor");
			_serviceCollection.Remove(commandProcessorDescription);
			_serviceCollection.AddSingleton(x => priorityProcessor);
			var commandFactoryDescription = _serviceCollection.FirstOrDefault(x => x.ServiceType.Name == "ICommandFactory");
			_serviceCollection.Remove(commandFactoryDescription);
			_serviceCollection.AddSingleton<ICommandFactory>(x => this);
			var serviceLocator = _serviceCollection.BuildServiceProvider();
			return (BaseCommand) serviceLocator.GetService(commandType);
		}
	}
}