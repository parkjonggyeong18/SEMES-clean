using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace semes
{
    /// <summary>
    /// DefectStatsPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DefectStatsPage : Page
    {
        // 차트 데이터
        public SeriesCollection DefectRateCollection { get; set; }
        public SeriesCollection DefectCountCollection { get; set; }

        // 불량 위치 표시를 위한 데이터
        private List<DefectPosition> defectPositions;

        public DefectStatsPage()
        {
            InitializeComponent();

            // 차트 데이터 초기화
            InitializeChartData();

            // 불량 위치 데이터 초기화
            InitializeDefectPositions();

            // UI 업데이트
            DataContext = this;
        }

        private void InitializeChartData()
        {
            try
            {
                // 실제 구현에서는 데이터베이스나 서비스에서 데이터를 가져와야 함
                // 여기서는 예시 데이터로 차트를 초기화

                // 불량률 차트 데이터 (선 그래프)
                DefectRateCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "불량률",
                        Values = new ChartValues<double> { 7.4, 33.0, 19.3, 40.3 },
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 10,
                        Stroke = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                        Fill = Brushes.Transparent
                    }
                };

                // 불량 수 차트 데이터 (막대 그래프)
                DefectCountCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "불량 수",
                        Values = new ChartValues<int> { 120, 130, 200, 220 },
                        Fill = new SolidColorBrush(Color.FromRgb(33, 150, 243))
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"차트 데이터 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeDefectPositions()
        {
            try
            {
                // 실제 구현에서는 데이터베이스나 서비스에서 데이터를 가져와야 함
                // 여기서는 예시 데이터
                defectPositions = new List<DefectPosition>
                {
                    new DefectPosition { Row = 2, Column = 0 },
                    new DefectPosition { Row = 3, Column = 3 },
                    new DefectPosition { Row = 3, Column = 4 },
                    new DefectPosition { Row = 3, Column = 5 },
                    new DefectPosition { Row = 4, Column = 2 }
                };

                // 불량 위치 UI 업데이트
                UpdateDefectPositionsUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"불량 위치 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDefectPositionsUI()
        {
            // 불량 위치 UI를 업데이트하는 로직
            // XAML에 정의된 Grid나 Canvas 등에 불량 위치 표시

            // 예를 들어, XAML에 Grid가 있고 해당 Grid의 행과 열에 불량 위치를 표시한다면:
            foreach (var position in defectPositions)
            {
                // 각 위치에 불량 표시용 원형 또는 다른 도형 추가
                Ellipse ellipse = new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Fill = Brushes.OrangeRed,
                    Margin = new Thickness(2)
                };

                // Grid에 추가
                // 예: defectPositionGrid.Children.Add(ellipse);
                // Grid.SetRow(ellipse, position.Row);
                // Grid.SetColumn(ellipse, position.Column);
            }
        }

        // 날짜 변경 이벤트 핸들러 (달력에서 날짜를 선택했을 때)
        private void DefectCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            var calendar = sender as Calendar;
            if (calendar != null && calendar.SelectedDate.HasValue)
            {
                DateTime selectedDate = calendar.SelectedDate.Value;

                // 선택된 날짜에 따라 데이터 업데이트
                LoadDataForDate(selectedDate);
            }
        }

        private void LoadDataForDate(DateTime date)
        {
            try
            {
                // 실제 구현에서는 선택된 날짜에 맞는 데이터를 가져와야 함
                // 여기서는 예시로 간단히 메시지 표시
                MessageBox.Show($"{date.ToString("yyyy-MM-dd")} 데이터를 불러옵니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);

                // 필요한 경우 차트 데이터와 불량 위치 데이터를 업데이트
                // UpdateChartsForDate(date);
                // UpdateDefectPositionsForDate(date);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"날짜별 데이터 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // 불량 위치 클래스
    public class DefectPosition
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
}