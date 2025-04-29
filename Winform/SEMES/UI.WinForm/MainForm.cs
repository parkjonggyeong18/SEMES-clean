using Infra.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.WinForm.ChildForms;
using UI.WinForm.Helpers;
using UI.WinForm.ViewModels;

namespace UI.WinForm
{
    public partial class MainForm : Base.BaseMainForm
    {
        /// <summary>
        /// This class inherits from the BaseMainForm class.
        /// </summary>

        #region -> Fields

        private DragControl dragControl; // Lets you drag the form.
        private UserViewModel connectedUser; // Gets or sets the connected user.
        private List<Form> listChildForms; // Gets or sets the child forms open on the form's desktop panel.
        private Form activeChildForm; // Gets or sets the currently displayed child form.
        #endregion

        #region -> Constructors

        public MainForm()
        {//Use this constructor if you don't want to show the data of the connected user.
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            dragControl = new DragControl(panelTitleBar, this);
            listChildForms = new List<Form>();
            connectedUser = new UserViewModel();
            linkProfile.Visible = false;
        }
        public MainForm(UserViewModel _connectedUser)
        {//Use this constructor at login and submit a user view model
            // to display the user's data in the side menu.

            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            dragControl = new DragControl(panelTitleBar, this);
            listChildForms = new List<Form>();
            connectedUser = _connectedUser;
        }
        #endregion

        #region -> Methods

        public void LoadUserData()
        {//Load the data of the connected user in the side menu.
            lblName.Text = connectedUser.FirstName;
            lblLastName.Text = connectedUser.LastName;
            lblPosition.Text = connectedUser.Position;
            if (connectedUser.Photo != null)
                pictureBoxPhoto.Image = Utils.ItemConverter.BinaryToImage(connectedUser.Photo);
            else pictureBoxPhoto.Image = Properties.Resources.defaultImageProfileUser;
        }
        private void ManagePermissions()
        {//Manage user permissions, this is just a demo,
            // You can remove it if you don't need it.
            switch (ActiveUser.Position)
            {
                case Positions.GeneralManager:

                    break;
                case Positions.Accountant:
                    btnUsers.Enabled = false;
                    btnPacients.Enabled = false;
                    btnHistory.Enabled = false;
                    btnCalendar.Enabled = false;
                    break;
                case Positions.AdministrativeAssistant:
                    btnReports.Enabled = false;
                    break;
                case Positions.Receptionist:
                    btnReports.Enabled = false;
                    btnUsers.Enabled = false;
                    break;
                case Positions.HMR:

                    break;
                case Positions.MarketingGuru:

                    break;
                case Positions.SystemAdministrator:

                    break;
            }
        }

        //This method is responsible for displaying a child form on the desktop panel of the main form.
        private void OpenChildForm<childForm>(Func<childForm> _delegate, object senderMenuButton) where childForm : Form
        {
            ///Generic method with a generic delegate parameter (Func <TResult>) where the data type is Form.
            ///This method ALLOWS to open forms WITH or WITHOUT parameters within the desktop panel. On many occasions,
            ///the youtube tutorials used methods similar to this. However, it simply allowed forms to be opened 
            ///WITHOUT parameters ( e.g <see cref="new MyForm ()"/> )
            ///and it was impossible to open a form WITH parameters( e.g. <see cref="new MyForm (user:'John', mail:'john@gmail.com'"/>)
            ///so this method solves this defect thanks to the generic delegate (Func <TResult>)   

            Button menuButton = (Button)senderMenuButton;//Button where the form is opened, you can send a null value, if you are not trying to open a form from a different control than the side menu buttons.
            Form form = listChildForms.OfType<childForm>().FirstOrDefault();//Find if the form is already instantiated or has been displayed before.

            if (activeChildForm != null && form == activeChildForm)//If the form is the same as the current active form, return and do nothing.
                return;

           if (form == null) // If the form does not exist, then create the instance and display it on the desktop panel.
            {

                form = _delegate (); // Execute the delegate
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None; // Remove the border.
                form.TopLevel = false; // Indicate that the form is not top-level
                form.Dock = DockStyle.Fill; // Set the dock style to full (Fill the desktop panel)
                listChildForms.Add (form); // Add child form to the list of forms.

                if (menuButton!= null) // If the menu button is other than null:
                {
                    ActivateButton (menuButton); // Activate / Highlight the button.
                    form.FormClosed += (s, e) =>
                    {// When the form closes, deactivate the button.
                        DeactivateButton (menuButton);
                    };
                }
                btnChildFormClose.Visible = true; // display the child form close button.
            }
            CleanDesktop (); // Remove the current child form from the desktop panel
            panelDesktop.Controls.Add (form); // add child form to desktop panel
            panelDesktop.Tag = form; // Store the form
            form.Show (); // Show the form
            form.BringToFront (); // Bring to front
            form.Focus (); // Focus the form
            lblCaption.Text = form.Text; // Set the title of the form.
            activeChildForm = form; // Set as active form.


            /// <Note>
            /// You can use the Func <TResult> delegate with anonymous methods or lambda expression,
            /// For example, we can call this method in the following way: Suppose we are in the click event method of some button.
            /// With anonymous method:
            /// <see cref = "OpenChildForm (delegate () {return new MyForm ('MyParameter');});" />
            /// With lambda expression (My favorite)
            /// <see cref = "OpenChildForm (() => new MyForm ('id', 'username'));" />
            /// </Note>
        }

