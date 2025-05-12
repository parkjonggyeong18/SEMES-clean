using System.Windows;
using System.Windows.Controls;
using semes.Models;

namespace semes.Pages
{
    public partial class CommunityDetailPage : Page
    {
        public string CurrentUser { get; set; }
        public Post Post { get; set; }

        public CommunityDetailPage(Post post)
        {
            InitializeComponent();

            // 로그인한 사용자 정보 가져오기
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                CurrentUser = mainWindow.CurrentUser;
            }

            // Post는 따로 바인딩용 속성에 넣기
            Post = post;

            // DataContext 설정
            this.DataContext = this;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CommunityEditPage(Post));
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using var conn = new MySqlConnector.MySqlConnection("Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;");
                await conn.OpenAsync();

                var cmd = new MySqlConnector.MySqlCommand("DELETE FROM posts WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", Post.Id);
                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("삭제되었습니다.");

                NavigationService.GoBack(); // 목록으로 돌아가기
            }
        }

    }
}
