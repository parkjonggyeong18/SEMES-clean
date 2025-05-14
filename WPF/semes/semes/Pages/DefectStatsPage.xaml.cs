// DefectStatsPage.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using semes.Services;

namespace semes
{
    public partial class DefectStatsPage : Page
    {
        public SeriesCollection DefectRateCollection { get; set; }
        public SeriesCollection DefectCountCollection { get; set; }
        public List<string> DateLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private readonly DefectStatsService _service = new();

        public DefectStatsPage()
        {
            InitializeComponent();
            LoadDataForDate(DateTime.Today);
            DataContext = this;
        }

        private async void LoadDataForDate(DateTime date)
        {
            try
            {
                var rateData = await _service.GetDefectRateTrendAsync(7);
                var countData = await _service.GetDefectCountTrendAsync(7);
                var positionData = await _service.GetDefectPositionsAsync(date);
                var (total, good, defect) = await _service.GetDefectSummaryAsync(date);

                // 날짜 라벨
                DateLabels = new List<string>();
                var defectRates = new ChartValues<double>();
                var defectCounts = new ChartValues<int>();

                foreach (var (dateStr, rate) in rateData)
                {
                    DateLabels.Add(dateStr);
                    defectRates.Add(rate);
                }

                foreach (var (_, count) in countData)
                {
                    defectCounts.Add(count);
                }

                DefectRateCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "불량률",
                        Values = defectRates,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 10,
                        Stroke = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                        Fill = Brushes.Transparent
                    }
                };

                DefectCountCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "불량 수",
                        Values = defectCounts,
                        Fill = new SolidColorBrush(Color.FromRgb(33, 150, 243))
                    }
                };

                YFormatter = val => val.ToString("F1") + "%";

                검사수Text.Text = $"{total} 건";
                양품수Text.Text = $"{good} 건";
                불량수Text.Text = $"{defect} 건";

                UpdateDefectPositionGrid(positionData);
                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로딩 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDefectPositionGrid(List<(int x, int y)> positions)
        {
            DefectPositionGrid.Children.Clear();

            var grid = new UniformGrid { Rows = 6, Columns = 18 };

            for (int i = 0; i < 6 * 18; i++)
                grid.Children.Add(new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0.5) });

            if (positions.Count == 0)
            {
                DefectPositionGrid.Children.Add(grid);
                return;
            }

            // 최대 좌표값 기준으로 스케일링
            int maxX = 0, maxY = 0;
            foreach (var (x, y) in positions)
            {
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            foreach (var (x, y) in positions)
            {
                int col = maxX == 0 ? 0 : Math.Clamp(x * 17 / maxX, 0, 17);
                int row = maxY == 0 ? 0 : Math.Clamp(y * 5 / maxY, 0, 5);
                int idx = row * 18 + col;

                if (idx >= 0 && idx < grid.Children.Count)
                {
                    var ellipse = new Ellipse
                    {
                        Width = 14,
                        Height = 14,
                        Fill = Brushes.Red,
                        Stroke = Brushes.DarkRed,
                        StrokeThickness = 1,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    ((Border)grid.Children[idx]).Child = ellipse;
                }
            }

            DefectPositionGrid.Children.Add(grid);
        }

        private void DefectCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefectCalendar.SelectedDate is DateTime date)
            {
                LoadDataForDate(date);
            }
        }
    }
}
