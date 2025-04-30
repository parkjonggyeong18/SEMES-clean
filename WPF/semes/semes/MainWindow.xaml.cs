using System.Windows;
using System.Windows.Controls;

namespace semes
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 초기 페이지로 대시보드 로드
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

        // 활성화된 버튼 스타일 적용
        private void SetActiveButton(Button activeButton)
        {
            // 모든 버튼 초기화
            btnDashboard.Background = System.Windows.Media.Brushes.Transparent;
            btnDefectDetection.Background = System.Windows.Media.Brushes.Transparent;
            btnDefectStats.Background = System.Windows.Media.Brushes.Transparent;

            // 활성화된 버튼 스타일 적용
            activeButton.Background = (System.Windows.Media.Brush)FindResource("MaterialDesignPaper");
        }
    }
}