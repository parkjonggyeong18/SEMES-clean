using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using semes.Pages;

namespace semes
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _currentUser;
        public string CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(new DashboardPage());
            SetActiveButton(btnDashboard);
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
            SetActiveButton(btnDashboard);
        }

        private void btnDefectDetection_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DefectDetectionPage());
            SetActiveButton(btnDefectDetection);
        }

        private void btnDefectStats_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DefectStatsPage());
            SetActiveButton(btnDefectStats);
        }

        private void btnCommunity_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CommunityPage(CurrentUser));
            SetActiveButton(btnCommunity);
        }

        // 🆕 AI 챗봇 버튼 클릭 이벤트 추가
        private void btnAIChatBot_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ChatBotPage());
            SetActiveButton(btnAIChatBot);
        }

        private void btnIndustryNews_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IndustryNewsPage());
            SetActiveButton(btnIndustryNews);
        }

        private void btnUserManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UserManagementPage());
            SetActiveButton(btnUserManagement);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("로그아웃 하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // 로그인 정보 초기화
                CurrentUser = null;

                // 버튼 비활성화
                btnDashboard.IsEnabled = false;
                btnDefectDetection.IsEnabled = false;
                btnDefectStats.IsEnabled = false;
                btnCommunity.IsEnabled = false;
                //btnAIChatBot.IsEnabled = false; // 🆕 AI 챗봇 버튼도 비활성화
                //btnIndustryNews.IsEnabled = false;
                btnUserManagement.Visibility = Visibility.Collapsed;

                this.Hide();

                var loginWindow = new Window
                {
                    Content = new Features.Auth.Views.LoginPage(),
                    Title = "로그인",
                    Width = 450,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.SingleBorderWindow
                };

                loginWindow.ShowDialog();
            }
        }

        public void SetActiveButton(Button activeButton)
        {
            btnDashboard.Background = System.Windows.Media.Brushes.Transparent;
            btnDefectDetection.Background = System.Windows.Media.Brushes.Transparent;
            btnDefectStats.Background = System.Windows.Media.Brushes.Transparent;
            btnUserManagement.Background = System.Windows.Media.Brushes.Transparent;
            btnCommunity.Background = System.Windows.Media.Brushes.Transparent;
            btnAIChatBot.Background = System.Windows.Media.Brushes.Transparent; // 🆕 AI 챗봇 버튼 추가
            btnIndustryNews.Background = System.Windows.Media.Brushes.Transparent;

            activeButton.Background = (System.Windows.Media.Brush)FindResource("MaterialDesignPaper");
        }
    }
}