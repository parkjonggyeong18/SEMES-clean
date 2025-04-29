using Domain.Models;
using Domain.Models.Contracts;
using Infra.Common;
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
using UI.WinForm.Utils;
using UI.WinForm.ViewModels;

namespace UI.WinForm.ChildForms
{
    public partial class FormUserMaintenance : Base.BaseFixedForm
    {
        /// <summary>
        /// Esta clase hereda de clase BaseFixedForm.
        /// </summary>

        #region -> Fields

        private IUserModel domainModel; // Domain model interface User.
        private BindingList<UserViewModel> userCollection; // Collection of users for bulk insert.
        private UserViewModel userViewModel; // User's view model.
        private TransactionAction transaction; // Transaction action for persistence.
        private TransactionAction listOperation = TransactionAction.Add; // Transaction action for the user collection.
        private Image defaultPhoto = Properties.Resources.defaultImageProfileUser; // Default photo for users who do not have a photo added.
        private string lastRecord = ""; /*Field to store the last data inserted or edited.
                                          This will allow you to select and view the changes in the datagridview of the Users form. */
        #endregion

        #region -> Properties

        public string LastRecord
        {/*Property to store the last data inserted or edited.
           This will allow you to select and view the changes in the datagridview of the Users form.*/
            get { return lastRecord; }
            set { lastRecord = value; }
        }
        #endregion

        #region -> Constructor

        public FormUserMaintenance(UserViewModel _userViewModel, TransactionAction _transaction)
        {
            InitializeComponent();

            //Initialize fields
            domainModel = new UserModel();
            userCollection = new BindingList<UserViewModel>();
            userViewModel = _userViewModel;
            transaction = _transaction;

            //Initialize control properties
            rbSingleInsert.Checked = true;
            cmbPosition.DataSource = Positions.GetPositions();
            dataGridView1.DataSource = userCollection;
            FillFields(_userViewModel);
            InitializeTransactionUI();
            InitializeDataGridView();
        }
        #endregion

        #region -> Methods

        private void InitializeTransactionUI()
        {//This method is responsible for setting the appearance properties based on the action of the transaction.
            switch (transaction)
            {
                case TransactionAction.View:
                    LastRecord = null;
                    this.TitleBarColor = Color.MediumSlateBlue;
                    lblTitle.Text = "User details";
                    lblTitle.ForeColor = Color.MediumSlateBlue;
                    btnSave.Visible = false;
                    panelAddedControl.Visible = false;
                    lblCurrentPass.Visible = false;
                    txtCurrentPass.Visible = false;
                    lblConfirmPass.Visible = false;
                    txtConfirmPass.Visible = false;
                    btnCancel.Text = "X  Close";
                    btnCancel.Location = new Point(300, 310);
                    btnCancel.BackColor = Color.MediumSlateBlue;
                    ReadOnlyFields();
                    break;

                case TransactionAction.Add:
                    this.TitleBarColor = Color.SeaGreen;
                    lblTitle.Text = "Add new user";
                    lblTitle.ForeColor = Color.SeaGreen;
                    btnSave.BackColor = Color.SeaGreen;
                    cmbPosition.SelectedIndex = -1;
                    lblCurrentPass.Visible = false;
                    txtCurrentPass.Visible = false;
                    break;

                case TransactionAction.Edit:
                    this.TitleBarColor = Color.RoyalBlue;
                    lblTitle.Text = "Edit user";
                    lblTitle.ForeColor = Color.RoyalBlue;
                    btnSave.BackColor = Color.RoyalBlue;
                    panelAddedControl.Visible = false;
                    lblCurrentPass.Visible = false;
                    txtCurrentPass.Visible = false;
                    break;

                case TransactionAction.Remove:
                    this.TitleBarColor = Color.IndianRed;
                    lblTitle.Text = "Remove user";
                    lblTitle.ForeColor = Color.IndianRed;
                    btnSave.Text = "Remove";
                    btnSave.BackColor = Color.IndianRed;
                    panelAddedControl.Visible = false;
                    lblCurrentPass.Visible = false;
                    txtCurrentPass.Visible = false;
                    ReadOnlyFields();
                    break;

                case TransactionAction.Special:
                    this.TitleBarColor = Color.RoyalBlue;
                    lblTitle.Text = "Update my user profile";
                    lblTitle.ForeColor = Color.RoyalBlue;
                    btnSave.BackColor = Color.RoyalBlue;
                    panelAddedControl.Visible = false;
                    lblPassword.Text = "New password";
                    cmbPosition.Enabled = false;
                    break;
            }
        }
        private void InitializeDataGridView()
        {//This method is responsible for adding edit and remove columns 
            //from the user collection of bulk insert option.

            DataGridViewImageColumn EditColumn = new DataGridViewImageColumn();
            DataGridViewImageColumn DeleteColumn = new DataGridViewImageColumn();

            EditColumn.Image = Properties.Resources.editIcon;
            EditColumn.Name = "EditColumn";
            EditColumn.HeaderText = " ";
            DeleteColumn.Image = Properties.Resources.deleteIcon;
            DeleteColumn.Name = "DeleteColumn";
            DeleteColumn.HeaderText = " ";

            this.dataGridView1.Columns.Add(EditColumn);
            this.dataGridView1.Columns.Add(DeleteColumn);

            dataGridView1.Columns["EditColumn"].Width = 25;
            dataGridView1.Columns["DeleteColumn"].Width = 25;
            dataGridView1.Columns[0].Width = 40;
            dataGridView1.Columns[1].Width = 100;
        }

