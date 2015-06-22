
namespace STS.General.Data
{
    public class TypeCode
    {
        /// <summary>
        ///  A null reference.
        /// </summary>
        public const int Empty = 0;

        /// <summary>
        /// A general type representing any reference or value type not explicitly represented by another TypeCode.
        /// </summary>
        public const int Object = 1;

        /// <summary>
        /// A database null (column) value. Not support for .NET Core.
        /// </summary>
        public const int DBNull = 2;

        /// <summary>
        ///  A simple type representing Boolean values of true or false.
        /// </summary>
        public const int Boolean = 3;

        /// <summary>
        ///  An integral type representing unsigned 16-bit integers with values between
        ///     0 and 65535. The set of possible values for the System.TypeCode.Char type
        ///     corresponds to the Unicode character set.
        /// </summary>
        public const int Char = 4;

        /// <summary>
        /// An integral type representing signed 8-bit integers with values between -128 and 127.
        /// </summary>
        public const int SByte = 5;

        /// <summary>
        ///   An integral type representing unsigned 8-bit integers with values between 0 and 255.
        /// </summary>
        public const int Byte = 6;

        /// <summary>
        ///  An integral type representing signed 16-bit integers with values between -32768 and 32767.
        /// </summary>
        public const int Int16 = 7;

        /// <summary>
        /// An integral type representing unsigned 16-bit integers with values between  0 and 65535.
        /// </summary>
        public const int UInt16 = 8;

        /// <summary>
        ///  An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647.
        /// </summary>
        public const int Int32 = 9;

        /// <summary>
        ///  An integral type representing unsigned 32-bit integers with values between 0 and 4294967295.
        /// </summary>
        public const int UInt32 = 10;

        /// <summary>
        ///  An integral type representing signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.
        /// </summary>
        public const int Int64 = 11;

        /// <summary>
        ///  An integral type representing unsigned 64-bit integers with values between 0 and 18446744073709551615.
        /// </summary>
        public const int UInt64 = 12;

        /// <summary>
        ///  A floating point type representing values ranging from approximately 1.5 x 10 -45 to 3.4 x 10 38 with a precision of 7 digits.
        /// </summary>
        public const int Single = 13;

        /// <summary>
        /// A floating point type representing values ranging from approximately 5.0 x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.
        /// </summary>
        public const int Double = 14;

        /// <summary>
        ///  A simple type representing values ranging from 1.0 x 10 -28 to approximately 7.9 x 10 28 with 28-29 significant digits.
        /// </summary>
        public const int Decimal = 15;

        /// <summary>
        /// A type representing a date and time value.
        /// </summary>
        public const int DateTime = 16;

        /// <summary>
        /// A sealed class type representing Unicode character strings.
        /// </summary>
        public const int String = 18;
    }
}
