using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using semes.Features.Auth.Views;

namespace semes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Frame MainFrame { get; set; }
        public static MainWindow MainWindowInstance { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 로그인 안되있으면 Login페이지로 로딩 하는 로직필요

            // 메인 윈도우 생성만 하고 표시하지 않음 (숨김)
            MainWindowInstance = new MainWindow();
            MainFrame = MainWindowInstance.MainFrame;

            // 버튼들 비활성화
            MainWindowInstance.btnDashboard.IsEnabled = false;
            MainWindowInstance.btnDefectDetection.IsEnabled = false;
            MainWindowInstance.btnDefectStats.IsEnabled = false;

            // 로그인 창을 별도 Window로 생성
            var loginWindow = new Window
            {
                Content = new LoginPage(),
                Title = "로그인",
                Width = 450,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.SingleBorderWindow
            };

            // 로그인 창만 표시
            //Current.MainWindow = loginWindow;
            loginWindow.Show();
        }
    }
}