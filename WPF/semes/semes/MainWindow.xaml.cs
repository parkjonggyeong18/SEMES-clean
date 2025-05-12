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

        private void SetActiveButton(Button activeButton)
        {
            btnDashboard.Background = System.Windows.Media.Brushes.Transparent;
            btnDefectDetection.Background = System.Windows.Media.Brushes.Transparent;
            btnDefectStats.Background = System.Windows.Media.Brushes.Transparent;
            btnUserManagement.Background = System.Windows.Media.Brushes.Transparent;
            btnCommunity.Background = System.Windows.Media.Brushes.Transparent;

            activeButton.Background = (System.Windows.Media.Brush)FindResource("MaterialDesignPaper");
        }
    }
}
