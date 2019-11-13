using System;
using System.Reflection;

using McMaster.Extensions.CommandLineUtils;

namespace Korzh.AzTool
{
    class Program
    {
        static int Main(string[] args)
        {

            Console.WriteLine($"Korzh.AzTool (aztool) utily version: {GetProgramVersion()} (c) Korzh.com 2019");
            Console.WriteLine();

            var app = new CommandLineApplication();
            RootCommand.Configure(app);
            return app.Execute(args);
        }

        static string GetProgramVersion()
        {

            return Assembly.GetExecutingAssembly()
                            .GetName().Version.ToString();
        }
    }

    // Commands/RootCommand.cs
    public class RootCommand : ICommand
    {
        public static void Configure(CommandLineApplication app)
        {
            app.Name = "aztool";
            app.HelpOption("-?|-h|--help");

            app.Command("rename", c => RenameCommand.Configure(c));

            app.OnExecute(new RootCommand(app).Run);
        }

        private readonly CommandLineApplication _app;

        public RootCommand(CommandLineApplication app)
        {
            _app = app;
        }

        public int Run()
        {
            _app.ShowHelp();

            return 0;
        }
    }

}
