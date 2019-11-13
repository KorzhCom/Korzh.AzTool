using McMaster.Extensions.CommandLineUtils;
using System;

namespace Korzh.AzTool
{
    public class RenameCommand : ICommand
    {

        private readonly CommandLineApplication _command;


        internal class Arguments 
        {
            public Arguments() { 
            
            
            }
        
        }

        public RenameCommand(CommandLineApplication command) 
        {
            _command = command;
        }

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Renames blobs by regex pattern";


            command.OnExecute(new RenameCommand(command).Run);
        }

        public int Run()
        {
            throw new NotImplementedException();
        }
    }

}
