using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace STS.General.Buffers
{
    public static class StructurePrimitives
    {
        public static void StructToByteArray(object obj, byte[] buffer)
        {
            try
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);					//Allocate the buffer to memory and pin it so that GC cannot use the space (Disable GC)
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);				// copy the struct into int byte[] mem alloc 
                handle.Free();																								//Allow GC to do its job
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] StructToByteArray(object obj)
        {
            try
            {
                // This function copys the structure data into a byte[]
                byte[] buffer = new byte[Marshal.SizeOf(obj)];									//Set the buffer ot the correct size

                StructToByteArray(obj, buffer);

                return buffer;																							// return the byte[] . After all thats why we are here right.
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static T ByteArrayToStruct<T>(byte[] buffer) where T : struct
        {
            T res = default(T);

            try
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                res = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                handle.Free();

                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static object ByteArrayToStruct(byte[] buffer, Type type)
        {
            object result = null;

            try
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                result = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
                handle.Free();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool CanConvertStructToByteArray(Type type)
        {
            if (type.IsPrimitive)
                return true;

            if (!type.IsValueType || type.IsAutoLayout)
                return false;

            foreach (var fi in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!CanConvertStructToByteArray(fi.FieldType))
                    return false;
            }

            return true;
        }
    }
}
