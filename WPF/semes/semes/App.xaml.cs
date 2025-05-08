using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using semes.Features.Auth.Views;

namespace semes
{
    // <summary>
    // Interaction logic for App.xaml
    // </summary>
    public partial class App : Application
    {
        public static Frame MainFrame { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 로그인 안되있으면 Login페이지로 로딩 하는 로직필요
            MainWindow mainWindow = new MainWindow();
            Current.MainWindow = mainWindow;
            mainWindow.Show();

            MainFrame = mainWindow.MainFrame;

            MainFrame.Navigate(new LoginPage());

            //mainWindow.btnDashboard. // TODO 클릭안되게 막아야함
            mainWindow.btnDashboard.IsEnabled = false;
            mainWindow.btnDefectDetection.IsEnabled = false;
            mainWindow.btnDefectStats.IsEnabled = false;
         }
    }
}
