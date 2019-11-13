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
            int count = 0;
            foreach (var container in blobClient.ListContainers()) {

                foreach (var blob in container.ListBlobs()) {

                    string blobName = null;
                    if (blob is CloudBlob) {
                        blobName = (blob as CloudBlob).Name;
                    }
                    else if (blob is CloudBlobDirectory) {
                        //do nothing for the moment
                     
                    }

                    if (blobName != null) {
                        var newName = _arguments.OldNameRegex.Replace(blobName, _arguments.NewNamePattern);
                        if (newName != blobName) {
                            try {
                                Console.Write("Renaming blob '{0}' -> '{1}'...", blobName, newName);
                                container.RenameBlob(blob as CloudBlob, newName);
                                Console.WriteLine("OK!");
                                count++;
                            }
                            catch (BlobRenameException ex) {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"Done! {count} renames were performed");
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
