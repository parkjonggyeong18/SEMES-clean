using System.Configuration;
using System.Data;
using System.Windows;
using semes.Features.Auth.Views;

namespace semes
{
    // <summary>
    // Interaction logic for App.xaml
    // </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 로그인 안되있으면 Login페이지로 로딩 하는 로직필요
            MainWindow mainWindow = new MainWindow();
            Current.MainWindow = mainWindow;
            MainWindow.Content = new LoginPage();
            mainWindow.Show();




            // LoginPage로 시작하기
            //if (mainWindow.Content is System.Windows.Controls.Frame mainFrame)
            //{
            //    mainFrame.Navigate(new LoginPage());
            //}
        }
    }
}
