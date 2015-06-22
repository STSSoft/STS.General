using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

#if NETFX_CORE
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
#endif

namespace STS.General.IO
{
    public static class IOUtils
    {
#if NETFX_CORE
        public static readonly Action ThrowEndOfFileError = () => { throw new System.IO.EndOfStreamException(); };
        public static readonly Action ThrowFileNotOpenError = () => { throw new System.IO.FileNotFoundException(); };
#else
        public static readonly Action ThrowEndOfFileError = Expression.Lambda<Action>(Expression.Call(Type.GetType("System.IO.__Error").GetMethod("EndOfFile", BindingFlags.NonPublic | BindingFlags.Static))).Compile();
        public static readonly Action ThrowFileNotOpenError = Expression.Lambda<Action>(Expression.Call(Type.GetType("System.IO.__Error").GetMethod("FileNotOpen", BindingFlags.NonPublic | BindingFlags.Static))).Compile();
#endif

        public static long GetTotalFreeSpace(string driveName)
        {
#if NETFX_CORE
            string propertyName = "System.FreeSpace";
            IDictionary<string, object> properties = GetProperties(driveName, propertyName);

            return (long)properties[propertyName];
#else

            driveName = driveName.ToUpper();

            var drive = DriveInfo.GetDrives().Where(x => x.IsReady && x.Name == driveName).FirstOrDefault();

            return drive != null ? drive.TotalFreeSpace : -1;
#endif
        }

        public static long GetTotalSpace(string driveName)
        {
#if NETFX_CORE
            string propertyName = "System.Capacity";
            IDictionary<string, object> properties = GetProperties(driveName, propertyName);

            return (long)properties[propertyName];
#else
            driveName = driveName.ToUpper();

            var drive = new DriveInfo(driveName);

            return drive != null ? drive.TotalSize : -1;
#endif
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern, bool recursive)
        {
#if NETFX_CORE
            if (!recursive)
            {
                Task<StorageFolder> storageTask = StorageFolder.GetFolderFromPathAsync(root).AsTask();
                Task<IReadOnlyList<StorageFile>> allFilesTask = storageTask.Result.GetFilesAsync().AsTask();

                foreach (var item in allFilesTask.Result)
                {
                    if (item.FileType == searchPattern)
                        yield return item.Path;
                }
            }
            else
            {
                StorageFolder currentFolder = null;
                Stack<string> pending = new Stack<string>();

                pending.Push(root);
                while (pending.Count != 0)
                {
                    var path = pending.Pop();
                    string[] next = null;

                    try
                    {
                        Task<StorageFolder> storageTask = StorageFolder.GetFolderFromPathAsync(path).AsTask();
                        currentFolder = storageTask.Result;

                        Task<IReadOnlyList<StorageFile>> allFilesTask = currentFolder.GetFilesAsync().AsTask();

                        next = allFilesTask.Result.Where(x => x.FileType == searchPattern).Select(file => file.Path).ToArray();
                    }
                    catch { }

                    if (next != null && next.Length != 0)
                    {
                        foreach (var file in next)
                            yield return file;
                    }

                    try
                    {
                        Task<IReadOnlyList<StorageFolder>> allFolder = currentFolder.GetFoldersAsync().AsTask();

                        next = allFolder.Result.Select(x => x.Path).ToArray();

                        foreach (var subdir in next)
                            pending.Push(subdir);
                    }
                    catch { }
                }
            }
#else
            if (!recursive)
            {
                foreach (var item in Directory.GetFiles(root, searchPattern))
                    yield return item;
            }
            else
            {
                Stack<string> pending = new Stack<string>();

                pending.Push(root);
                while (pending.Count != 0)
                {
                    var path = pending.Pop();
                    string[] next = null;
                    try
                    {
                        next = Directory.GetFiles(path, searchPattern);
                    }
                    catch { }

                    if (next != null && next.Length != 0)
                    {
                        foreach (var file in next)
                            yield return file;
                    }
                    try
                    {
                        next = Directory.GetDirectories(path);
                        foreach (var subdir in next)
                            pending.Push(subdir);
                    }
                    catch
                    {
                    }
                }
            }
#endif
        }

#if NETFX_CORE
        private static IDictionary<string, object> GetProperties(string path, params string[] properties)
        {
            Task<StorageFolder> storageTask = StorageFolder.GetFolderFromPathAsync(path).AsTask();

            if (storageTask.Result == null)
                return null;

            Task<IDictionary<string, object>> freeSpace = storageTask.Result.Properties.RetrievePropertiesAsync(properties).AsTask();

            return freeSpace.Result;
        }
#endif
    }
}