        private void PersistSingleRow()
        {//Method to persist a single row in the database.
            try
            {
                var userObject = FillViewModel();//Get view model.
                var validateData = new DataValidation(userObject);//Validate fields of the object.
                var validatePassword = txtPassword.Text == txtConfirmPass.Text;//Validate passwords.

                if (validateData.Result == true && validatePassword == true)//If the object is valid.
                {
                    var userModel = userViewModel.MapUserModel(userObject);//Map view model to domain model.
                    switch (transaction)
                    {
                        case TransactionAction.Add://Add user
                            AddUser(userModel);
                            break;
                        case TransactionAction.Edit://Edit user
                            EditUser(userModel);
                            break;
                        case TransactionAction.Remove://Remove user
                            RemoveUser(userModel);
                            break;
                        case TransactionAction.Special://Update user profile
                            if (string.IsNullOrWhiteSpace(txtCurrentPass.Text) == false)
                            {
                                if (txtCurrentPass.Text == userViewModel.Password)//For security, validate the user's current password.
                                    EditUser(userModel);
                                else
                                    MessageBox.Show("Your current password is incorrect", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                                MessageBox.Show("Please enter your current password", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                    }
                }

                else
                {
                    if (validateData.Result == false)
                        MessageBox.Show(validateData.ErrorMessage, "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                        MessageBox.Show("Passwords do not match", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                LastRecord = null;//Set null as last record.
                var message = ExceptionManager.GetMessage(ex);//Get exception message.
                MessageBox.Show(message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);//Show message.
            }

        }
        private void PersistMultipleRows()
        {//Method to persist multiple rows in the database (Bulk insert).
            try
            {
                if (userCollection.Count > 0)//Validate if there is data to insert.
                {
                    var userModelList = userViewModel.MapUserModel(userCollection.ToList());//Map user collection to domain model collection.
                    switch (transaction)
                    {
                        case TransactionAction.Add:
                            AddUserRange(userModelList);//Add user range.
                            break;
                    }
                }
                else MessageBox.Show("There is no data, please add data in the table", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                LastRecord = null;
                var message = ExceptionManager.GetMessage(ex);
                MessageBox.Show(message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void AddUser(UserModel userModel)
        {
            var result = domainModel.Add(userModel);
            if (result > 0)
            {
                LastRecord = userModel.Username;//Set the last record.
                MessageBox.Show("User added successfully", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                LastRecord = null;
                MessageBox.Show("Operation was not performed, try again", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void AddUserRange(List<UserModel> userModelList)
        {
            var result = domainModel.AddRange(userModelList);

            if (result > 0)
            {
                LastRecord = userModelList[0].Username;//Set the first object as the last record.
                MessageBox.Show(result.ToString()+" users added successfully.");
                this.Close();
            }
            else
            {
                lastRecord = null;
                MessageBox.Show("Operation was not performed, try again", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void EditUser(UserModel userModel)
        {
            var result = domainModel.Edit(userModel);
            if (result > 0)
            {
                LastRecord = userModel.Username;//Set the last record.
                MessageBox.Show("User updated successfully", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                LastRecord = null;
                MessageBox.Show("Operation was not performed, try again", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void RemoveUser(UserModel userModel)
        {
            var result = domainModel.Remove(userModel);
            if (result > 0)
            {
                LastRecord = "";//Set an empty string as the last record, since the user no longer exists, therefore it is not possible to select and view the changes (See Users form).
                MessageBox.Show("User deleted successfully", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                LastRecord = null;
                MessageBox.Show("Operation was not performed, try again", "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ModifyUserCollection()
        {//This method is responsible for adding or modifying a user from the user collection for bulk insert.
            var userObject = FillViewModel();

            var validateData = new DataValidation(userObject);//Validate object.
            var validatePassword = txtPassword.Text == txtConfirmPass.Text;

            if (validateData.Result == true && validatePassword == true)
            {
                switch (listOperation)
                {
                    case TransactionAction.Add:
                        var findUser = userCollection.FirstOrDefault(user => user.Email == userObject.Email
                                                                   || user.Username == userObject.Username);
                        if (findUser == null)
                        {
                            var lastUser = userCollection.LastOrDefault();
                            if (lastUser == null) userObject.Id = 1;
                            else userObject.Id = lastUser.Id + 1;

                            userCollection.Add(userObject);
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Duplicate data.\nEmail or username has already been added",
                                "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;

                    case TransactionAction.Edit:
                        var findObject = userCollection.SingleOrDefault(user => user.Id == userViewModel.Id);
                        var index = userCollection.IndexOf(findObject);
                        userCollection[index] = userObject;

                        userCollection.ResetBindings();
                        ClearFields();
                        break;
                }

            }
            else
            {
                if (validateData.Result == false)
                    MessageBox.Show(validateData.ErrorMessage, "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else MessageBox.Show("Passwords do not match", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FillFields(UserViewModel userView)
        {//Load the data from the view model into the fields of the form.
            txtUsername.Text = userView.Username;
            txtPassword.Text = userView.Password;
            txtConfirmPass.Text = userView.Password;
            txtFirstName.Text = userView.FirstName;
            txtLastName.Text = userView.LastName;
            cmbPosition.Text = userView.Position;
            txtEmail.Text = userView.Email;
            if (userView.Photo != null)
                PictureBoxPhoto.Image = ItemConverter.BinaryToImage(userView.Photo);
            else PictureBoxPhoto.Image = defaultPhoto;
        }
        private UserViewModel FillViewModel()
        {//Fill and return the data of the form fields in a new view model object.
            var userView = new UserViewModel();

            userView.Id = userViewModel.Id;
            userView.Username = txtUsername.Text;
            userView.Password = txtPassword.Text;
            userView.FirstName = txtFirstName.Text;
            userView.LastName = txtLastName.Text;
            userView.Position = cmbPosition.Text;
            userView.Email = txtEmail.Text;
            if (PictureBoxPhoto.Image == defaultPhoto)
                userView.Photo = null;
            else userView.Photo = ItemConverter.ImageToBinary(PictureBoxPhoto.Image);

            return userView;
        }

        private void ClearFields()
        {//Clear the form fields.
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPass.Clear();
            txtCurrentPass.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            PictureBoxPhoto.Image = defaultPhoto;
            cmbPosition.SelectedIndex = -1;

            listOperation = TransactionAction.Add;
            btnAddUser.Text = "Add";
            btnAddUser.BackColor = Color.CornflowerBlue;
        }
        private void ReadOnlyFields()
        {//Make the form fields read-only.
            txtUsername.ReadOnly = true;
            txtPassword.ReadOnly = true;
            txtConfirmPass.ReadOnly = true;
            txtCurrentPass.ReadOnly = true;
            txtFirstName.ReadOnly = true;
            txtLastName.ReadOnly = true;
            txtEmail.ReadOnly = true;
            btnAddPhoto.Enabled = false;
            btnDeletePhoto.Enabled = false;
            cmbPosition.Enabled = false;
        }
        #endregion

        #region -> Event methods

        private void btnSave_Click(object sender, EventArgs e)
        {//Save changes button

            if (rbSingleInsert.Checked) // If the radio button is checked.
                PersistSingleRow(); // Execute the persist single row method.
            else // Otherwise, execute the method of persisting multiple rows (Bulk insert)
                PersistMultipleRows();
        }
        private void btnAddUserList_Click(object sender, EventArgs e)
        {//Add user to user collection button for bulk insertion.
            ModifyUserCollection();
        }

        private void FormUserMaintenance_Load(object sender, EventArgs e)
        {

        }
        private void rbSingleInsert_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSingleInsert.Checked)//Change appearance for single insert.
            {
                panelMultiInsert.Visible = false;
                btnCancel.Location = new Point(210, 310);
                btnSave.Location = new Point(386, 310);
                this.Size = new Size(754, 370);
            }
            else //Change appearance for bulk insert.
            {
                panelMultiInsert.Visible = true;
                btnCancel.Location = new Point(212, 654);
                btnSave.Location = new Point(388, 654);
                this.Size = new Size(754, 715);
            }
        }

        private void btnAddPhoto_Click(object sender, EventArgs e)
        {//Add an image to the image box for the user's photo.
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Images(.jpg,.png)|*.png;*.jpg";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                PictureBoxPhoto.Image = new Bitmap(openFile.FileName);
            }
        }
        private void btnDeletePhoto_Click(object sender, EventArgs e)
        {//Delete user photo
            PictureBoxPhoto.Image = defaultPhoto;
        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {//Change the cursor if the mouse pointer enters the edit or delete column.
            if (e.ColumnIndex == dataGridView1.Columns["EditColumn"].Index
                || e.ColumnIndex == dataGridView1.Columns["DeleteColumn"].Index)
            {
                dataGridView1.Cursor = Cursors.Hand;
            }
        }
        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {//Change the cursor if the mouse pointer enters the edit or delete column.
            if (e.ColumnIndex == dataGridView1.Columns["EditColumn"].Index
                || e.ColumnIndex == dataGridView1.Columns["DeleteColumn"].Index)
            {
                dataGridView1.Cursor = Cursors.Default;
            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {//Delete or edit a user from the user collection.
            if (e.RowIndex == dataGridView1.NewRowIndex || e.RowIndex < 0)
                return;

            if (e.ColumnIndex == dataGridView1.Columns["DeleteColumn"].Index)
            {
                if (listOperation != TransactionAction.Edit)
                    userCollection.RemoveAt(e.RowIndex);
                else MessageBox.Show("Data is being edited, please finish the operation.");
            }
            if (e.ColumnIndex == dataGridView1.Columns["EditColumn"].Index)
            {
                userViewModel = userCollection[e.RowIndex];
                FillFields(userViewModel);
                listOperation = TransactionAction.Edit;
                btnAddUser.Text = "Update";
                btnAddUser.BackColor = Color.MediumSlateBlue;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {//If the action is canceled set null as last record.
            LastRecord = null;
            this.Close();
        }
        #endregion

        protected override void CloseForm()
        {//If the form is closed, set null as last record.
            base.CloseForm();
            LastRecord = null;
        }

    }
}
