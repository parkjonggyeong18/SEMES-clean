using MySqlConnector;
using semes.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace semes.Pages
{
    public partial class CommunityPage : Page
    {
        public string CurrentUser { get; }

        private ObservableCollection<Post> Posts = new ObservableCollection<Post>();
        private string _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";

        public CommunityPage(string currentUser)
        {
            InitializeComponent();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                this.DataContext = new { Posts = Posts, CurrentUser = mainWindow.CurrentUser };
            }

            Loaded += async (s, e) => await LoadPostsAsync();
        }


        private async Task LoadPostsAsync()
        {
            Posts.Clear();

            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT id, title, content, created_at, author FROM posts ORDER BY created_at DESC", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Posts.Add(new Post
                    {
                        Id = reader.GetInt32("id"),
                        Title = reader.GetString("title"),
                        Content = reader.GetString("content"),
                        CreatedAt = reader.GetDateTime("created_at"),
                        Author = reader.GetString("author")
                    });
                }
            }

            postListView.ItemsSource = Posts;
        }

        private void WritePost_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CommunityWritePage());
        }

        private void postListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (postListView.SelectedItem is Post selectedPost)
            {
                NavigationService.Navigate(new CommunityDetailPage(selectedPost));
                postListView.SelectedItem = null;
            }
        }

    }
}
