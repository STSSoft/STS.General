using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace STS.General.GUI.Extensions
{
    public static class ToolStripItemCollectionExtensions
    {
        public static ToolStripMenuItem GetItem(this ToolStripItemCollection collection, string text)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i].Text == text)
                    return (ToolStripMenuItem)collection[i];
            }

            return null;
        }
    }
}
