using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Korzh.AzTool
{
    public class CopyCommand : ICommand
    {

        internal class Arguments 
        {

            public readonly CommandArgument SrcConnectionIdArg;

            public readonly CommandArgument DestConnectionIdArg;

            public Arguments(CommandLineApplication command) 
            {
                SrcConnectionIdArg = command.Argument("<source connection ID>", "The ID of some previously registered connection we will copy from")
                                         .IsRequired();

                DestConnectionIdArg = command.Argument("<destination connection ID>", "The ID of some previously registered connection we want to copy to")
                         .IsRequired();
            }

            public string SrcConnectionId => !string.IsNullOrEmpty(SrcConnectionIdArg.Value)
                                          ? SrcConnectionIdArg.Value
                                          : "Default";

            public string DestConnectionId => !string.IsNullOrEmpty(DestConnectionIdArg.Value)
                                          ? DestConnectionIdArg.Value
                                          : "local";
        }

        private readonly Arguments _arguments;

        internal CopyCommand(Arguments arguments) 
        {
            _arguments = arguments;
        }

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Copies all content of some storage account to another storage account";

            var arguments = new Arguments(command);
            command.OnExecute(new CopyCommand(arguments).Run);
        }

        public int Run()
        {
            var srcAccount = GetStorageAccount(_arguments.SrcConnectionId);
            if (srcAccount is null) {
                return -1;
            }

            var destAccount = GetStorageAccount(_arguments.DestConnectionId);
            if (srcAccount is null) {
                return -1;
            }


            CopyBlobsAsync(srcAccount, destAccount)
                .GetAwaiter().GetResult();

            return 0;
        }

        private async Task CopyBlobsAsync(CloudStorageAccount srcAccount, CloudStorageAccount destAccount)
        {
            var srcBlobClient = srcAccount.CreateCloudBlobClient();
            var destBlobClient = destAccount.CreateCloudBlobClient();
            int containerCount = 0;
            foreach (var srcContainer in srcBlobClient.ListContainers()) {
                Console.WriteLine($"Processing container '{srcContainer.Name}'");
                int blobCount = 0;
                var destContainer = destBlobClient.GetContainerReference(srcContainer.Name);
                await destContainer.CreateIfNotExistsAsync();

                foreach (var srcBlob in srcContainer.ListBlobs()) {
                    if (srcBlob is CloudBlockBlob srcBlockBlob) {
                        var blobName = srcBlockBlob.Name;
                        
                        //TEMPORAL FILTER (copying images only)
                        //if (!blobName.EndsWith(".png") && !blobName.EndsWith(".jpg")) continue;

                        Console.Write($"  Copying {blobName}...");
                        var destBlockBlob = destContainer.GetBlockBlobReference(blobName);
                        using (var srcStream = await srcBlockBlob.OpenReadAsync()) {
                            await destBlockBlob.UploadFromStreamAsync(srcStream);
                        }
                        Console.WriteLine($"ok");
                    }
                    else if (srcBlob is CloudBlobDirectory) {
                        //do nothing for the moment
                     
                    }
                    blobCount++;

                }
                Console.WriteLine($"Done. '{blobCount} blobs were copied");

                containerCount++;
            }
            Console.WriteLine($"All Done! {containerCount} blob containers were copied");
        }

        private CloudStorageAccount GetStorageAccount(string connectionId) 
        {
            string connectionString = Settings.LocalConnectionString;
            if (connectionId.ToLower() != "local") {
                var storage = new ConnectionStorage(Settings.GlobalConfigFilePath);
                connectionString = storage.Get(connectionId);
                if (connectionString is null) {
                    Console.WriteLine("Connection with current ID is not found: " + connectionId);
                    return null;
                }
            }

            return CloudStorageAccount.Parse(connectionString);
        }
    }

}
