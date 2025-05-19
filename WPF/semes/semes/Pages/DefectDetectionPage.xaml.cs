using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using semes.Services;

namespace semes
{
    public partial class DefectDetectionPage : Page
    {
        // 불량 목록
        private ObservableCollection<DefectItem> defectItems;

        // 현재 PCB 시리얼 넘버
        private string currentSerialNumber;

        // 서비스 인스턴스
        private readonly PcbInspectionService _pcbInspectionService;
        private readonly OpenAIService _openAIService;

        // 불량 마커를 관리하기 위한 딕셔너리 (ID로 찾기 쉽게)
        private Dictionary<int, UIElement> defectMarkers = new Dictionary<int, UIElement>();

        // PCB 이미지 크기
        private const double ORIGINAL_IMAGE_WIDTH = 12000;
        private const double ORIGINAL_IMAGE_HEIGHT = 4000;

        // AI 분석 완료 상태
        private bool isAiAnalysisComplete = false;

        public DefectDetectionPage()
        {
            InitializeComponent();

            // 서비스 인스턴스 생성
            _pcbInspectionService = new PcbInspectionService();
            _openAIService = new OpenAIService();

            // defectItems 내용이 변할때마다 호출할 함수 등록
            defectItems = new ObservableCollection<DefectItem>();
            defectItems.CollectionChanged += DefectItems_CollectionChanged;

            // PCBImage가 로드될 때 실행될 함수
            PCBImage.Loaded += PCBImage_Loaded;

            PCBContainer.MouseWheel += PCBContainer_MouseWheel;
        }

        #region PCB 이미지 로딩 콜백
        private void PCBImage_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustCanvasToImage();
            FitImageToView();
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

        private void FitImageToView()
        {
            if (PCBImage == null || PCBImage.ActualWidth <= 0 || PCBScrollViewer == null)
                return;

            // 스크롤뷰어 크기와 이미지 크기 기반으로 적절한 스케일 계산
            double scaleX = PCBScrollViewer.ActualWidth / PCBImage.ActualWidth;
            double scaleY = PCBScrollViewer.ActualHeight / PCBImage.ActualHeight;

            // 더 작은 비율을 선택하여 이미지가 완전히 보이게 함
            double scale = Math.Min(scaleX, scaleY) * 0.95; // 약간의 여백을 위해 0.95 곱함

            // 최소 스케일 값과 비교해서 더 작은 값을 사용
            scale = Math.Min(scale, 0.055);

            // 스케일 적용
            PCBScaleTransform.ScaleX = scale;
            PCBScaleTransform.ScaleY = scale;

            Console.WriteLine($"초기 이미지 스케일: {scale}");
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

            // 크기에 따라 표시 크기 조정
            double sizeMultiplier = 5;
            double displaySize = Math.Max(defect.Width * sizeMultiplier, 10);

            // 불량 마커 생성
            Ellipse marker = new Ellipse
            {
                Width = displaySize,
                Height = displaySize,
                Fill = new SolidColorBrush(Colors.Red) { Opacity = sizeMultiplier },
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = 10,
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

        #region 불량검출 클릭 이벤트 리스너 **
        private async void DefectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 진행 상태 패널 표시
                ProgressOverlay.Visibility = Visibility.Visible;
                StatusMessage.Text = "PCB 스캔 중...";
                ProgressBar.IsIndeterminate = true;

                // 버튼 비활성화
                DefectBtn.IsEnabled = false;
                ClearBtn.IsEnabled = false;

                // 비동기 작업으로 변경
                await Task.Run(() =>
                {
                    Thread.Sleep(1000);

                    // UI스레드에서 메시지 업데이트
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage.Text = "ZMap 생성 중...";
                    });

                    // 1. ZMap 생성
                    string zmapType = "slope_xy";
                    int numDefects = new Random().Next(0, 21);

                    var generator = new ZMapGenerator();
                    generator.Generate(zmapType, numDefects, out string csvPath, out string tifPath);

                    // 메시지 업데이트
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage.Text = "이물질 검출 중...";
                    });

