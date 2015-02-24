using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STS.General.Extensions;

namespace STS.General.Buffers
{
    public class HashCodeBuilder
    {
        private int hashCode = 37;
        private const int CONSTANT = 17;

        public override int GetHashCode()
        {
            return hashCode;
        }

        public void Append(byte[] obj)
        {
            hashCode = hashCode * CONSTANT + obj.GetHashCodeEx();
        }

        public void Append(string obj)
        {
            hashCode = hashCode * CONSTANT + obj.GetHashCode();
        }

        public void Append(byte obj)
        {
            hashCode = hashCode * CONSTANT + obj;
        }

        public void Append(int obj)
        {
            hashCode = hashCode * CONSTANT + obj;
        }

        public void Append(long obj)
        {
            hashCode = hashCode * CONSTANT + obj.GetHashCode();
        }

        public void Append(object obj)
        {
            hashCode = hashCode * CONSTANT + obj.GetHashCode();
        }

        public void Append(Type obj)
        {
            throw new Exception("Do not use.");
        }
    }
}
