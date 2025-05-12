using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace semes
{
    //<summary>
    //DefectDetectionPage.xaml에 대한 상호 작용 논리
    //</summary>
    public partial class DefectDetectionPage : Page
    {
        // 불량 목록
        private ObservableCollection<DefectItem> defectItems;

        // 불량 마커를 관리하기 위한 딕셔너리 (ID로 찾기 쉽게)
        private Dictionary<int, UIElement> defectMarkers = new Dictionary<int, UIElement>();

        // PCB 이미지 크기
        private const double ORIGINAL_IMAGE_WIDTH = 12000;
        private const double ORIGINAL_IMAGE_HEIGHT = 4000;

        public DefectDetectionPage()
        {
            InitializeComponent();

            // defectItems 내용이 변할때마다 호출할 함수 등록
            defectItems = new ObservableCollection<DefectItem>();
            defectItems.CollectionChanged += DefectItems_CollectionChanged;

            // PCBImage가 로드될 때 실행될 함수
            PCBImage.Loaded += PCBImage_Loaded;
        }
        private void CreateSampleDefects()
        {
            // 예시 데이터 - 원본 이미지 좌표계 기준 (픽셀 단위)
            defectItems.Add(new DefectItem { Id = 1, X = 500, Y = 400, Width = 15, Height = 2.5 });
            defectItems.Add(new DefectItem { Id = 2, X = 1200, Y = 800, Width = 20, Height = 3.2 });
            defectItems.Add(new DefectItem { Id = 3, X = 2000, Y = 1500, Width = 10, Height = 1.8 });
            defectItems.Add(new DefectItem { Id = 4, X = 3000, Y = 2000, Width = 18, Height = 2.7 });
            defectItems.Add(new DefectItem { Id = 5, X = 0, Y = 0, Width = 18, Height = 2.7 });
        }

        #region PCB 이미지 로딩 콜백
        private void PCBImage_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustCanvasToImage();
        }

        private void AdjustCanvasToImage()
        {
            // Canvas 크기를 이미지 크기에 맞게 설정
            if (PCBImage != null && DefectCanvas != null)
            {
                double imageWidth = PCBImage.ActualWidth;
                double imageHeight = PCBImage.ActualHeight;

                // Canvas 크기를 이미지 크기와 정확히 일치시킴
                DefectCanvas.Width = imageWidth;
                DefectCanvas.Height = imageHeight;

                // Canvas 위치도 이미지 위치와 정확히 일치시킴
                Canvas.SetLeft(DefectCanvas, Canvas.GetLeft(PCBImage));
                Canvas.SetTop(DefectCanvas, Canvas.GetTop(PCBImage));

                Console.WriteLine($"이미지 크기: {imageWidth} x {imageHeight}");
                Console.WriteLine($"Canvas 크기: {DefectCanvas.Width} x {DefectCanvas.Height}");
            }
        }
        #endregion

        #region Canvas 관련 함수
        // defectItems 변경 감지 이벤트 핸들러
        private void DefectItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DefectCanvas == null || PCBImage == null || PCBImage.ActualWidth == 0)
                return;

            // 변경 유형에 따라 처리
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // 새 항목 추가
                    foreach (DefectItem newItem in e.NewItems)
                    {
                        AddDefectMarker(newItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    // 항목 제거
                    foreach (DefectItem oldItem in e.OldItems)
                    {
                        RemoveDefectMarker(oldItem.Id);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    // 항목 교체
                    foreach (DefectItem oldItem in e.OldItems)
                    {
                        RemoveDefectMarker(oldItem.Id);
                    }
                    foreach (DefectItem newItem in e.NewItems)
                    {
                        AddDefectMarker(newItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // 모든 항목 제거 (Clear 호출 시)
                    DefectCanvas.Children.Clear();
                    defectMarkers.Clear();
                    break;
            }

            UpdateDefectResultsList();
        }

        // 개별 불량 마커 추가
        private void AddDefectMarker(DefectItem defect)
        {
            double scaleX = PCBImage.ActualWidth / ORIGINAL_IMAGE_WIDTH;
            double scaleY = PCBImage.ActualHeight / ORIGINAL_IMAGE_HEIGHT;

            double displayX = defect.X * scaleX;
            double displayY = defect.Y * scaleY;
            double displaySize = Math.Max(defect.Width * scaleX, 10);

            // 불량 마커 생성
            Ellipse marker = new Ellipse
            {
                Width = displaySize,
                Height = displaySize,
                Fill = new SolidColorBrush(Colors.Red) { Opacity = 0.5 },
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = 2,
                Tag = defect
            };

            // 마커 위치 설정 (중앙 정렬)
            Canvas.SetLeft(marker, displayX - displaySize / 2);
            Canvas.SetTop(marker, displayY - displaySize / 2);

            // Canvas에 추가
            DefectCanvas.Children.Add(marker);

            // 딕셔너리에도 저장 (나중에 제거하기 쉽게)
            defectMarkers[defect.Id] = marker;

            Console.WriteLine($"불량 마커 추가: ID={defect.Id}, 위치=({displayX}, {displayY})");
        }

        // 불량 마커 제거
        private void RemoveDefectMarker(int defectId)
        {
            if (defectMarkers.TryGetValue(defectId, out UIElement marker))
            {
                // Canvas에서 제거
                DefectCanvas.Children.Remove(marker);

                // 딕셔너리에서도 제거
                defectMarkers.Remove(defectId);

                Console.WriteLine($"불량 마커 제거: ID={defectId}");
            }
        }
        #endregion

        #region 클릭 이벤트 리스너
        private void DefectBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleDefects();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            DefectCanvas.Children.Clear();
        }
        #endregion

        #region 검출 결과 목록 갱신 관련 함수
        // 검출 결과 목록 업데이트
        private void UpdateDefectResultsList()
        {
            // UI 스레드에서 실행되도록 Dispatcher 사용
            Dispatcher.Invoke(() =>
            {
                // 결과 목록 초기화
                DefectResultsPanel.Children.Clear();

                // 불량이 없으면 메시지 표시
                if (defectItems.Count == 0)
                {
                    TextBlock noDefectsMessage = new TextBlock
                    {
                        Text = "검출된 불량이 없습니다.",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 60, 0, 0),
                        Foreground = (Brush)FindResource("MaterialDesignBodyLight")
                    };
                    DefectResultsPanel.Children.Add(noDefectsMessage);
                    return;
                }

                // 불량 데이터 표시를 위한 헤더 추가 (선택사항)
                Grid headerGrid = CreateDefectResultHeader();
                DefectResultsPanel.Children.Add(headerGrid);

                // 각 불량에 대한 결과 항목 추가
                foreach (var defect in defectItems)
                {
                    // 불량 정보를 표시할 Grid 생성
                    Grid defectGrid = CreateDefectResultItem(defect);
                    DefectResultsPanel.Children.Add(defectGrid);
                }
            });
        }

        // 결과 목록 헤더 생성
        private Grid CreateDefectResultHeader()
        {
            Grid grid = new Grid();
            grid.Margin = new Thickness(5);

            // 열 정의
            for (int i = 0; i < 4; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 헤더 라벨
            string[] headers = new string[] { "ID", "X 위치", "Y 위치", "높이" };

            for (int i = 0; i < headers.Length; i++)
            {
                TextBlock headerText = new TextBlock
                {
                    Text = headers[i],
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(5),
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetColumn(headerText, i);
                grid.Children.Add(headerText);
            }

            // 구분선
            Border separator = new Border
            {
                Height = 1,
                Background = Brushes.LightGray,
                Margin = new Thickness(0, 25, 0, 0)
            };
            Grid.SetColumnSpan(separator, 4);
            grid.Children.Add(separator);

            return grid;
        }

        // 개별 불량 결과 항목 생성
        private Grid CreateDefectResultItem(DefectItem defect)
        {
            Grid grid = new Grid();
            grid.Margin = new Thickness(5, 2, 5, 2);

            // 열 정의
            for (int i = 0; i < 4; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // ID
            TextBlock idText = new TextBlock
            {
                Text = defect.Id.ToString(),
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(idText, 0);
            grid.Children.Add(idText);

            // X 위치
            TextBlock xText = new TextBlock
            {
                Text = defect.X.ToString(),
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(xText, 1);
            grid.Children.Add(xText);

            // Y 위치
            TextBlock yText = new TextBlock
            {
                Text = defect.Y.ToString(),
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(yText, 2);
            grid.Children.Add(yText);

            // 높이
            TextBlock heightText = new TextBlock
            {
                Text = defect.Height.ToString("F1"),
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(heightText, 3);
            grid.Children.Add(heightText);

            // 항목 선택 가능하게 만들기 (선택사항)
            grid.MouseEnter += (s, e) => grid.Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 255));
            grid.MouseLeave += (s, e) => grid.Background = null;
            grid.MouseDown += (s, e) => HighlightDefect(defect.Id);

            return grid;
        }

        // 불량 항목 선택 시 해당 불량 강조 표시 (선택사항)
        private void HighlightDefect(int defectId)
        { // 선택된 불량 찾기
            DefectItem selectedDefect = null;
            foreach (var defect in defectItems)
            {
                if (defect.Id == defectId)
                {
                    selectedDefect = defect;
                    break;
                }
            }

            if (selectedDefect == null)
                return;

            // 상세 정보 업데이트
            UpdateDefectDetails(selectedDefect);

            // 마커 강조 표시
            if (defectMarkers.TryGetValue(defectId, out UIElement marker))
            {
                // 모든 마커를 원래 스타일로
                foreach (var item in defectMarkers.Values)
                {
                    if (item is Ellipse ellipse)
                    {
                        ellipse.Stroke = new SolidColorBrush(Colors.Yellow);
                        ellipse.StrokeThickness = 2;
                    }
                }

                // 선택된 마커 강조
                if (marker is Ellipse selectedEllipse)
                {
                    selectedEllipse.Stroke = new SolidColorBrush(Colors.Lime);
                    selectedEllipse.StrokeThickness = 3;
                }
            }
        }
        #endregion

        #region 불량 상세 정보 갱신 관련 함수
        // 불량 상세 정보 업데이트
        private void UpdateDefectDetails(DefectItem defect)
        {
            // UI 스레드에서 실행되도록 Dispatcher 사용
            Dispatcher.Invoke(() =>
            {
                // 불량 ID
                DefectIdValue.Text = defect.Id.ToString();

                // 위치 (X,Y)
                DefectPositionValue.Text = $"({defect.X}, {defect.Y})";

                // 높이
                DefectHeightValue.Text = $"{defect.Height} um";

                // 크기 (Width 속성 사용)
                DefectSizeValue.Text = $"{defect.Width} um";
            });
        }
        #endregion

        // 불량 항목 클래스
        public class DefectItem
        {
            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public double Height { get; set; }
            public double Width { get; set; }
        }
    }
}