                    // 2. EXE 경로: 실행 디렉토리에 있는 Project4.exe
                    //string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Project4.exe");
                    string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Algorithm.exe");
                    string arguments = $"\"{csvPath}\"";

                    if (!File.Exists(exePath))
                    {
                        MessageBox.Show($"EXE 파일이 존재하지 않습니다:\n{exePath}", "실행 오류");
                        return;
                    }

                    // 3. EXE 실행 및 결과 반영
                    var result = RunDefectDetectionAndParseOutput(exePath, arguments);

                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage.Text = "결과 처리 중...";
                    });

                    // 결과 처리
                    Dispatcher.Invoke(() =>
                    {
                        currentSerialNumber = result.Item1;
                        List<DefectItem> items = result.Item2;


                        // 기존 불량 항목 초기화
                        defectItems.Clear();

                        // 파싱된 불량 항목 추가
                        foreach (var item in items)
                        {
                            defectItems.Add(item);
                        }

                        // 시리얼 번호 업데이트
                        if (!string.IsNullOrEmpty(currentSerialNumber))
                        {
                            SerialNumberValue.Text = currentSerialNumber;
                        }
                    });
                });

                // 모든 작업 완료된 후 UI 업데이트
                StatusMessage.Text = "검사 완료";
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = 100;

                await Task.Delay(1000);
                ProgressOverlay.Visibility = Visibility.Collapsed;

                // 버튼 활성화
                DefectBtn.IsEnabled = true;
                ClearBtn.IsEnabled = true;

                // 결과에 따라 상태 메세지 표시
                if (defectItems.Count == 0)
                {
                    MessageBox.Show("이물질이 검출되지 않았습니다. 정상 PCB입니다.", "검사 결과", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"{defectItems.Count}개의 이물질이 검출되었습니다.", "검사 결과", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // DB에 저장
                bool saveSuccess = await _pcbInspectionService.SaveInspectionResultAsync(currentSerialNumber, defectItems);

                if (saveSuccess)
                {
                    MessageBox.Show($"검사 결과가 DB에 저장되었습니다.\n시리얼 넘버: {currentSerialNumber}",
                        "저장 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("DB 저장에 실패했습니다.", "저장 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // AI 분석 버튼 활성화 (추가된 부분)
                AIAnalysisBtn.IsEnabled = (defectItems.Count > 0);
                isAiAnalysisComplete = false;
                AIInitialMessage.Visibility = Visibility.Visible;
                AILoadingMessage.Visibility = Visibility.Collapsed;
                AIAnalysisResults.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"검사 과정에서 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);

                // 오류 발생 시에도 UI 상태 정리
                ProgressOverlay.Visibility = Visibility.Collapsed;
                DefectBtn.IsEnabled = true;
                ClearBtn.IsEnabled = true;
            }
        }

        // 알고리즘(exe) 실행 후 출력되는 XML 로그를 파싱하는 함수
        private Tuple<string, List<DefectItem>> RunDefectDetectionAndParseOutput(string exePath, string arguments = "")
        {
            List<DefectItem> defectItems = new List<DefectItem>();
            StringBuilder logBuilder = new StringBuilder();
            string serialNumber = "";

            try
            {
                // 외부 프로세스(exe) 실행 설정
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true  // 콘솔 창을 보이게 할 경우 false, 숨길 경우 true
                };

                // 프로세스 실행 및 출력 캡처
                using (Process process = Process.Start(startInfo))
                {
                    // 표준 출력을 실시간으로 읽어서 StringBuilder에 저장
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = process.StandardOutput.ReadLine();
                        logBuilder.AppendLine(line);
                        Console.WriteLine(line);

                        // 시리얼 넘버 추출
                        if (line.Contains("<SerialNumber>") && line.Contains("</SerialNumber>"))
                        {
                            int startIndex = line.IndexOf("<SerialNumber>") + "<SerialNumber>".Length;
                            int endIndex = line.IndexOf("</SerialNumber>");
                            if (startIndex >= 0 && endIndex > startIndex)
                            {
                                serialNumber = line.Substring(startIndex, endIndex - startIndex);
                                Console.WriteLine($"시리얼 넘버 추출: {serialNumber}");
                            }
                        }
                    }
                    process.WaitForExit();
                }

                // 시리얼 넘버가 비어있으면 오류 메시지 표시
                if (string.IsNullOrEmpty(serialNumber))
                {
                    MessageBox.Show("알고리즘에서 시리얼 넘버를 생성하지 않았습니다. C++ 코드를 확인해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine("시리얼 넘버를 찾을 수 없습니다.");
                    serialNumber = "ERROR-NO-SERIAL";
                }

                // 이물질 추출
                string logContent = logBuilder.ToString();
                string pattern = @"<DefectItem>[\s\S]*?<\/DefectItem>";
                MatchCollection matches = Regex.Matches(logContent, pattern, RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    try
                    {
                        // XML 파싱
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(match.Value);

                        // 각 필드 값 추출
                        int id = int.Parse(xmlDoc.SelectSingleNode("//Id").InnerText);

                        // 좌표값에 소수점이 있을 경우 반올림하여 정수로 변환
                        double x = double.Parse(xmlDoc.SelectSingleNode("//X").InnerText);
                        double y = double.Parse(xmlDoc.SelectSingleNode("//Y").InnerText);
                        double width = double.Parse(xmlDoc.SelectSingleNode("//Width").InnerText);
                        double height = double.Parse(xmlDoc.SelectSingleNode("//Height").InnerText);

                        // PCB 실제 크기(mm)에 대한 상수 설정
                        const double PCB_WIDTH_MM = 240.0;  // PCB 너비(mm)
                        const double PCB_HEIGHT_MM = 77.5;  // PCB 높이(mm)

                        // mm에서 픽셀로 변환 (ORIGINAL_IMAGE_WIDTH, ORIGINAL_IMAGE_HEIGHT는 픽셀 단위)
                        int pixelX = (int)Math.Round(x * ORIGINAL_IMAGE_WIDTH / PCB_WIDTH_MM);
                        int pixelY = (int)Math.Round(y * ORIGINAL_IMAGE_HEIGHT / PCB_HEIGHT_MM);

                        // 너비도 픽셀 단위로 변환
                        double pixelWidth = width * ORIGINAL_IMAGE_WIDTH / PCB_WIDTH_MM;
                        double pixelHeight = height * ORIGINAL_IMAGE_HEIGHT / PCB_HEIGHT_MM;

                        // DefectItem 객체 생성 및 리스트에 추가
                        DefectItem item = new DefectItem
                        {
                            Id = id,
                            X = pixelX,
                            Y = pixelY,
                            Width = pixelWidth,
                            Height = pixelHeight
                        };

                        defectItems.Add(item);
                        Console.WriteLine($"불량 항목 추가됨: ID={id}, X={x}, Y={y}, Width={width}, Height={height}");
                    }
                    catch (Exception xmlEx)
                    {
                        Console.WriteLine($"XML 파싱 오류: {xmlEx.Message}");
                        Console.WriteLine($"문제가 된 XML: {match.Value}");
                    }
                }

                Console.WriteLine($"총 {defectItems.Count}개의 불량 항목을 파싱했습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"알고리즘 실행 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"오류 발생: {ex.Message}");
            }

            return new Tuple<string, List<DefectItem>>(serialNumber, defectItems);
        }
        #endregion

        #region 초기화 클릭 이벤트 리스너
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            DefectCanvas.Children.Clear();

            // 마커 딕셔너리 초기화
            defectMarkers.Clear();

            // 불량 항목 목록 초기화
            defectItems.Clear();

            // 불량 상세 정보 초기화
            DefectIdValue.Text = "";
            DefectPositionValue.Text = "";
            DefectHeightValue.Text = "";
            DefectSizeValue.Text = "";

            // 시리얼 번호 초기화
            SerialNumberValue.Text = "-";

            // 현재 PCB 시리얼 넘버 초기화
            currentSerialNumber = "";

            // AI 분석 관련 UI 초기화 (추가된 부분)
            AIAnalysisBtn.IsEnabled = false;
            isAiAnalysisComplete = false;
            AIInitialMessage.Visibility = Visibility.Visible;
            AILoadingMessage.Visibility = Visibility.Collapsed;
            AIAnalysisResults.Visibility = Visibility.Collapsed;
            DefectTypesList.Children.Clear();
            RecommendationsList.Items.Clear();
            HistoricalDataText.Text = "";
        }
        #endregion

        #region 체크박스 체크 이벤트 리스너
        // 체크박스 언체크 이벤트 - 불량 표시 보이기
        private void DefectVisibility_Checked(object sender, RoutedEventArgs e)
        {
            if (DefectCanvas != null)
            {
                DefectCanvas.Visibility = Visibility.Visible; // Canvas자체를 보임
            }
        }

        // 체크박스 언체크 이벤트 - 불량 표시 숨기기
        private void DefectVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DefectCanvas != null)
            {
                DefectCanvas.Visibility = Visibility.Collapsed; // Canvas자체를 숨김
            }
        }
        #endregion

        #region 검출 결과 목록 (우측 중앙)
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
                        Text = "정상 PCB입니다.",
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
        #endregion

        #region 결과 내보내기 클릭 이벤트 리스너 (우측 중앙)
        // 결과 내보내기 버튼 클릭 이벤트
        private void ExportResult_Click(object sender, EventArgs e)
        {
            // 내보낼 데이터가 있는지 확인
            if (defectItems == null || defectItems.Count == 0)
            {
                MessageBox.Show("내보낼 검출 결과가 없습니다", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 저장할 대화 상자 생성
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV 파일 (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"불량검출결과_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                // 대화 상자 표시 및 결과 확인
                bool? result = saveDialog.ShowDialog();

                // 사용자가 저장을 선택한 경우
                if (result == true)
                {
                    // 선택한 파일 경로로 CSV파일 생성
                    ExportToCsv(saveDialog.FileName);
                    // 성공 메세지 표시
                    MessageBox.Show("결과가 성공적으로 내보내졌습니다.", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"결과 내보내기 중 오류가 발생했습니다.: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // CSV 파일 내보내기 
        private void ExportToCsv(string filePath)
        {
            // CSV 파일 생성
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // 헤더
                writer.WriteLine("ID,X좌표,Y좌표,너비(um),높이(um)");

                // 각 불량 항목에 대한 데이터 작성
                foreach (var defect in defectItems)
                {
                    writer.WriteLine($"{defect.Id},{defect.X},{defect.Y},{defect.Width},{defect.Height}");
                }
            }
        }
        #endregion

        #region 불량 상세 정보 (우측 하단)
        // 헤더 생성
        private Grid CreateDefectResultHeader()
        {
            Grid grid = new Grid();
            grid.Margin = new Thickness(5);

            // 열 정의 - 5개로 변경
            for (int i = 0; i < 5; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 헤더 라벨
            string[] headers = new string[] { "ID", "X 위치", "Y 위치", "높이", "크기" };

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

            // 구분선 - ColumnSpan을 5로 변경
            Border separator = new Border
            {
                Height = 1,
                Background = Brushes.LightGray,
                Margin = new Thickness(0, 25, 0, 0)
            };
            Grid.SetColumnSpan(separator, 5);
            grid.Children.Add(separator);

            return grid;
        }

        // 개별 불량 결과 항목 생성
        private Grid CreateDefectResultItem(DefectItem defect)
        {
            Grid grid = new Grid();
            grid.Margin = new Thickness(5, 2, 5, 2);

            // 열 정의 - 5개로 변경
            for (int i = 0; i < 5; i++)
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

            // 너비 추가
            TextBlock widthText = new TextBlock
            {
                Text = defect.Width.ToString("F1"),
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(widthText, 4);
            grid.Children.Add(widthText);

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
                        ellipse.StrokeThickness = 10;
                    }
                }

                // 선택된 마커 강조
                if (marker is Ellipse selectedEllipse)
                {
                    selectedEllipse.Stroke = new SolidColorBrush(Colors.Lime);
                    selectedEllipse.StrokeThickness = 40;
                }
            }
        }

        //  불량 상세 정보 갱신 관련 함수
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

        // 마우스 휠 이벤트 핸들러 - 확대/축소 기능
        private void PCBContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Ctrl 키를 누른 상태에서 마우스 휠을 사용할 때만 확대/축소 (선택 사항)
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double zoom = e.Delta > 0 ? 0.1 : -0.1;

                // 최소/최대 확대/축소 제한 (예: 0.055배~5배)
                double newZoom = Math.Max(0.055, Math.Min(5, PCBScaleTransform.ScaleX + zoom));

                // 확대/축소 적용
                PCBScaleTransform.ScaleX = newZoom;
                PCBScaleTransform.ScaleY = newZoom;

                e.Handled = true;
            }
        }

        #region AI 불량 분석 기능
        // 불량 유형 정보 클래스
        public class DefectTypeInfo
        {
            public string TypeName { get; set; }
            public int Count { get; set; }
            public int Confidence { get; set; }
        }

        // AI 분석 결과 클래스
        public class AnalysisResult
        {
            public List<DefectTypeInfo> DefectTypes { get; set; }
            public string SeverityLevel { get; set; }
            public List<string> Recommendations { get; set; }
            public string HistoricalContext { get; set; }
        }

        // AI 분석 버튼 클릭 이벤트
        private async void AIAnalysisBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 분석 중복 실행 방지
                if (isAiAnalysisComplete)
                {
                    MessageBox.Show("이미 분석이 완료되었습니다. 새로운 분석을 위해 초기화 후 다시 시도해주세요.",
                        "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 로딩 상태 표시
                AIInitialMessage.Visibility = Visibility.Collapsed;
                AILoadingMessage.Visibility = Visibility.Visible;
                AIAnalysisResults.Visibility = Visibility.Collapsed;

                // 버튼 비활성화
                AIAnalysisBtn.IsEnabled = false;

                // 불량 데이터 준비
                StringBuilder defectDataBuilder = new StringBuilder();
                defectDataBuilder.AppendLine($"PCB 시리얼 번호: {currentSerialNumber}");
                defectDataBuilder.AppendLine($"검출된 불량 수: {defectItems.Count}");

                foreach (var defect in defectItems)
                {
                    defectDataBuilder.AppendLine($"불량ID: {defect.Id}, 위치: ({defect.X}, {defect.Y}), 크기: {defect.Width}x{defect.Height}");
                }

                // GPT 프롬프트 구성
                string prompt = $@"
당신은 PCB 불량 전문가입니다. 다음 PCB 불량 데이터를 분석하여 다음 형식으로 응답해주세요:

1. 불량 유형 분석: 이 PCB에서 발견된 불량의 유형을 분류해주세요. 각 유형별 개수와 신뢰도(%)를 제시해주세요.
2. 심각도 평가: '낮음', '중간', '높음' 중 하나로 평가해주세요.
3. 개선 권장사항: 3가지 이내로 구체적인 권장사항을 제시해주세요.
4. 과거 데이터 분석: 이 PCB의 불량 패턴에 대한 인사이트를 제공해주세요.

PCB 불량 데이터:
{defectDataBuilder.ToString()}

응답 형식은 다음과 같이 JSON 형식으로 작성해주세요:
{{
  ""defectTypes"": [
    {{ ""typeName"": ""불량유형명"", ""count"": 숫자, ""confidence"": 숫자 }},
    ...
  ],
  ""severityLevel"": ""낮음/중간/높음"",
  ""recommendations"": [
    ""권장사항1"",
    ""권장사항2"",
    ""권장사항3""
  ],
  ""historicalContext"": ""과거 데이터 분석 내용""
}}
";

                // API 호출 (비동기)
                AnalysisResult result = await _openAIService.GetAnalysisFromGPT(prompt);

                // 분석 결과 표시
                DisplayAnalysisResults(result);

                // 상태 업데이트
                AILoadingMessage.Visibility = Visibility.Collapsed;
                AIAnalysisResults.Visibility = Visibility.Visible;
                isAiAnalysisComplete = true;

                // 버튼 활성화 (사용자가 다시 분석할 수 있도록)
                AIAnalysisBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"AI 분석 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);

                // 오류 발생 시 초기 상태로 복귀
                AILoadingMessage.Visibility = Visibility.Collapsed;
                AIInitialMessage.Visibility = Visibility.Visible;
                AIAnalysisBtn.IsEnabled = (defectItems.Count > 0);
            }
        }

        // 분석 초기화 버튼 클릭 이벤트
        private void ResetAnalysisBtn_Click(object sender, RoutedEventArgs e)
        {
            // 상태 초기화
            AIAnalysisResults.Visibility = Visibility.Collapsed;
            AIInitialMessage.Visibility = Visibility.Visible;
            isAiAnalysisComplete = false;
            AIAnalysisBtn.IsEnabled = (defectItems.Count > 0);

            // 내용 초기화
            DefectTypesList.Children.Clear();
            RecommendationsList.Items.Clear();
            HistoricalDataText.Text = "";
        }

        // 분석 결과 표시 함수
        private void DisplayAnalysisResults(AnalysisResult result)
        {
            Dispatcher.Invoke(() =>
            {
                // 불량 유형 목록 표시
                DefectTypesList.Children.Clear();
                foreach (var defectType in result.DefectTypes)
                {
                    // 유형별 항목 추가
                    if (defectType.Count > 0)
                    {
                        Grid typeGrid = new Grid();
                        typeGrid.Margin = new Thickness(0, 4, 0, 4);

                        typeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        typeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

                        // 유형 이름
                        TextBlock typeName = new TextBlock
                        {
                            Text = $"{defectType.TypeName} ({defectType.Count}개)",
                            Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33)),
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumn(typeName, 0);
                        typeGrid.Children.Add(typeName);

                        // 신뢰도 바
                        StackPanel confidencePanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        // 신뢰도 프로그레스 바
                        ProgressBar confidenceBar = new ProgressBar
                        {
                            Value = defectType.Confidence,
                            Maximum = 100,
                            Width = 100,
                            Height = 8,
                            Margin = new Thickness(0, 0, 8, 0),
                            Foreground = new SolidColorBrush(Color.FromRgb(25, 118, 210))
                        };

                        // 신뢰도 텍스트
                        TextBlock confidenceText = new TextBlock
                        {
                            Text = $"{defectType.Confidence}%",
                            Foreground = new SolidColorBrush(Color.FromRgb(97, 97, 97)),
                            Width = 40
                        };

                        confidencePanel.Children.Add(confidenceBar);
                        confidencePanel.Children.Add(confidenceText);

                        Grid.SetColumn(confidencePanel, 1);
                        typeGrid.Children.Add(confidencePanel);

                        DefectTypesList.Children.Add(typeGrid);
                    }
                }

                // 심각도 표시
                SeverityText.Text = result.SeverityLevel;

                // 심각도에 따른 색상 설정
                if (result.SeverityLevel == "낮음")
                {
                    SeverityIndicator.Background = new SolidColorBrush(Color.FromRgb(200, 230, 201));
                    SeverityText.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                }
                else if (result.SeverityLevel == "중간")
                {
                    SeverityIndicator.Background = new SolidColorBrush(Color.FromRgb(255, 236, 179));
                    SeverityText.Foreground = new SolidColorBrush(Color.FromRgb(255, 111, 0));
                }
                else // 높음
                {
                    SeverityIndicator.Background = new SolidColorBrush(Color.FromRgb(255, 205, 210));
                    SeverityText.Foreground = new SolidColorBrush(Color.FromRgb(198, 40, 40));
                }

                // 권장사항 목록
                RecommendationsList.Items.Clear();
                foreach (var recommendation in result.Recommendations)
                {
                    ListBoxItem item = new ListBoxItem();

                    TextBlock text = new TextBlock
                    {
                        Text = recommendation,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
                    };

                    item.Content = text;
                    RecommendationsList.Items.Add(item);
                }

                // 과거 데이터 분석
                HistoricalDataText.Text = result.HistoricalContext;
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