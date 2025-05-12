using semes.Models;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;

namespace semes.Pages
{
    public partial class CommunityEditPage : Page
    {
        private readonly Post _post;
        private string _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";

        public CommunityEditPage(Post post)
        {
            InitializeComponent();
            _post = post;
            txtTitle.Text = post.Title;
            txtContent.Text = post.Content;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string newTitle = txtTitle.Text.Trim();
            string newContent = txtContent.Text.Trim();

            if (string.IsNullOrEmpty(newTitle) || string.IsNullOrEmpty(newContent))
            {
                MessageBox.Show("제목과 내용을 모두 입력해주세요.");
                return;
            }

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("UPDATE posts SET title = @title, content = @content WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@title", newTitle);
            cmd.Parameters.AddWithValue("@content", newContent);
            cmd.Parameters.AddWithValue("@id", _post.Id);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show("게시글이 수정되었습니다.");

            var mainWindow = Application.Current.MainWindow as MainWindow;
            string currentUser = mainWindow?.CurrentUser ?? "";
            NavigationService.Navigate(new CommunityPage(currentUser));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
