using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace STS.General.GUI.Frames
{
    public class PromptedTextbox : TextBox
    {
        const int WM_SETFOCUS = 7;
        const int WM_KILLFOCUS = 8;
        const int WM_ERASEBKGND = 14;
        const int WM_PAINT = 15;

        private bool focusSelect = true;
        private bool drawPrompt = true;
        private string promptText = String.Empty;
        private Color promptColor = SystemColors.GrayText;
        private Font promptFont = null;
        private Image drawImage = null;

        public PromptedTextbox()
        {
            this.PromptFont = this.Font;
        }

        [Category("Appearance")]
        [DisplayName("Image")]
        public Image DrawIcon
        {
            get { return drawImage; }
            set { drawImage = value; }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("The prompt text to display when there is nothing in the Text property.")]
        public string PromptText
        {
            get { return promptText; }
            set { promptText = value.Trim(); this.Invalidate(); }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("The ForeColor to use when displaying the PromptText.")]
        public Color PromptForeColor
        {
            get { return promptColor; }
            set { promptColor = value; this.Invalidate(); }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        [Description("The Font to use when displaying the PromptText.")]
        public Font PromptFont
        {
            get { return promptFont; }
            set { promptFont = value; this.Invalidate(); }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Behavior")]
        [Description("Automatically select the text when control receives the focus.")]
        public bool FocusSelect
        {
            get { return focusSelect; }
            set { focusSelect = value; }
        }

        protected override void OnEnter(EventArgs e)
        {
            if (this.Text.Length > 0 && focusSelect)
                this.SelectAll();

            base.OnEnter(e);
        }

        protected override void OnTextAlignChanged(EventArgs e)
        {
            base.OnTextAlignChanged(e);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (drawPrompt && this.Text.Length == 0)
                DrawTextPrompt(e.Graphics);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WM_SETFOCUS:
                    drawPrompt = false;
                    break;

                case WM_KILLFOCUS:
                    drawPrompt = true;
                    break;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_PAINT && drawPrompt && this.Text.Length == 0 && !this.GetStyle(ControlStyles.UserPaint))
                DrawTextPrompt();

            if (drawImage != null)
                DrawImage();
        }

        protected void DrawImage()
        {
            using (Graphics g = this.CreateGraphics())
                g.DrawImage(drawImage, new Rectangle(ClientRectangle.X + (ClientRectangle.Width - 20), ClientRectangle.Y, 20, ClientRectangle.Height));
        }

        protected virtual void DrawTextPrompt()
        {
            using (Graphics g = this.CreateGraphics())
            {
                DrawTextPrompt(g);
            }
        }

        protected virtual void DrawTextPrompt(Graphics g)
        {
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.Top | TextFormatFlags.EndEllipsis;
            Rectangle rect = this.ClientRectangle;

            switch (this.TextAlign)
            {
                case HorizontalAlignment.Center:
                    flags = flags | TextFormatFlags.HorizontalCenter;
                    rect.Offset(0, 1);
                    break;
                case HorizontalAlignment.Left:
                    flags = flags | TextFormatFlags.Left;
                    rect.Offset(1, 1);
                    break;
                case HorizontalAlignment.Right:
                    flags = flags | TextFormatFlags.Right;
                    rect.Offset(0, 1);
                    break;
            }

            TextRenderer.DrawText(g, promptText, promptFont, rect, promptColor, this.BackColor, flags);
        }
    }
}
