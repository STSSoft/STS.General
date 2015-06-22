using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.Storage;
#endif

namespace STS.General.IO
{
    public class IO
    {
        public static Stream OpenStreamForRead(string path, int bufferSize)
        {
#if NETFX_CORE

            string fileName = Path.GetFileName(path);
            StorageFolder directory = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path)).AsTask().Result;

            return new WindowsUniversalFileStream(fileName, directory, FileAccessMode.Read, CreationCollisionOption.ReplaceExisting, StorageOpenOptions.None, bufferSize, bufferSize);
#else
            return new OptimizedFileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.None);
#endif
        }

        public static Stream OpenStreamForWrite(string path, bool replaceIfExist, int bufferSize, long initializeLength = long.MinValue)
        {
#if NETFX_CORE
            string fileName = Path.GetFileName(path);
            StorageFolder directory = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path)).AsTask().Result;

            CreationCollisionOption options;
            if (replaceIfExist)
                options = CreationCollisionOption.ReplaceExisting;
            else
                options = CreationCollisionOption.FailIfExists;

            return new WindowsUniversalFileStream(fileName, directory, FileAccessMode.ReadWrite, options, StorageOpenOptions.None, bufferSize, bufferSize);
#else
            FileMode mode;
            if (replaceIfExist)
                mode = FileMode.Create;
            else
                mode = FileMode.CreateNew;

            return new OptimizedFileStream(path, mode, FileAccess.ReadWrite, FileShare.Read, bufferSize, FileOptions.None, initializeLength);
#endif
        }
    }
}