        private void CloseChildForm()
        {//Close active child form.

            if (activeChildForm != null)
            {
                listChildForms.Remove(activeChildForm); // Remove from the list of forms.
                panelDesktop.Controls.Remove(activeChildForm); // Remove from the desktop panel.
                activeChildForm.Close(); // Close the form.
                RefreshDesktop(); // Refresh the desktop panel (Show the previous form if it is the case, otherwise restore the main form)
            }
        }
        private void CleanDesktop()
        {//Clean the desktop.

            if (activeChildForm != null)
            {
                activeChildForm.Hide();
                panelDesktop.Controls.Remove(activeChildForm);
            }
            /*This method hides and removes the active child form from the desktop panel, so there will only be
            one child form open within the desktop panel, since adding a new form removes the old form and adds 
            the new one (check the OpenChildForm method) Inactive child forms are stored in a generic list.

            I created these methods to get rid of the doubts as many think (myself included) that having 20 or 
            50 forms added within the desktop panel affects performance, well I didn't realize that. In fact, it is 
            possible to add 10 thousand controls dynamically in a displayed form and there is no limit if it is
            added from the form's constructor, for experimenting, I added 100 thousand labels and 10 thousand 
            panels with color although it took more than 10 minutes to show ( pc: 8 ram, SixCore 3.50 GHz cpu) 
            and there is no performance problem (consumed 20mb ram) and scrolling the form works fine.

            Therefore, if these methods seem very tedious or difficult to understand, you can use the methods 
            of opening a child form within the Previous Projects panel (tutorials) that are shown on YouTube,
            where the secondary forms are stored within the desktop panel, they overlap one after the other and one 
            is displayed (form.bringtofront ();).

            However, it still doesn't seem appropriate to me to add so many forms within a panel considering 
            that a default form is top-level and I don't like the idea of ​​having so many controls in a panel (child form controls).

            As a result, I preferred to store the child forms in a generic list and add and display only a 
            single form on the desktop panel :) */
        }
        private void RefreshDesktop()
        {// This method is responsible for updating the main form with the proper parameters,
            // either reset the default values or show a previous child form if that's the case.
            var childForm = listChildForms.LastOrDefault();//Check and get the last (old) child form in the form list
            if (childForm != null)//if there is an instantiated child form in the list, add it back to the desktop panel.
            {
                activeChildForm = childForm;
                panelDesktop.Controls.Add(childForm);
                panelDesktop.Tag = childForm;
                childForm.Show();
                childForm.BringToFront();
                lblCaption.Text = childForm.Text;
            }
            else //If there is no result, there is no instance in the list of child forms.
            {
                ResetDefaults();//Reset main form to defaults
            }
        }
        private void ResetDefaults()
        {
            activeChildForm = null;
            lblCaption.Text = "   Home";
            btnChildFormClose.Visible = false;
        }

        private void ActivateButton(Button menuButton)
        {
            menuButton.ForeColor = Color.RoyalBlue;
            //menuButton.BackColor = panelMenuHeader.BackColor;
        }
        private void DeactivateButton(Button menuButton)
        {
            menuButton.ForeColor = Color.DarkGray;
            //menuButton.BackColor = panelSideMenu.BackColor;
        }

        #endregion
        
        #region -> Event methods

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadUserData(); // Load user data.
            ManagePermissions(); // Manage user permissions.
            ResetDefaults(); // Load default values.
        }

        #region - Log out, Close application, minimize and maximize.

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("로그아웃 하시겠어요?", "Message",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                this.Close();//Close the form
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.CloseApp();//Close the application.
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            this.MaximizeRestore();
            if (this.WindowState == FormWindowState.Maximized)
                btnMaximize.Image = Properties.Resources.btnRestore;
            else btnMaximize.Image = Properties.Resources.btnMaximize;
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.Minimize();
        }
        private void btnChildFormClose_Click(object sender, EventArgs e)
        {
            CloseChildForm();
        }
        #endregion

        #region - Convert profile photo to circular

        private void pictureBoxPhoto_Paint(object sender, PaintEventArgs e)
        {
            using (GraphicsPath graphicsPath = new GraphicsPath())
            {
                var rectangle = new Rectangle(0, 0, pictureBoxPhoto.Width - 1, pictureBoxPhoto.Height - 1);
                graphicsPath.AddEllipse(rectangle);
                pictureBoxPhoto.Region = new Region(graphicsPath);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var pen = new Pen(new SolidBrush(pictureBoxPhoto.BackColor), 1);
                e.Graphics.DrawEllipse(pen, rectangle);
            }
        }
        #endregion

        #region - Collapse or Expand side menu

        private void btnSideMenu_Click(object sender, EventArgs e)
        {
            if (panelSideMenu.Width > 100)
            {
                panelSideMenu.Width = 52;
                foreach (Control control in panelMenuHeader.Controls)
                {
                    if (control != btnSideMenu)
                        control.Visible = false;
                }
            }
            else
            {
                panelSideMenu.Width = 230;
                foreach (Control control in panelMenuHeader.Controls)
                {
                    control.Visible = true;
                }
            }
        }
        #endregion

        #region - Open child forms

        private void btnUsers_Click(object sender, EventArgs e)
        {
            OpenChildForm(() => new FormUsers(), sender);
            // () =>: Call the generic delegate - instantiate a form without parameters.
            // sender: button users.
        }

        private void btnPacients_Click(object sender, EventArgs e)
        {
            OpenChildForm(() => new FormPacients(), sender);
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            OpenChildForm(() => new FormHistory(), sender);
        }

        private void btnCalendar_Click(object sender, EventArgs e)
        {
            OpenChildForm(() => new FormCalendar(), sender);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            OpenChildForm(() => new FormReports(), sender);
        }

        private void linkProfile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // If the control is not a side menu button, send NULL as a parameter.
            OpenChildForm(() => new FormUserProfile(connectedUser, this), null);
            // () =>: Call the generic delegate - instantiate a form with the user's view model parameter.
            // sender: NULL, because it is not a button.
        }
        #endregion

        #endregion

    }
}
