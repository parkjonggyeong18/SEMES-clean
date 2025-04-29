using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI.WinForm.ChildForms
{
    public partial class FormRecoverPassword : Base.BaseFixedForm
    {
        public FormRecoverPassword()
        {
            InitializeComponent();
            lblMessage.Text = "";
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) == false)
            {
                var result = new Domain.Models.UserModel().RecoverPassword(txtUser.Text);
                if (result != null)
                {
                    lblMessage.Text = "Hi, " + result.FirstName +
                       ",\nPassword recovery sent to your email: " +
                       result.Email + "\nHowever, we ask that you change your password immediately once you enter the application.";
                    lblMessage.ForeColor = Color.DimGray;
                }
                else
                {
                    lblMessage.Text = "Sorry, you do not have an account associated with the email or username provided.";
                    lblMessage.ForeColor = Color.IndianRed;
                }
            }
            else
            {
                lblMessage.Text = "Please enter your username or email";
                lblMessage.ForeColor = Color.IndianRed;
            }
        }
    }
}
