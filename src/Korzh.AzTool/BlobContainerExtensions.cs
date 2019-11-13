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
        public static void RenameBlob(this CloudBlobContainer container, CloudBlob blob, string newName)
        {
            RenameBlobAsync(container, blob, newName).GetAwaiter().GetResult();
        }

        public static async Task RenameBlobAsync(this CloudBlobContainer container, CloudBlob blob, string newName)
        {

            CloudBlob target = null;

            if (blob is CloudBlockBlob) {
                target = container.GetBlockBlobReference(newName);
            }
            else if (blob is CloudPageBlob) {
                target = container.GetPageBlobReference(newName);
            }
            else if (blob is CloudAppendBlob) {
                target = container.GetAppendBlobReference(newName);
            }

            await target.StartCopyAsync(blob.Uri);

            while (target.CopyState.Status == CopyStatus.Pending)
                await Task.Delay(0);

            if (target.CopyState.Status != CopyStatus.Success) {
                throw new BlobRenameException("Rename failed: " + target.CopyState.Status);
            }

            await blob.DeleteAsync();
        }
    }
}
