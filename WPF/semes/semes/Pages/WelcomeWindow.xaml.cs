using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace semes.Features.Auth.Views
{
    public partial class WelcomeWindow : Window
    {
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private double progress = 0;
        private string username;

        public WelcomeWindow(string username)
        {
            InitializeComponent();
            this.username = username;

            // 사용자명 설정
            lblUsername.Text = $"환영합니다, {username}님!";

            // 타이머 초기화
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(50);
            timer1.Tick += Timer1_Tick;

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(50);
            timer2.Tick += Timer2_Tick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = 0.0;
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            // 페이드 인
            if (Opacity < 1)
                Opacity += 0.05;

            // 프로그레스 업데이트
            progress += 2;

            // 100을 넘지 않도록 제한
            if (progress > 100) progress = 100;

            // 퍼센트와 프로그레스바를 동시에 업데이트
            lblProgress.Text = $"{(int)progress}%";
            UpdateCircularProgressDirectly(progress);

            if (progress >= 100)
            {
                timer1.Stop();
                // 1초 대기 후 페이드 아웃 시작
                Dispatcher.BeginInvoke(new Action(async () => {
                    await System.Threading.Tasks.Task.Delay(1000);
                    timer2.Start();
                }), DispatcherPriority.Background);
            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            // 페이드 아웃
            Opacity -= 0.1;
            if (Opacity <= 0)
            {
                timer2.Stop();
                Close();
            }
        }

        private void UpdateCircularProgressDirectly(double percentage)
        {
            // 원 둘레 계산 (2 * π * r, 여기서 r = 75)
            double circumference = 2 * Math.PI * 75; // 약 471
            double dashOffset = circumference - (circumference * percentage / 100);

            // 애니메이션 없이 직접 값 설정 - 완벽한 동기화
            progressCircle.StrokeDashOffset = dashOffset;
        }
    }
}