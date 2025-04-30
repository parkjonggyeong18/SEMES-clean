using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace semes
{
    /// <summary>
    /// DefectDetectionPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DefectDetectionPage : Page
    {
        // PCB 이미지 경로
        private string currentImagePath;

        // 불량 목록 (실제 구현에서는 더 복잡한 클래스가 필요할 수 있음)
        private List<DefectItem> defectItems = new List<DefectItem>();

        // 선택된 불량 항목
        private DefectItem selectedDefect;

        public DefectDetectionPage()
        {
            InitializeComponent();
        }

        // 이미지 업로드 버튼 클릭 이벤트 핸들러
        private void BtnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.bmp|모든 파일|*.*",
                Title = "PCB 이미지 선택"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    currentImagePath = openFileDialog.FileName;
                    LoadImage(currentImagePath);

                    // 이미지 로드 후 불량 리스트 초기화
                    ClearDefectList();

                    // PCB 정보 표시
                    DisplayPCBInfo(currentImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 불량검출 버튼 클릭 이벤트 핸들러
        private void BtnDetectDefects_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentImagePath))
            {
                MessageBox.Show("먼저 PCB 이미지를 업로드해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 여기서 실제 불량 검출 알고리즘을 구현하거나 호출해야 함
                // 예시 데이터로 임의의 불량을 생성
                DetectDefects();

                // 불량 목록 UI 업데이트
                UpdateDefectListUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"불량 검출 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 초기화 버튼 클릭 이벤트 핸들러
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // 이미지 초기화
            currentImagePath = null;

            // 이미지 뷰 초기화
            // PCB 이미지 요소가 있다면 Source를 null로 설정

            // 불량 목록 초기화
            ClearDefectList();

            // PCB 정보 초기화
            ClearPCBInfo();

            // 불량 상세 정보 초기화
            ClearDefectDetailInfo();
        }

        // 불량 표시 체크박스 이벤트 핸들러
        private void ChkShowDefects_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // 불량 표시 여부에 따라 UI 업데이트
            // 실제 구현에서는 이미지 위에 불량 위치를 표시하는 로직이 필요
        }

        // 불량 위치로 이동 버튼 클릭 이벤트 핸들러
        private void BtnMoveToDefect_Click(object sender, RoutedEventArgs e)
        {
            if (selectedDefect == null)
            {
                MessageBox.Show("이동할 불량을 먼저 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 선택된 불량 위치로 이미지 스크롤 또는 뷰 이동
            // 실제 구현에서는 스크롤 뷰어나 다른 방식으로 이미지 내 위치로 이동 구현
        }

        // 이미지 로드 함수
        private void LoadImage(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            // PCB 이미지 컨트롤에 이미지 설정
            // 예: pcbImageControl.Source = bitmapImage;
        }

        // 불량 검출 함수 (예시 데이터 생성)
        private void DetectDefects()
        {
            // 실제 구현에서는 이미지 처리 알고리즘을 통한 불량 검출 로직이 들어가야 함
            // 여기서는 예시 데이터만 생성

            // 기존 불량 목록 초기화
            defectItems.Clear();

            // 임의의 불량 데이터 생성 (예시)
            Random random = new Random();
            int defectCount = random.Next(1, 6); // 1~5개의 불량 생성

            for (int i = 0; i < defectCount; i++)
            {
                DefectItem defect = new DefectItem
                {
                    Id = i + 1,
                    X = random.Next(100, 3900),
                    Y = random.Next(100, 11900),
                    Height = random.Next(10, 100) / 10.0,
                    Width = random.Next(50, 200) / 10.0
                };

                defectItems.Add(defect);
            }
        }

        // 불량 목록 UI 업데이트
        private void UpdateDefectListUI()
        {
            // XAML에 정의된 목록 컨트롤에 불량 항목 추가
            // ListView나 DataGrid 등이 있다면 해당 컨트롤의 ItemsSource를 설정

            // 검출 결과 제목 업데이트
            // 예: txtDefectResultTitle.Text = $"검출 결과 ({defectItems.Count}개)";

            // 검출 결과가 없을 경우 메시지 표시
            if (defectItems.Count == 0)
            {
                // 검출 결과 없음 메시지 표시
            }
        }

        // 불량 항목 선택 이벤트 핸들러
        private void DefectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 선택된 불량 항목 가져오기
            // 예: DefectItem selectedItem = (DefectItem)defectListView.SelectedItem;

            // 선택된 항목이 있으면 상세 정보 표시
            if (selectedDefect != null)
            {
                DisplayDefectDetailInfo(selectedDefect);
            }
            else
            {
                ClearDefectDetailInfo();
            }
        }

        // PCB 정보 표시
        private void DisplayPCBInfo(string imagePath)
        {
            try
            {
                // 실제 구현에서는 이미지에서 PCB 정보를 추출하거나 관련 데이터를 가져와야 함
                // 여기서는 예시 데이터 표시

                // 파일 정보 가져오기
                FileInfo fileInfo = new FileInfo(imagePath);
                long fileSizeBytes = fileInfo.Length;
                string fileSizeMB = (fileSizeBytes / (1024.0 * 1024.0)).ToString("F2");

                // XAML에 정의된 텍스트 요소에 정보 설정
                // 예: txtPCBSize.Text = "77.5mm x 240mm";
                // 예: txtPCBResolution.Text = "4000 x 12000";
                // 예: txtPCBPixelSize.Text = "-";
                // 예: txtPCBDataSize.Text = $"약 {fileSizeMB}MB";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PCB 정보 표시 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 불량 상세 정보 표시
        private void DisplayDefectDetailInfo(DefectItem defect)
        {
            // XAML에 정의된 텍스트 요소에 정보 설정
            // 예: txtDefectId.Text = defect.Id.ToString();
            // 예: txtDefectPosition.Text = $"{defect.X}, {defect.Y}";
            // 예: txtDefectHeight.Text = defect.Height.ToString("F1");
            // 예: txtDefectSize.Text = $"{defect.Width} x {defect.Height}";
        }

        // 불량 목록 초기화
        private void ClearDefectList()
        {
            defectItems.Clear();
            // UI 컨트롤 초기화
            // 예: defectListView.ItemsSource = null;
        }

        // PCB 정보 초기화
        private void ClearPCBInfo()
        {
            // 텍스트 요소 초기화
            // 예: txtPCBSize.Text = "-";
            // 예: txtPCBResolution.Text = "-";
            // 예: txtPCBPixelSize.Text = "-";
            // 예: txtPCBDataSize.Text = "-";
        }

        // 불량 상세 정보 초기화
        private void ClearDefectDetailInfo()
        {
            // 텍스트 요소 초기화
            // 예: txtDefectId.Text = "-";
            // 예: txtDefectPosition.Text = "-";
            // 예: txtDefectHeight.Text = "-";
            // 예: txtDefectSize.Text = "-";
        }
    }

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