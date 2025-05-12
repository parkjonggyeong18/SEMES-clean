using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using semes.Services;
using NavigationService = semes.Services.NavigationService;

namespace semes.Features.Auth.Views
{
    /// <summary>
    /// LoginPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoginPage : Page
    {
        private AuthService _authService;

        public LoginPage()
        {
            InitializeComponent();

            _authService = new AuthService();
        }

        // 로그인 버튼 클릭시 실행되는 메서드
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // 입력값 검증
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("아이디와 비밀번호를 확인해주세요.");
                return;
            }

            // 입력값과 일치하는 회원 DB에서 검색
            bool success = await _authService.AuthenticateAsync(username, password);

            // 로그인 성공시
            if (success)
            {
                string currentUser = _authService.CurrentUser;

                Window currentWindow = Window.GetWindow(this);

                if (currentWindow is MainWindow mainWindow)
                {
                    mainWindow.CurrentUser = currentUser;

                    mainWindow.btnDashboard.IsEnabled = true;
                    mainWindow.btnDefectDetection.IsEnabled = true;
                    mainWindow.btnDefectStats.IsEnabled = true;
                    mainWindow.btnCommunity.IsEnabled = true;

                    if (_authService.UserRole == "ADMIN")
                    {
                        mainWindow.btnUserManagement.Visibility = Visibility.Visible;
                    }
                }   

                this.NavigationService.Navigate(new DashboardPage());

                MessageBox.Show($"환영합니다, {currentUser}님!");
            }
            else
            {
                MessageBox.Show("아이디 또는 비밀번호가 올바르지 않습니다.");
            }
        }
    }
}
