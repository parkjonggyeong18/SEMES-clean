using Domain.Models;
using Domain.Models.Contracts;
using Infra.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UI.WinForm.ViewModels;

namespace UI.WinForm.ChildForms
{
    public partial class FormUsers : Form
    {
        #region -> Fields

        private IUserModel domainModel = new UserModel(); // User's domain model.
        private UserViewModel userViewModel = new UserViewModel(); // User's view model.
        private List<UserViewModel> userViewList; // List of users.
        private FormUserMaintenance maintenanceForm; // maintenance form.
        #endregion

        #region -> Constructor

        public FormUsers()
        {
            InitializeComponent();
        }
        #endregion

        #region -> Methods

        private void LoadUserData()
        {// Fill the data grid with the list of users.
            userViewList = userViewModel.MapUserViewModel(domainModel.GetAll()); // Get all users.
            dataGridView1.DataSource = userViewList; // Set the data source.
        }
        private void FindUser(string value)
        { //Search users.
            userViewList = userViewModel.MapUserViewModel(domainModel.GetByValue(value)); // Filter user by value.
            dataGridView1.DataSource = userViewList; // Set the data source with the results.
        }
        private UserViewModel GetUser(int id)
        {//Get user by ID.
            var userModel = domainModel.GetSingle(id.ToString()); // Find a single user.
            if (userModel != null) // If there is a result, return a user view model object.
                return userViewModel.MapUserViewModel(userModel);
            else // Otherwise, return a null value, and show message.
            {
                MessageBox.Show("No user with id " + id.ToString() + " found", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }
        #endregion

        #region -> Event methods

        private void FormUsers_Load(object sender, EventArgs e)
        {
            LoadUserData(); // Load data.
            dataGridView1.Columns[0].Width = 50; // Set a fixed width for the ID column.
            dataGridView1.Columns[1].Width = 100; // Set a fixed width for the Username column.
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            FindUser(txtSearch.Text);//Search user.
        }
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FindUser(txtSearch.Text);//Find user if enter key is pressed in search text box.
            }
        }

        private void btnDetalles_Click(object sender, EventArgs e)
        {// Show user details.
            if (dataGridView1.RowCount <= 0)
            {
                MessageBox.Show("No data to select", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var user = GetUser((int)dataGridView1.CurrentRow.Cells[0].Value); // Get user ID and search using GetUser(id) method.
                if (user == null) return;
                var frm = new FormUserMaintenance(user, TransactionAction.View); // Instantiate form, and send parameters (view and action model).
                frm.ShowDialog(); // Show form.
            }
            else
                MessageBox.Show("Please select a row", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {//Add new user.
            maintenanceForm = new FormUserMaintenance(new UserViewModel(), TransactionAction.Add); // Instantiate form, and send parameters (view model and action ).
            maintenanceForm.FormClosed += new FormClosedEventHandler(MaintenanceFormClosed); // Associate closed event, to update the datagrdiview after the maintenance form is closed.
            maintenanceForm.ShowDialog(); // Show maintenance form.

        }
        private void btnEdit_Click(object sender, EventArgs e)
        {//Edit user.
            if (dataGridView1.RowCount <= 0)
            {
                MessageBox.Show("No data to select", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (dataGridView1.SelectedCells.Count > 1)
            {
                var user = GetUser((int)dataGridView1.CurrentRow.Cells[0].Value);//Get user ID and search using GetUser(id) method.
                if (user == null) return;

                maintenanceForm = new FormUserMaintenance(user, TransactionAction.Edit); // Instantiate form, and send parameters (view model and action ).
                maintenanceForm.FormClosed += new FormClosedEventHandler(MaintenanceFormClosed); // Associate closed event, to update the datagrdiview after the maintenance form is closed.
                maintenanceForm.ShowDialog(); // Show maintenance form.          
            }
            else
                MessageBox.Show("Please select a row", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void btnRemove_Click(object sender, EventArgs e)
        {//Remove user.
            if (dataGridView1.RowCount <= 0)
            {
                MessageBox.Show("No data to select", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var user = GetUser((int)dataGridView1.CurrentRow.Cells[0].Value);//Get user ID and search using GetUser(id) method.
                if (user == null) return;

                maintenanceForm = new FormUserMaintenance(user, TransactionAction.Remove); // Instantiate form, and send parameters (view model and action ).
                maintenanceForm.FormClosed += new FormClosedEventHandler(MaintenanceFormClosed); // Associate closed event, to update the datagrdiview after the maintenance form is closed.
                maintenanceForm.ShowDialog(); // Show maintenance form.            
            }
            else
                MessageBox.Show("Please select a row", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        //Refresh datagridview
        private void MaintenanceFormClosed(object sender, FormClosedEventArgs e)
        {// Refresh the datagridview after the maintenance form closes.
            var lastData = maintenanceForm.LastRecord; // Get the last record of the maintenance form.
            if (lastData!= null) // If you have a last record.
            {
                LoadUserData (); // Update the datagridview.
                if (lastData!= "") // If the last record field is different from an empty string, then you should highlight and display the added or edited user.
                {
                    var index = userViewList.FindIndex (u => u.Username == lastData); // Find the index of the last user registered or modified.
                    dataGridView1.CurrentCell = dataGridView1.Rows [index] .Cells [0]; // Focus the cell of the last record.
                    dataGridView1.Rows [index] .Selected = true; // Select row.
                    // Note, if you added multiple users at the same time (Bulk insert) the first record in the user collection will be selected.
                }
            }
        }

        #endregion

    }
}
