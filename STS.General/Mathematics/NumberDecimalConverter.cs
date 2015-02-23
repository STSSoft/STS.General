using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Mathematics
{
    public class NumberDecimalConverter
    {
        public readonly char[] Digits;

        public NumberDecimalConverter(params char[] digits)
        {
            Digits = digits;
        }

        public int From(string number)
        {
            int resultNumber = 0;
            int placeInNumber = 0;

            for (int i = number.Length - 1; i >= 0; i--)
                resultNumber += Array.IndexOf(Digits, number[i]) * (int)Math.Pow(Digits.Length, placeInNumber++);

            return resultNumber;
        }

        public string To(int decNumber)
        {
            int number = decNumber;
            string resultNumber = "";

            do
            {
                resultNumber = string.Concat(Digits[number % Digits.Length], resultNumber);
                number /= Digits.Length;
            }
            while (number > 0);

            return resultNumber;
        }
    }
}
