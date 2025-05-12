using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;
using System.Threading.Tasks;
using System;

namespace semes.Pages
{
    public partial class UserManagementPage : Page
    {
        private ObservableCollection<User> Users = new ObservableCollection<User>();
        private string _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";

        public UserManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            Users.Clear();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand("SELECT username, password FROM users", conn);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Users.Add(new User
                            {
                                Username = reader.GetString("username"),
                                Password = reader.GetString("password")
                            });
                        }
                    }
                }

                userListView.ItemsSource = Users;
            }
            catch (Exception ex)
            {
                MessageBox.Show("회원 목록 불러오기 실패: " + ex.Message);
            }
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUserId.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("아이디와 비밀번호를 입력하세요.");
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand("INSERT INTO users (username, password) VALUES (@username, @password)", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    int result = await cmd.ExecuteNonQueryAsync();
                    if (result == 1)
                    {
                        MessageBox.Show("회원이 추가되었습니다.");
                        LoadUsers();
                        txtUserId.Clear();
                        txtPassword.Clear();
                    }
                    else
                    {
                        MessageBox.Show("회원 추가 실패");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("회원 추가 중 오류 발생: " + ex.Message);
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is User user)
            {
                try
                {
                    using (var conn = new MySqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var cmd = new MySqlCommand("DELETE FROM users WHERE username = @username", conn);
                        cmd.Parameters.AddWithValue("@username", user.Username);

                        int result = await cmd.ExecuteNonQueryAsync();
                        if (result == 1)
                        {
                            MessageBox.Show("회원 삭제 성공");
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show("회원 삭제 실패");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("삭제 중 오류 발생: " + ex.Message);
                }
            }
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
