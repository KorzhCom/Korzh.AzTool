using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;

namespace Korzh.AzTool
{

    public class BlobRenameException : Exception 
    {
        public BlobRenameException(string message) : base(message) { }
    }


    public static class BlobContainerExtensions
    {
        public static void Rename(this CloudBlobContainer container, string oldName, string newName)
        {
            RenameAsync(container, oldName, newName).GetAwaiter().GetResult();
        }

        public static async Task RenameAsync(this CloudBlobContainer container, string oldName, string newName)
        {

            var source = await container.GetBlobReferenceFromServerAsync(oldName);
            var target = container.GetBlockBlobReference(newName);

            await target.StartCopyAsync(source.Uri);

            while (target.CopyState.Status == CopyStatus.Pending)
                await Task.Delay(0);

            if (target.CopyState.Status != CopyStatus.Success) {
                throw new BlobRenameException("Rename failed: " + target.CopyState.Status);
            }

            await source.DeleteAsync();
        }
    }
}
