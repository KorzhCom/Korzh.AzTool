using System;
using System.Linq;

using McMaster.Extensions.CommandLineUtils;

namespace Korzh.AzTool
{

    public class ConnectionsCommand : ICommand
    {
        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Manipulates with connections (add, remove, list)";
            command.HelpOption("-?|-h|--help");

            // add connections subcommands
            command.Command("add", c => ConnectionsAddCommand.Configure(c));
            command.Command("remove", c => ConnectionsRemoveCommand.Configure(c));
            command.Command("list", c => ConnectionsListCommand.Configure(c));

            Func<int> runCommandFunc = new ConnectionsCommand(command).Run;
            command.OnExecute(runCommandFunc);
        }

        private CommandLineApplication _command;

        public ConnectionsCommand(CommandLineApplication command)
        {
            _command = command;
        }

        public int Run()
        {
            _command.ShowHelp();

            return 0;
        }
    }

    public class ConnectionsAddCommand : ICommand
    {

        public class Arguments
        {

            public readonly CommandArgument ConnectionIdArg;
            public readonly CommandArgument ConnectionStringArg;

            public Arguments(CommandLineApplication command)
            {
                ConnectionIdArg = command.Argument("<сonnection ID>", "The connection ID stored in the configuration")
                                         .IsRequired();

                ConnectionStringArg = command.Argument("<сonnection string>", "The connection string to add")
                                             .IsRequired();

            }

            public string ConnectionId => ConnectionIdArg.Value;
            public string ConnectionString => ConnectionStringArg.Value;

        }

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Adds connection to configuration file";
            command.HelpOption("-?|-h|--help");


            var argumets = new Arguments(command);

            Func<int> runCommandFunc = new ConnectionsAddCommand(argumets).Run;
            command.OnExecute(runCommandFunc);
        }

        private readonly Arguments _arguments;

        public ConnectionsAddCommand(Arguments arguments)
        {
            _arguments = arguments;
        }

        public int Run()
        {

            if (_arguments.ConnectionId.ToLowerInvariant() == "local") {
                Console.WriteLine("Connection with ID: \"Local\" is reserved. ");
                return -1;
            }

            var storage = new ConnectionStorage(Settings.GlobalConfigFilePath);
            storage.Add(_arguments.ConnectionId, _arguments.ConnectionString);
            storage.SaveChanges();

            Console.WriteLine($"Connection {_arguments.ConnectionId} has been added.");

            return 0;
        }
    }

    public class ConnectionsRemoveCommand : ICommand
    {
        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Removes connection from configuration file";
            command.HelpOption("-?|-h|--help");

            var connectionIdArg = command.Argument("<сonnection ID>", "The ID of the connection stored in the configuration")
                                        .IsRequired();

            Func<int> runCommandFunc = new ConnectionsRemoveCommand(connectionIdArg).Run;
            command.OnExecute(runCommandFunc);
        }

        private readonly CommandArgument _connectionIdArg;

        public ConnectionsRemoveCommand(CommandArgument connectionIdArg)
        {
            _connectionIdArg = connectionIdArg;
        }

        public int Run()
        {

            if (_connectionIdArg.Value.ToLowerInvariant() == "local") {
                Console.WriteLine("Connection with ID: \"Local\" cannot be removed.");
                return -1;
            }

            var storage = new ConnectionStorage(Settings.GlobalConfigFilePath);

            storage.Remove(_connectionIdArg.Value);
            storage.SaveChanges();

            Console.WriteLine($"Connection {_connectionIdArg.Value} has been removed.");

            return 0;
        }
    }

    public class ConnectionsListCommand : ICommand
    {
        public static void Configure(CommandLineApplication command)
        {

            command.Description = "Shows the list of connections.";

            Func<int> runCommandFunc = new ConnectionsListCommand().Run;
            command.OnExecute(runCommandFunc);
        }


        public ConnectionsListCommand()
        {
      
        }

        public int Run()
        {
            var storage = new ConnectionStorage(Settings.GlobalConfigFilePath);

            var connections = storage.List();
            Console.WriteLine($"Connections (global): ");
            Console.WriteLine("\"{1}\": \"{2}\"", "local", Settings.LocalConnectionString);
            if (connections.Any()) {
                foreach (var connection in connections) {
                    Console.WriteLine("\"{1}\": \"{2}\"", connection.ConnectionId, connection.ConnectionString);
                }
            }

            return 0;
        }
    }
}