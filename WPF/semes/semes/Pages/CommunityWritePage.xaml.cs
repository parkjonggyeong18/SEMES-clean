using MySqlConnector;
using System.Windows;
using System.Windows.Controls;

namespace semes.Pages
{
    public partial class CommunityWritePage : Page
    {
        private string _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";

        public CommunityWritePage()
        {
            InitializeComponent();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string content = txtContent.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("제목과 내용을 모두 입력해주세요.");
                return;
            }

            // 현재 로그인한 사용자
            string currentUser = ((MainWindow)Application.Current.MainWindow).CurrentUser;

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            
            var cmd = new MySqlCommand("INSERT INTO posts (title, content, author) VALUES (@title, @content, @author)", conn);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@author", currentUser);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show("게시글이 등록되었습니다.");
            NavigationService.GoBack(); // 목록으로 돌아감
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
