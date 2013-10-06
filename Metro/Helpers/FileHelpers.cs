using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Cloudsdale_Metro.Helpers {
    public static class FileHelpers {
        public static async Task<bool> FileExists(this StorageFolder folder, string fileName) {
            return (await folder.GetFilesAsync()).Any(file => file.Name == fileName);
        }

        public static async Task<StorageFolder> EnsureFolderExists(this StorageFolder parent, string folderName) {
            return await parent.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        }

        public static async Task<string> ReadAllText(this StorageFile file, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }

            using (var fileStream = await file.OpenStreamForReadAsync())
            using (var streamReader = new StreamReader(fileStream, encoding)) {
                return await streamReader.ReadToEndAsync();
            }
        }

        public static async Task SaveAllText(this StorageFile file, string contents, Encoding encoding = null) {
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            
            using (var fileStream = await file.OpenStreamForWriteAsync())
            using (var streamWriter = new StreamWriter(fileStream, encoding)) {
                await streamWriter.WriteAsync(contents);
            }
        }
    }
}
