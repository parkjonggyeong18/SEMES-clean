using Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.WinForm.Helpers;
using UI.WinForm.ViewModels;
using Infra.Common;

namespace UI.WinForm
{

    public partial class LoginForm : Form
    {
        #region -> Fields

        private DragControl dragControl;
        private string usernamePlaceholder; 
        private string passwordPlaceholder; 
        private Color placeholderColor; 
        private Color textColor;
        #endregion
        private static readonly Dictionary<string, string> PosMap =
    new Dictionary<string, string>         // C# 7.3 → 타입 전체 기재
{
    { "시스템 관리자",        Positions.SystemAdministrator },
    { "부장",            Positions.Accountant },
    { "과장",                Positions.AdministrativeAssistant },
    { "대리",                Positions.HMR },
    { "주임",                Positions.Receptionist },
    { "사원",              Positions.MarketingGuru }
};
        #region -> Constructor

        public LoginForm()
        {
            InitializeComponent();
            dragControl = new DragControl(this, this); 
            this.FormBorderStyle = FormBorderStyle.None;   // 캡션 테두리 제거
            this.ClientSize = new Size(840, 300);         // 클라이언트 영역 지정
            this.AutoScaleMode = AutoScaleMode.None;
            usernamePlaceholder = txtUsername.Text; 
            passwordPlaceholder = txtPassword.Text; 
            placeholderColor = txtUsername.ForeColor; 
            textColor = Color.DimGray; 

            lblDescription.Select(); 
        }
        #endregion

        #region -> Methods

        private void SetPlaceholder()
        {
           
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = usernamePlaceholder;
                txtUsername.ForeColor = placeholderColor;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.Text = passwordPlaceholder;
                txtPassword.ForeColor = placeholderColor;
                txtPassword.UseSystemPasswordChar = false;
            }
        }
        private void RemovePlaceHolder(TextBox textBox, string placeholderText)
        {
            if (textBox.Text == placeholderText)
            {
                textBox.Text = ""; 
                textBox.ForeColor = textColor;
                if (textBox == txtPassword) 
                    textBox.UseSystemPasswordChar = true;

            }
        }
        private void Login()
        {

         
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || txtUsername.Text == usernamePlaceholder)
            {
                ShowMessage("사내 닉네임이나 이메일을 입력하세요.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text == passwordPlaceholder)
            {
                ShowMessage("비밀번호를 입력하세요.");
                return;
            }
        

         
            var userModel = new UserModel().Login(txtUsername.Text, txtPassword.Text);
            if (userModel != null)
            {
                var userViewModel = new UserViewModel().MapUserViewModel(userModel);
                ActiveUser.Id = userViewModel.Id;//Load the data of the connected user.
                ActiveUser.Position = userViewModel.Position;

                string oriPos = userViewModel.Position;
                string engPos;
                ActiveUser.Position = PosMap.TryGetValue(oriPos, out engPos) ? engPos : oriPos;

                Form mainForm;//Define the field for the main form.
                
                if (ActiveUser.Position == Positions.GeneralManager || ActiveUser.Position == Positions.Accountant
                    || ActiveUser.Position == Positions.AdministrativeAssistant ||ActiveUser.Position==Positions.SystemAdministrator)
                {
                   
                    mainForm = new MainForm(userViewModel);
                }
                else if(ActiveUser.Position==Positions.HMR)
                {
                    mainForm = new ChildForms.FormUsers();
                }
                else if (ActiveUser.Position == Positions.Receptionist)
                {
                    mainForm = new ChildForms.FormPacients();
                }
                else if (ActiveUser.Position == Positions.MarketingGuru)
                {
                    mainForm = new ChildForms.FormReports();
                }
                else
                {
                    mainForm = null;
                    ShowMessage("로그인 권한 없음");
                    return;
                }
                this.Hide();
                var welcomeForm = new WelcomeForm(userViewModel.FullName);
                welcomeForm.ShowDialog();
                mainForm.FormClosed += new FormClosedEventHandler(MainForm_SessionClosed);
                mainForm.Show();
            }
            else 
                ShowMessage("닉네임/이메일과 비밀번호가 맞지 않습니다.");

        }
        private void Logout()
        {
            this.Show();
            txtUsername.Clear();
            txtPassword.Clear();
            SetPlaceholder();
            ActiveUser.Id = 0;
            ActiveUser.Position = "";
            lblDescription.Select();
            lblErrorMessage.Visible = false;
        }
        private void ShowMessage(string message)
        {
            lblErrorMessage.Text = "    " + message;
            lblErrorMessage.Visible = true;
        }
        #endregion

        #region -> Event methods

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login();
        }
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Login();
        }
        private void MainForm_SessionClosed(object sender, FormClosedEventArgs e)
        {
            Logout();
        }
  
        private void LoginForm_Paint(object sender, PaintEventArgs e)
        {
        
            e.Graphics.DrawLine(new Pen(Color.Gray, 1), txtPassword.Location.X, txtPassword.Bottom + 5, txtPassword.Right, txtPassword.Bottom + 5);
            
            e.Graphics.DrawLine(new Pen(Color.Gray, 1), txtUsername.Location.X, txtUsername.Bottom + 5, txtUsername.Right, txtUsername.Bottom + 5);
         
            e.Graphics.DrawRectangle(new Pen(Color.Gray), 0, 0, this.Width - 1, this.Height - 1);
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
         
            e.Graphics.DrawLine(new Pen(lblDescription.ForeColor, 1), lblDescription.Location.X, lblDescription.Top - 5, lblDescription.Right - 5, lblDescription.Top - 5);
        }

        private void txtUsername_Enter(object sender, EventArgs e)
        {
          
            RemovePlaceHolder(txtUsername, usernamePlaceholder);
        }
        private void txtUsername_Leave(object sender, EventArgs e)
        {
           
            SetPlaceholder();
        }
        private void txtPassword_Enter(object sender, EventArgs e)
        {
          
            RemovePlaceHolder(txtPassword, passwordPlaceholder);
        }
        private void txtPassword_Leave(object sender, EventArgs e)
        {
          
            SetPlaceholder();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
            
        }
        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void lblForgotPass_Click(object sender, EventArgs e)
        {
            
            var frm = new ChildForms.FormRecoverPassword();
            frm.ShowDialog();
        }
        #endregion

        #region -> Overrides

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style &= ~0x00C00000;            // WS_CAPTION 끄기
                cp.Style |= 0x00080000             // WS_SYSMENU
                          | 0x00020000;            // WS_MINIMIZEBOX
                return cp;
            }
        }
        
        #endregion

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void lblDescription_Click(object sender, EventArgs e)
        {

        }
    }
}
