using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using semes.Services;

namespace semes.Pages
{
    public partial class ChatBotPage : Page
    {
        private readonly HttpClient _httpClient;
        private bool _isProcessing = false;

        // 서비스 인스턴스들
        private readonly DashboardService _dashboardService;
        private readonly DefectStatsService _defectStatsService;

        // API 키
        private const string API_KEY = "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";

        // 정적 메시지 컬렉션 - 페이지 인스턴스가 바뀌어도 유지됨
        private static ObservableCollection<ChatMessage> _staticMessages;

        // 초기화 여부를 체크하는 정적 변수
        private static bool _isInitialized = false;

        public ChatBotPage()
        {
            InitializeComponent();

            // HTTP 클라이언트 설정
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            // 서비스 초기화
            _dashboardService = new DashboardService();
            _defectStatsService = new DefectStatsService();

            // 정적 메시지 컬렉션 초기화 (처음 한 번만)
            if (_staticMessages == null)
            {
                _staticMessages = new ObservableCollection<ChatMessage>();
            }

            // ListView에 정적 컬렉션 바인딩
            ChatListView.ItemsSource = _staticMessages;

            // 초기 메시지 추가 (앱 실행 후 처음 한 번만)
            if (!_isInitialized)
            {
                AddMessage("AI 어시스턴트", "안녕하세요! PCB 검사 데이터 분석을 도와드립니다.\n\n예시 질문:\n- 오늘 검사 결과는 어때?\n- 지난 7일 불량률 추이를 보여줘\n- 최근 불량이 많은 PCB 목록은?\n- 전체 품질 통계 알려줘", false);
                _isInitialized = true;
            }

            // 페이지가 로드될 때마다 스크롤을 맨 아래로
            this.Loaded += (s, e) => ScrollToBottom();

            this.Unloaded += (s, e) => _httpClient?.Dispose();
        }

