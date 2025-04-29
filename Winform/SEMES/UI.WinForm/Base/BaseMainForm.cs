using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI.WinForm.Base
{
    public class BaseMainForm : Form
    {
        /// <summary>
        /// This class inherits from the Form class of the windows form library.
        /// This class is responsible for implementing the resizing method
        /// (Resize the form from the lower right corner), Minimize,
        /// Close, Maximize and restore.
        /// </summary>

        #region -> Fields

        private int tolerance = 12; // Tolerance for resizing.
        private const int WM_NCHITTEST = 0x0084; // Win32, Mouse input notification: determines which part of the window corresponds to a point, allows to resize the form.
        private const int WS_MINIMIZEBOX = 0x20000; // Native methods: represents a window style that has a minimize button
        private const int HTBOTTOMRIGHT = 17; // Lower right corner of the border of a window, allows you to change the size diagonally to the right.
        private Rectangle sizeGripRectangle; // Size Grip in the lower right corner of a window.
        protected Panel PanelClientArea; // Client area of the form.
        #endregion

        #region -> Constructor

        public BaseMainForm()
        {
            this.Padding = new Padding(1);
            PanelClientArea = new Panel();
            PanelClientArea.BackColor = Color.WhiteSmoke;
            PanelClientArea.Dock = DockStyle.Fill;
            this.Controls.Add(PanelClientArea);
        }
        #endregion

        #region -> Overridden methods

        protected override void WndProc(ref Message m)
        {//WindowProc function: Windows / OS level message processing override

            switch (m.Msg)
            {
                case WM_NCHITTEST://If the Windows message is WM_NCHITTEST
                    base.WndProc(ref m);
                    if (this.WindowState == FormWindowState.Normal)//Resize the form if it is in normal state.
                    {
                        var hitPoint = this.PointToClient(new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16));
                        if (sizeGripRectangle.Contains(hitPoint))
                            m.Result = new IntPtr(HTBOTTOMRIGHT);//Resize diagonally to the right.
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }

        }
        protected override void OnSizeChanged(EventArgs e)
        {//This event occurs when the form changes size.
            base.OnSizeChanged(e);
            // Create a region with the size of the form.
            var region = new Region(new Rectangle(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height));
            // Create a new rectangle for the size control with the dimensions of the form minus the value of the tolerance (Coordinate) and the size of the tolerance (12px).
            sizeGripRectangle = new Rectangle(this.ClientRectangle.Width - tolerance, this.ClientRectangle.Height - tolerance, tolerance, tolerance);

            region.Exclude(sizeGripRectangle); // Exclude a portion of the region for size Grip.
            this.PanelClientArea.Region = region; // Set the created region.
            this.Invalidate(); // Redraw the form.
        }
        protected override void OnPaint(PaintEventArgs e)
        {//This event occurs when the form is drawn or redrawn.
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            SolidBrush blueBrush = new SolidBrush(Color.WhiteSmoke);
            e.Graphics.FillRectangle(blueBrush, sizeGripRectangle); // Draw a filled rectangle at the coordinates of the size control.
            ControlPaint.DrawSizeGrip(e.Graphics, Color.WhiteSmoke, sizeGripRectangle); // Draw the size grip (diagonal lines)
            e.Graphics.DrawRectangle(new Pen(Color.RoyalBlue), 0, 0, this.Width - 1, this.Height - 1); // Draw the border of the form.
        }

        protected override CreateParams CreateParams
        {//Override form creation parameters
            get
            {
                CreateParams param = base.CreateParams;
                param.Style |= WS_MINIMIZEBOX; //Set a minimize box in the window style / Will allow to minimize the form from the taskbar icon.
                return param;
            }
        }
        #endregion

        #region -> Métodos

        protected void Minimize()
        {
            this.WindowState = FormWindowState.Minimized;
        }
        protected void MaximizeRestore()
        {
            if (this.WindowState == FormWindowState.Normal)//Maximizar el formulario
            {
                /*When maximizing a borderless form, it covers the entire screen by hiding the taskbar,
                  * for this it is necessary to specify a maximum size:*/
                this.MaximumSize = Screen.FromHandle(this.Handle).WorkingArea.Size;//Set the size of the desktop area as the maximum size of the form.
                this.WindowState = FormWindowState.Maximized;
                this.Padding = new Padding(0);//Hide the border.
            }
            else//Restore the size of the form.
            {
                this.WindowState = FormWindowState.Normal;
                this.Padding = new Padding(1);//Display the border.
            }
        }
        protected void CloseApp()
        {
            if (MessageBox.Show("Are you sure to close the application?", "Message",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                Application.Exit();//Close the entire application ending all processes.
        }
        #endregion

    }
}
