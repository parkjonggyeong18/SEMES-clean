using MySqlConnector;
using semes.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace semes.Pages
{
    public partial class CommunityPage : Page
    {
        public string CurrentUser { get; }
        private ObservableCollection<Post> Posts = new ObservableCollection<Post>();
        private ObservableCollection<Post> AllPosts = new ObservableCollection<Post>();
        private string _connectionString = "Server=stg-yswa-kr-practice-db-master.mariadb.database.azure.com;Port=3306;Database=s12p31s105;User=S12P31S105@stg-yswa-kr-practice-db-master;Password=QQAurD9pAg;";

        public CommunityPage(string currentUser)
        {
            InitializeComponent();
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                this.DataContext = new { Posts = Posts, CurrentUser = mainWindow.CurrentUser };
            }

            // 검색 기능 추가
            SearchBox.GotFocus += SearchBox_GotFocus;
            SearchBox.LostFocus += SearchBox_LostFocus;
            SearchBox.TextChanged += SearchBox_TextChanged;

            Loaded += async (s, e) => await LoadPostsAsync();
        }

        private async Task LoadPostsAsync()
        {
            try
            {
                Posts.Clear();
                AllPosts.Clear();

                using (var conn = new MySqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    var cmd = new MySqlCommand("SELECT id, title, content, created_at, author FROM posts ORDER BY created_at DESC", conn);
                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var post = new Post
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.GetString("title"),
                            Content = reader.GetString("content"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            Author = reader.GetString("author")
                        };
                        Posts.Add(post);
                        AllPosts.Add(post);
                    }
                }

                postListView.ItemsSource = Posts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WritePost_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CommunityWritePage());
        }

        // 게시글 카드 클릭 이벤트
        private void PostCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Post selectedPost)
            {
                NavigationService.Navigate(new CommunityDetailPage(selectedPost));
            }
        }

        // 검색 기능
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "게시글 검색...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                SearchBox.Text = "게시글 검색...";
                SearchBox.Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99));
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrEmpty(searchText) || searchText == "게시글 검색...")
            {
                Posts.Clear();
                foreach (var post in AllPosts)
                {
                    Posts.Add(post);
                }
            }
            else
            {
                Posts.Clear();
                foreach (var post in AllPosts)
                {
                    if (post.Title.ToLower().Contains(searchText) ||
                        post.Content.ToLower().Contains(searchText) ||
                        post.Author.ToLower().Contains(searchText))
                    {
                        Posts.Add(post);
                    }
                }
            }
        }

        private void SearchBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}