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
                ShowMessage("아이디와 비밀번호를 확인해주세요.");
                return;
            }

            // 입력값과 일치하는 회원 DB에서 검색
            bool success = await _authService.AuthenticateAsync(username, password);

            // 로그인 성공시
            if (success)
            {
                string currentUser = _authService.CurrentUser;

                // 에러 메시지 숨기기 (성공시)
                lblErrorMessage.Visibility = Visibility.Collapsed;

                // 현재 로그인 창 닫기
                Window loginWindow = Window.GetWindow(this);
                loginWindow.Close();

                var mainWindow = App.MainWindowInstance;
                mainWindow.CurrentUser = currentUser;
                // Welcome 창 표시
                WelcomeWindow welcomeWindow = new WelcomeWindow(currentUser);

                // Welcome 창이 닫힐 때 메인 윈도우 표시
                welcomeWindow.Closed += (s, args) =>
                {
                    // App.MainWindowInstance를 직접 사용하고 설정
                    App.MainWindowInstance.CurrentUser = currentUser;
                    App.MainWindowInstance.btnDashboard.IsEnabled = true;
                    App.MainWindowInstance.btnDefectDetection.IsEnabled = true;
                    App.MainWindowInstance.btnDefectStats.IsEnabled = true;
                    App.MainWindowInstance.btnCommunity.IsEnabled = true;

                    // 관리자 권한 확인
                    if (_authService.UserRole == "ADMIN")
                    {
                        App.MainWindowInstance.btnUserManagement.Visibility = Visibility.Visible;
                    }

                    App.MainWindowInstance.SetActiveButton(App.MainWindowInstance.btnDashboard);

                    // 메인 윈도우 표시
                    Application.Current.MainWindow = App.MainWindowInstance;
                    App.MainWindowInstance.Show();

                    // 대시보드로 이동
                    App.MainFrame.Navigate(new DashboardPage());
                };

                welcomeWindow.ShowDialog();
            }
            else
            {
                // 로그인 실패시 에러 메시지 표시
                ShowMessage("아이디 또는 비밀번호가 올바르지 않습니다.");
            }
        }

        // WinForm과 같은 방식의 에러 메시지 표시 메서드
        private void ShowMessage(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visibility = Visibility.Visible;
        }
    }
}