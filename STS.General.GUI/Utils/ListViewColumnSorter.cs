using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace STS.General.GUI.Utils
{

    public class ListViewColumnSorter : IComparer
    {
        public int ColumnToSort { get; set; }
        public SortOrder Order { get; set; }

        public ListViewColumnSorter()
        {
            ColumnToSort = 0;
            Order = SortOrder.None;
        }

        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            compareResult = listviewX.SubItems[ColumnToSort].Text.CompareTo(listviewY.SubItems[ColumnToSort].Text);

            if (Order == SortOrder.Ascending)
                return compareResult;
            else if (Order == SortOrder.Descending)
                return (-compareResult);
            else
                return 0;
        }
    }
}
