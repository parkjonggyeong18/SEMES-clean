using semes.Models;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            LoadPostData();
        }

        private void LoadPostData()
        {
            txtTitle.Text = _post.Title;
            txtContent.Text = _post.Content;

            // 포커스를 제목 필드로 설정
            txtTitle.Focus();
            txtTitle.SelectAll();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string newTitle = txtTitle.Text.Trim();
            string newContent = txtContent.Text.Trim();

            // 유효성 검사
            if (string.IsNullOrEmpty(newTitle))
            {
                ShowErrorMessage("제목을 입력해주세요.");
                txtTitle.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newContent))
            {
                ShowErrorMessage("내용을 입력해주세요.");
                txtContent.Focus();
                return;
            }

            // 변경사항 확인
            if (newTitle == _post.Title && newContent == _post.Content)
            {
                ShowInfoMessage("변경된 내용이 없습니다.");
                return;
            }

            try
            {
                // 저장 전 확인 메시지
                var result = MessageBox.Show("게시글을 수정하시겠습니까?",
                                           "수정 확인",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using var conn = new MySqlConnection(_connectionString);
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand("UPDATE posts SET title = @title, content = @content WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@title", newTitle);
                    cmd.Parameters.AddWithValue("@content", newContent);
                    cmd.Parameters.AddWithValue("@id", _post.Id);

                    await cmd.ExecuteNonQueryAsync();

                    ShowSuccessMessage("게시글이 성공적으로 수정되었습니다.");

                    // 메인 페이지로 돌아가기
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    string currentUser = mainWindow?.CurrentUser ?? "";
                    NavigationService.Navigate(new CommunityPage(currentUser));
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"게시글 수정 중 오류가 발생했습니다: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // 변경사항이 있는지 확인
            string currentTitle = txtTitle.Text.Trim();
            string currentContent = txtContent.Text.Trim();

            if (currentTitle != _post.Title || currentContent != _post.Content)
            {
                var result = MessageBox.Show("변경사항이 저장되지 않습니다. 정말 취소하시겠습니까?",
                                           "취소 확인",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }

            NavigationService.GoBack();
        }

        // 메시지 표시 메서드들
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "성공", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowInfoMessage(string message)
        {
            MessageBox.Show(message, "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}