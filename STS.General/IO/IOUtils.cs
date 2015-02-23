using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace STS.General.IO
{
    public static class IOUtils
    {
        public static readonly Action ThrowEndOfFileError = Expression.Lambda<Action>(Expression.Call(Type.GetType("System.IO.__Error").GetMethod("EndOfFile", BindingFlags.NonPublic | BindingFlags.Static))).Compile();
        public static readonly Action ThrowFileNotOpenError = Expression.Lambda<Action>(Expression.Call(Type.GetType("System.IO.__Error").GetMethod("FileNotOpen", BindingFlags.NonPublic | BindingFlags.Static))).Compile();

        public static long GetTotalFreeSpace(string driveName)
        {
            driveName = driveName.ToUpper();

            var drive = DriveInfo.GetDrives().Where(x => x.IsReady && x.Name == driveName).FirstOrDefault();

            return drive != null ? drive.TotalFreeSpace : -1;
        }

        public static long GetTotalSpace(string driveName)
        {
            driveName = driveName.ToUpper();

            var drive = new DriveInfo(driveName);

            return drive != null ? drive.TotalSize : -1;
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern, bool recursive)
        {
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
                        foreach (var subdir in next) pending.Push(subdir);
                    }
                    catch
                    {
                    }
                }
            }
        }        
    }
}
