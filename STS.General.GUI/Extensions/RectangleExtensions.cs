using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace STS.General.GUI.Extensions
{
    public class RectangleExtensions
    {
        public static void Inflate(ref Rectangle rectangle, int left, int right, int top, int bottom)
        {
            rectangle.X -= left;
            rectangle.Width += left;

            rectangle.Width += right;

            rectangle.Y -= top;
            rectangle.Height += top;

            rectangle.Height += bottom;
        }

        public static void Inflate(ref RectangleF rectangle, int left, int right, int top, int bottom)
        {
            rectangle.X -= left;
            rectangle.Width += left;

            rectangle.Width += right;

            rectangle.Y -= top;
            rectangle.Height += top;

            rectangle.Height += bottom;
        }
    }
}
