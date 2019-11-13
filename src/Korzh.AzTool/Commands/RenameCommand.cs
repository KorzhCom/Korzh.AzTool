using System;
using System.Text.RegularExpressions;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Korzh.AzTool
{
    public class RenameCommand : ICommand
    {

        internal class Arguments 
        {

            public readonly CommandArgument ConnectionIdArg;

            public readonly CommandArgument OldNamePatternArg;

            public readonly CommandArgument NewNamePatternArg;

            public Arguments(CommandLineApplication command) 
            {
                ConnectionIdArg = command.Argument("<connection ID>", "The ID of some previously registered connection")
                                         .IsRequired();

                OldNamePatternArg = command.Argument("<old regex pattern>", "Old names regex pattern for blobs to rename")
                                           .IsRequired();

                NewNamePatternArg = command.Argument("<new regex pattern>", "New name regex pattern for blobs to rename")
                                           .IsRequired();

            }

            public string ConnectionId => !string.IsNullOrEmpty(ConnectionIdArg.Value)
                                          ? ConnectionIdArg.Value
                                          : "local";


            private Regex _oldNameRegex = null;

            public Regex OldNameRegex 
            {
                get {
                    if (_oldNameRegex is null) {
                        _oldNameRegex = new Regex(OldNamePatternArg.Value);
                    }

                    return _oldNameRegex;
                }
            
            }

            public string NewNamePattern => NewNamePatternArg.Value;

        }

        private readonly Arguments _arguments;

        internal RenameCommand(Arguments arguments) 
        {
            _arguments = arguments;
        }

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Renames blobs by regex pattern";

            var arguments = new Arguments(command);
            command.OnExecute(new RenameCommand(arguments).Run);
        }

        public int Run()
        {
            var account = GetStorageAccount();
            if (account is null) {
                return -1;
            }

            RenameBlobs(account);

            return 0;
        }

        private void RenameBlobs(CloudStorageAccount account)
        {
            var blobClient = account.CreateCloudBlobClient();
            foreach (var container in blobClient.ListContainers()) {
                foreach (CloudBlockBlob blob in container.ListBlobs()) {

                    var newName = _arguments.OldNameRegex.Replace(blob.Name, _arguments.NewNamePattern);
                    if (newName != blob.Name) {
                        try {
                            Console.WriteLine("Start rename blob '{0}' -> '{1}'", blob.Name, newName);
                            container.Rename(blob.Name, newName);
                            Console.WriteLine("Renamed successfully");
                        }
                        catch (BlobRenameException ex) {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        private CloudStorageAccount GetStorageAccount() 
        {
            string connectionString = Settings.LocalConnectionString;
            if (_arguments.ConnectionId.ToLower() != "local") {
                var storage = new ConnectionStorage(Settings.GlobalConfigFilePath);
                connectionString = storage.Get(_arguments.ConnectionId);
                if (connectionString is null) {
                    Console.WriteLine("Connection with current ID is not found: " + _arguments.ConnectionId);
                    return null;
                }
            }

            return CloudStorageAccount.Parse(connectionString);
        }
    }

}
