using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace semes
{
    /// <summary>
    /// DashboardPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();

            // 대시보드 데이터 로드
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // 실제 구현에서는 데이터베이스나 서비스에서 데이터를 가져와야 함
                // 여기서는 예시 데이터로 차트를 업데이트함

                // 불량률 차트 업데이트 (62%)
                UpdateDefectRateChart(62);

                // 불량 검출 차트 업데이트 (34%)
                UpdateDefectDetectionChart(34);

                // 다른 통계 정보도 필요하다면 업데이트
            }
            catch (Exception ex)
            {
                MessageBox.Show($"대시보드 데이터 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDefectRateChart(double percentage)
        {
            // 원형 차트 업데이트를 위한 Path 생성
            // XAML에 정의된 Path를 코드에서 조작하는 방식
            // 실제 구현에서는 Path 요소에 대한 참조를 가져와 업데이트해야 함

            // 예시: Path 요소가 이름이 있다면 해당 요소 직접 업데이트
            // defectRatePath.Data = CreateArcPathData(percentage);
        }

        private void UpdateDefectDetectionChart(double percentage)
        {
            // 불량 검출 차트 업데이트 로직
            // 위와 유사하게 구현
        }

        // 원형 차트의 Arc를 그리기 위한 Path Geometry 생성 함수
        private PathGeometry CreateArcPathData(double percentage)
        {
            double angleInDegrees = 360 * (percentage / 100);
            double angleInRadians = angleInDegrees * (Math.PI / 180);

            double radius = 80;
            Point center = new Point(100, 100);

            // 시작점 (항상 12시 방향에서 시작)
            Point startPoint = new Point(center.X, center.Y - radius);

            // 끝점 계산
            double endX = center.X + radius * Math.Sin(angleInRadians);
            double endY = center.Y - radius * Math.Cos(angleInRadians);
            Point endPoint = new Point(endX, endY);

            // Arc 그리기
            PathFigure figure = new PathFigure();
            figure.StartPoint = startPoint;

            ArcSegment arc = new ArcSegment();
            arc.Point = endPoint;
            arc.Size = new Size(radius, radius);
            arc.SweepDirection = SweepDirection.Clockwise;
            arc.IsLargeArc = angleInDegrees > 180;

            figure.Segments.Add(arc);

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return geometry;
        }
    }
}