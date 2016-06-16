using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace STS.General.GUI.Frames
{
    public class ListViewExtended : ListView
    {
        public ColumnHeader LastSelectedColumn;

        [Browsable(false)]
        public bool IsSelectedColumn;

        public ListViewGroupState LastState;
        private Rectangle _headerRect;

        private const int LVM_FIRST = 0x1000;                    // ListView messages
        private const int LVM_SETGROUPINFO = (LVM_FIRST + 147);  // ListView messages Setinfo on Group
        private const int WM_LBUTTONUP = 0x0202;                 // Windows message left button

        private delegate void CallBackSetGroupState(ListViewGroup lstvwgrp, ListViewGroupState state);
        private delegate void CallbackSetGroupString(ListViewGroup lstvwgrp, string value);

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref LVGROUP lParam);

        // Delegate that is called for each child window of the ListView.
        private delegate bool EnumWinCallBack(IntPtr hwnd, IntPtr lParam);

        // Calls EnumWinCallBack for each child window of hWndParent (i.e. the ListView).
        [DllImport("user32.Dll")]
        private static extern int EnumChildWindows(IntPtr hWndParent, EnumWinCallBack callBackFunc, IntPtr lParam);

        // Gets the bounding rectangle of the specified window (ListView header bar).
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //Double buffered list view
        public ListViewExtended()
        {
            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnContextMenuChanged(EventArgs e)
        {
            base.OnContextMenuChanged(e);

            ContextMenuStrip.Opening += ContextMenuStrip_Opening;
        }

        public void ContextMenuStrip_Opening(object sender, EventArgs e)
        {
            ColumnSelected();

            ((ContextMenuStrip)sender).Show(Control.MousePosition);
        }

        public bool ColumnSelected()
        {
            EnumChildWindows(this.Handle, new EnumWinCallBack(EnumWindowCallBack), IntPtr.Zero);

            if (_headerRect.Contains(Control.MousePosition))
            {
                IsSelectedColumn = true;

                int xoffset = Control.MousePosition.X - _headerRect.Left;

                int sum = 0;
                foreach (ColumnHeader header in GetOrderedHeaders(this))
                {
                    sum += header.Width;
                    if (sum > xoffset)
                    {
                        LastSelectedColumn = header;
                        break;
                    }
                }
            }
            else
                IsSelectedColumn = false;


            return IsSelectedColumn;
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }

        private static int? GetGroupID(ListViewGroup lstvwgrp)
        {
            int? rtnval = null;
            Type GrpTp = lstvwgrp.GetType();
            if (GrpTp != null)
            {
                PropertyInfo pi = GrpTp.GetProperty("ID", BindingFlags.NonPublic | BindingFlags.Instance);
                if (pi != null)
                {
                    object tmprtnval = pi.GetValue(lstvwgrp, null);
                    if (tmprtnval != null)
                    {
                        rtnval = tmprtnval as int?;
                    }
                }
            }
            return rtnval;
        }

        private static void setGrpState(ListViewGroup lstvwgrp, ListViewGroupState state)
        {
            if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward allows collaps of ListViewGroups
                return;
            if (lstvwgrp == null || lstvwgrp.ListView == null)
                return;
            if (lstvwgrp.ListView.InvokeRequired)
                lstvwgrp.ListView.Invoke(new CallBackSetGroupState(setGrpState), lstvwgrp, state);
            else
            {
                int? GrpId = GetGroupID(lstvwgrp);
                int gIndex = lstvwgrp.ListView.Groups.IndexOf(lstvwgrp);
                LVGROUP group = new LVGROUP();
                group.CbSize = Marshal.SizeOf(group);
                group.State = state;
                group.Mask = ListViewGroupMask.State;
                if (GrpId != null)
                {
                    group.IGroupId = GrpId.Value;
                    SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, ref group);
                    SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, ref group);
                }
                else
                {
                    group.IGroupId = gIndex;
                    SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, ref group);
                    SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, ref group);
                }
                lstvwgrp.ListView.Refresh();
            }
        }

        private static void setGrpFooter(ListViewGroup lstvwgrp, string footer)
        {
            if (Environment.OSVersion.Version.Major < 6)
                return;
            if (lstvwgrp == null || lstvwgrp.ListView == null)
                return;
            if (lstvwgrp.ListView.InvokeRequired)
                lstvwgrp.ListView.Invoke(new CallbackSetGroupString(setGrpFooter), lstvwgrp, footer);
            else
            {
                int? GrpId = GetGroupID(lstvwgrp);
                int gIndex = lstvwgrp.ListView.Groups.IndexOf(lstvwgrp);
                LVGROUP group = new LVGROUP();
                group.CbSize = Marshal.SizeOf(group);
                group.PszFooter = footer;
                group.Mask = ListViewGroupMask.Footer;
                if (GrpId != null)
                {
                    group.IGroupId = GrpId.Value;
                    SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, GrpId.Value, ref group);
                }
                else
                {
                    group.IGroupId = gIndex;
                    SendMessage(lstvwgrp.ListView.Handle, LVM_SETGROUPINFO, gIndex, ref group);
                }
            }
        }

        public void SetGroupState(ListViewGroupState state)
        {
            LastState = state;

            foreach (ListViewGroup lvg in this.Groups)
                setGrpState(lvg, state);
        }

        public void SetGroupFooter(ListViewGroup lvg, string footerText)
        {
            setGrpFooter(lvg, footerText);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONUP)
                base.DefWndProc(ref m);
            base.WndProc(ref m);
        }

        private static ColumnHeader[] GetOrderedHeaders(ListView lv)
        {
            ColumnHeader[] arr = new ColumnHeader[lv.Columns.Count];

            foreach (ColumnHeader header in lv.Columns)
            {
                arr[header.DisplayIndex] = header;
            }

            return arr;
        }

        private bool EnumWindowCallBack(IntPtr hwnd, IntPtr lParam)
        {
            // Determine the rectangle of the ListView header bar and save it in _headerRect.
            RECT rct;

            if (!GetWindowRect(hwnd, out rct))
            {
                _headerRect = Rectangle.Empty;
            }
            else
            {
                _headerRect = new Rectangle(rct.Left, rct.Top, rct.Right - rct.Left, rct.Bottom - rct.Top);
            }

            return false; // Stop the enum
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), Description("LVGROUP StructureUsed to set and retrieve groups.")]
    public struct LVGROUP
    {
        public int CbSize;
        public ListViewGroupMask Mask;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszHeader;

        public int CchHeader;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszFooter;

        public int CchFooter;
        public int IGroupId;
        public int StateMask;
        public ListViewGroupState State;
        public uint UAlign;
        public IntPtr PszSubtitle;
        public uint CchSubtitle;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszTask;

        public uint CchTask;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszDescriptionTop;

        public uint CchDescriptionTop;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszDescriptionBottom;

        public uint CchDescriptionBottom;
        public int ITitleImage;
        public int IExtendedImage;
        public int IFirstItem;
        public IntPtr CItems;
        public IntPtr PszSubsetTitle;
        public IntPtr CchSubsetTitle;
    }

    public enum ListViewGroupMask
    {
        None = 0x00000,
        Header = 0x00001,
        Footer = 0x00002,
        State = 0x00004,
        Align = 0x00008,
        GroupId = 0x00010,
        SubTitle = 0x00100,
        Task = 0x00200,
        DescriptionTop = 0x00400,
        DescriptionBottom = 0x00800,
        TitleImage = 0x01000,
        ExtendedImage = 0x02000,
        Items = 0x04000,
        Subset = 0x08000,
        SubsetItems = 0x10000
    }

    public enum ListViewGroupState
    {
        /// <summary>
        /// Groups are expanded, the group name is displayed, and all items in the group are displayed.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// The group is collapsed.
        /// </summary>
        Collapsed = 1,
        /// <summary>
        /// The group is hidden.
        /// </summary>
        Hidden = 2,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group does not display a header.
        /// </summary>
        NoHeader = 4,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group can be collapsed.
        /// </summary>
        Collapsible = 8,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group has keyboard focus.
        /// </summary>
        Focused = 16,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group is selected.
        /// </summary>
        Selected = 32,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group displays only a portion of its items.
        /// </summary>
        SubSeted = 64,
        /// <summary>
        /// Version 6.00 and Windows Vista. The subset link of the group has keyboard focus.
        /// </summary>
        SubSetLinkFocused = 128,
    }
}