        private void AddMessage(string sender, string text, bool isUser)
        {
            // UI 스레드에서 실행 보장
            Dispatcher.Invoke(() =>
            {
                _staticMessages.Add(new ChatMessage
                {
                    Sender = sender,
                    Text = text,
                    Timestamp = DateTime.Now,
                    IsUser = isUser
                });
                ScrollToBottom();
            });
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ChatListView.Items.Count > 0)
                {
                    ChatListView.ScrollIntoView(ChatListView.Items[ChatListView.Items.Count - 1]);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !_isProcessing)
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text) || _isProcessing)
                return;

            string userText = InputTextBox.Text.Trim();
            InputTextBox.Text = "";

            // 사용자 메시지 추가
            AddMessage("사용자", userText, true);

            // UI 상태 변경
            _isProcessing = true;
            SendButton.Content = "분석중...";
            SendButton.IsEnabled = false;

            try
            {
                // 1. 사용자 질문 분석 및 DB 데이터 조회
                string dbAnalysisResult = await AnalyzeUserQueryAndFetchData(userText);

                // 2. AI에게 분석 결과와 함께 질문
                string systemPrompt = @"당신은 PCB 검사 데이터 분석 전문가입니다. 
사용자의 질문에 대해 제공된 DB 데이터를 바탕으로 정확하고 유용한 답변을 해주세요.
답변은 한국어로 친근하고 이해하기 쉽게 작성해주세요.
숫자나 데이터가 있을 때는 요약과 함께 인사이트를 제공해주세요.";

                var requestData = new
                {
                    model = "chatgpt-4o-latest",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = $"사용자 질문: {userText}\n\nDB 데이터 분석 결과:\n{dbAnalysisResult}" }
                    },
                    max_tokens = 1000,
                    temperature = 0.7
                };

                // JSON 변환
                string jsonContent = JsonSerializer.Serialize(requestData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // API 호출
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    // 응답 처리
                    string responseText = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseText);

                    if (responseData.TryGetProperty("choices", out var choices) &&
                        choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("message", out var message) &&
                            message.TryGetProperty("content", out var content))
                        {
                            string aiResponse = content.GetString() ?? "응답을 받지 못했습니다.";
                            AddMessage("AI 어시스턴트", aiResponse, false);
                        }
                        else
                        {
                            AddMessage("AI 어시스턴트", "응답 형식 오류가 발생했습니다.", false);
                        }
                    }
                    else
                    {
                        AddMessage("AI 어시스턴트", "응답에서 선택지를 찾을 수 없습니다.", false);
                    }
                }
                else
                {
                    // API 호출 실패 시 DB 데이터만 표시
                    AddMessage("AI 어시스턴트", $"API 호출에 실패했지만 DB 데이터를 조회했습니다:\n\n{dbAnalysisResult}", false);
                }
            }
            catch (Exception ex)
            {
                AddMessage("AI 어시스턴트", $"오류가 발생했습니다: {ex.Message}", false);
            }
            finally
            {
                // UI 상태 복원
                _isProcessing = false;
                SendButton.Content = "전송";
                SendButton.IsEnabled = true;
                InputTextBox.Focus();
            }
        }

        private async Task<string> AnalyzeUserQueryAndFetchData(string userQuery)
        {
            var result = new StringBuilder();
            DateTime today = DateTime.Today;

            try
            {
                // 질문 유형별 데이터 조회
                string lowerQuery = userQuery.ToLower();

                // 1. 오늘/당일 검사 결과 관련
                if (lowerQuery.Contains("오늘") || lowerQuery.Contains("당일") || lowerQuery.Contains("현재"))
                {
                    var (total, good, defect) = await _dashboardService.GetDashboardStatsByDateAsync(today);
                    double defectRate = total > 0 ? (double)defect / total * 100 : 0;

                    result.AppendLine($"## 오늘({today:yyyy-MM-dd}) 검사 결과");
                    result.AppendLine($"- 총 검사: {total}건");
                    result.AppendLine($"- 양품: {good}건");
                    result.AppendLine($"- 불량: {defect}건");
                    result.AppendLine($"- 불량률: {defectRate:F1}%\n");

                    // 최근 검사 기록도 추가
                    var recentRecords = await _dashboardService.GetRecentInspectionRecordsAsync(5);
                    result.AppendLine("### 최근 5건 검사 기록");
                    foreach (var record in recentRecords)
                    {
                        result.AppendLine($"- {record.SerialNumber}: {(record.IsGood ? "양품" : $"불량({record.DefectCount}개)")} [{record.InspectionDate:HH:mm}]");
                    }
                    result.AppendLine();
                }

                // 2. 불량률 추이/통계 관련
                if (lowerQuery.Contains("불량률") || lowerQuery.Contains("추이") || lowerQuery.Contains("통계"))
                {
                    // 7일간 불량률 추이
                    var dailyRates = await _dashboardService.GetDailyDefectRateTrendAsync(7);
                    if (dailyRates.Any())
                    {
                        result.AppendLine("## 최근 7일 불량률 추이");
                        foreach (var rate in dailyRates.OrderBy(r => r.Date))
                        {
                            result.AppendLine($"- {rate.Date:MM/dd}: {rate.DefectRate:F1}% (총 {rate.TotalCount}건 중 {rate.DefectCount}건 불량)");
                        }
                        result.AppendLine();
                    }

                    // 7일 평균 불량률
                    double avgRate = await _dashboardService.Get7DayAverageDefectRateAsync();
                    result.AppendLine($"### 7일 평균 불량률: {avgRate:F1}%\n");
                }

                // 3. 불량 PCB 관련
                if (lowerQuery.Contains("불량") && (lowerQuery.Contains("목록") || lowerQuery.Contains("리스트") || lowerQuery.Contains("많은")))
                {
                    var topDefects = await _dashboardService.GetTopDefectPCBsAsync(10);
                    if (topDefects.Any())
                    {
                        result.AppendLine("## 불량 개수가 많은 PCB (상위 10개)");
                        foreach (var pcb in topDefects)
                        {
                            result.AppendLine($"- {pcb.SerialNumber}: {pcb.DefectCount}개 불량 [{pcb.InspectionDate:MM/dd HH:mm}]");
                        }
                        result.AppendLine();
                    }
                }

                // 4. 전체 통계 관련
                if (lowerQuery.Contains("전체") || lowerQuery.Contains("종합") || lowerQuery.Contains("전반적"))
                {
                    var stats = await _dashboardService.GetQualityStatisticsAsync();
                    result.AppendLine("## 전체 품질 통계");
                    result.AppendLine($"- 총 검사 횟수: {stats.TotalInspections}건");
                    result.AppendLine($"- 양품: {stats.GoodCount}건");
                    result.AppendLine($"- 불량품: {stats.DefectCount}건");
                    result.AppendLine($"- 전체 불량률: {stats.DefectRate:F1}%");
                    result.AppendLine($"- 평균 불량 개수: {stats.AverageDefectCount:F1}개");
                    result.AppendLine($"- 최대 불량 개수: {stats.MaxDefectCount}개");
                    result.AppendLine($"- 검사 기간: {stats.EarliestInspection:yyyy-MM-dd} ~ {stats.LatestInspection:yyyy-MM-dd}\n");
                }

                // 5. 특정 날짜 관련
                if (lowerQuery.Contains("어제") || lowerQuery.Contains("전일"))
                {
                    DateTime yesterday = today.AddDays(-1);
                    var (total, good, defect) = await _dashboardService.GetDashboardStatsByDateAsync(yesterday);
                    double defectRate = total > 0 ? (double)defect / total * 100 : 0;

                    result.AppendLine($"## 어제({yesterday:yyyy-MM-dd}) 검사 결과");
                    result.AppendLine($"- 총 검사: {total}건");
                    result.AppendLine($"- 양품: {good}건");
                    result.AppendLine($"- 불량: {defect}건");
                    result.AppendLine($"- 불량률: {defectRate:F1}%\n");
                }

                // 6. 기본적으로 최근 검사 결과 제공
                if (result.Length == 0)
                {
                    var recentRecords = await _dashboardService.GetRecentInspectionRecordsAsync(10);
                    result.AppendLine("## 최근 검사 기록 (10건)");
                    foreach (var record in recentRecords)
                    {
                        result.AppendLine($"- {record.SerialNumber}: {(record.IsGood ? "양품" : $"불량({record.DefectCount}개)")} [{record.InspectionDate:MM/dd HH:mm}]");
                    }
                    result.AppendLine();

                    // 오늘 요약도 추가
                    var (todayTotal, todayGood, todayDefect) = await _dashboardService.GetDashboardStatsByDateAsync(today);
                    double todayRate = todayTotal > 0 ? (double)todayDefect / todayTotal * 100 : 0;
                    result.AppendLine($"### 오늘 요약: 총 {todayTotal}건 검사, 불량률 {todayRate:F1}%");
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"DB 조회 중 오류 발생: {ex.Message}");
            }

            return result.ToString();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // 정적 컬렉션을 초기화
            _staticMessages.Clear();

            // 초기화 후 환영 메시지 추가
            AddMessage("AI 어시스턴트", "대화가 초기화되었습니다. PCB 검사 데이터에 대해 궁금한 것을 물어보세요!", false);
        }

        // 앱 종료 시 대화 내용을 완전히 리셋하는 메서드 (필요한 경우 사용)
        public static void ResetChatHistory()
        {
            _staticMessages?.Clear();
            _isInitialized = false;
        }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsUser { get; set; }
    }
}