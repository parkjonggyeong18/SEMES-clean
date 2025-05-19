using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace semes.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint = "https://api.openai.com/v1/chat/completions";
        private readonly string _model = "chatgpt-4o-latest";  

        public OpenAIService()
        {
            _httpClient = new HttpClient();

            // API 키 가져오기 - 설정에서 로드하거나 대체 방법 사용
            _apiKey = GetApiKey();

            // 헤더 설정
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(120); // 타임아웃 설정
        }

        private string GetApiKey()
        {
            // 1. 구성 파일에서 API 키 시도
            string apiKey = ConfigurationManager.AppSettings["OpenAIApiKey"];

            // 2. 환경 변수에서 시도
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            }

            // 3. 백업 키 또는 오류 처리
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.WriteLine("⚠️ OpenAI API 키가 구성되지 않았습니다. 기능이 제한될 수 있습니다.");

  
                apiKey = "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";
            }

            return apiKey;
        }

        public async Task<DefectDetectionPage.AnalysisResult> GetAnalysisFromGPT(string prompt)
        {
            // 요청 타임스탬프 기록
            var startTime = DateTime.Now;
            Debug.WriteLine($"OpenAI API 요청 시작: {startTime}");

            try
            {
                // 요청 데이터 구성
                var requestData = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a PCB defect analysis expert with experience in semiconductor manufacturing. You can identify defect patterns, assess severity levels, and provide actionable recommendations based on defect location, size, and other parameters." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.2, // 낮은 온도로 더 일관된 응답 생성
                    max_tokens = 2000,
                    top_p = 0.95,
                    frequency_penalty = 0,
                    presence_penalty = 0
                };

                // JSON 직렬화
                var requestBody = JsonSerializer.Serialize(requestData);
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // 요청 전송 및 응답 수신
                var response = await _httpClient.PostAsync(_endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                // 요청 시간 측정
                var elapsedTime = DateTime.Now - startTime;
                Debug.WriteLine($"OpenAI API 응답 수신: {elapsedTime.TotalSeconds}초 소요");

                // 응답 상태 코드 확인
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody);
                    var errorMessage = errorResponse?.error?.message ?? $"API 오류: {response.StatusCode}";

                    Debug.WriteLine($"OpenAI API 오류: {errorMessage}");
                    throw new Exception($"OpenAI API 호출 실패: {errorMessage}");
                }

                // API 응답 파싱
                var apiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseBody);
                string resultText = apiResponse?.choices?[0]?.message?.content;

                if (string.IsNullOrEmpty(resultText))
                {
                    throw new Exception("API 응답에서 결과를 찾을 수 없습니다.");
                }

                // 로그
                Debug.WriteLine($"토큰 사용량: {apiResponse.usage.total_tokens}");

                // JSON 응답 추출 - 개선된 정규식 방법
                return ExtractAndParseJsonResult(resultText);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP 요청 오류: {ex.Message}");
                throw new Exception($"API 통신 오류: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"요청 타임아웃: {ex.Message}");
                throw new Exception("API 요청 시간이 초과되었습니다. 나중에 다시 시도하세요.", ex);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON 파싱 오류: {ex.Message}");
                throw new Exception("응답 데이터 형식 오류", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"예상치 못한 오류: {ex}");
                throw new Exception($"AI 분석 중 오류 발생: {ex.Message}", ex);
            }
        }

        // JSON 결과 추출 및 파싱 
        private DefectDetectionPage.AnalysisResult ExtractAndParseJsonResult(string resultText)
        {
            // JSON 블록 찾기
            int startIndex = resultText.IndexOf('{');
            int endIndex = resultText.LastIndexOf('}');

            if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
            {
                // JSON이 명시적으로 포함되지 않은 경우 결과 구조화 시도
                return StructureNonJsonResponse(resultText);
            }

            string jsonResult = resultText.Substring(startIndex, endIndex - startIndex + 1);

            try
            {
                // JSON 파싱 옵션
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                // JSON 파싱 및 결과 반환
                var result = JsonSerializer.Deserialize<DefectDetectionPage.AnalysisResult>(jsonResult, options);

                // 결과 유효성 검사
                ValidateResult(result);

                return result;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON 파싱 실패: {ex.Message}");
                Debug.WriteLine($"파싱 시도한 JSON: {jsonResult}");

                // 대체 파싱 방법 시도
                return StructureNonJsonResponse(resultText);
            }
        }

        // 결과 유효성 검사 및 기본값 설정
        private void ValidateResult(DefectDetectionPage.AnalysisResult result)
        {
            if (result == null)
            {
                throw new Exception("분석 결과가 null입니다.");
            }

            // 불량 유형 null 확인 및 초기화
            if (result.DefectTypes == null)
            {
                result.DefectTypes = new List<DefectDetectionPage.DefectTypeInfo>();
            }

            // 심각도 null 확인 및 기본값 설정
            if (string.IsNullOrEmpty(result.SeverityLevel))
            {
                result.SeverityLevel = "중간";
            }

            // 권장사항 null 확인 및 초기화
            if (result.Recommendations == null)
            {
                result.Recommendations = new List<string>();
            }

            // 역사적 컨텍스트 null 확인 및 기본값 설정
            if (string.IsNullOrEmpty(result.HistoricalContext))
            {
                result.HistoricalContext = "충분한 과거 데이터가 없습니다.";
            }
        }

        // JSON이 아닌 응답을 구조화하기 위한 대체 메서드
        private DefectDetectionPage.AnalysisResult StructureNonJsonResponse(string text)
        {
            Debug.WriteLine("비정형 응답을 구조화하는 중...");

            var result = new DefectDetectionPage.AnalysisResult
            {
                DefectTypes = new List<DefectDetectionPage.DefectTypeInfo>(),
                Recommendations = new List<string>(),
                SeverityLevel = "중간",
                HistoricalContext = "데이터 분석 불가"
            };

            try
            {
                // 텍스트 분석을 통한 구조화

                // 1. 불량 유형 추출 시도
                if (text.Contains("불량 유형") || text.Contains("defect type"))
                {
                    var defectTypes = ExtractDefectTypes(text);
                    if (defectTypes.Any())
                    {
                        result.DefectTypes = defectTypes;
                    }
                }

                // 2. 심각도 추출 시도
                var severity = ExtractSeverity(text);
                if (!string.IsNullOrEmpty(severity))
                {
                    result.SeverityLevel = severity;
                }

                // 3. 권장사항 추출 시도
                var recommendations = ExtractRecommendations(text);
                if (recommendations.Any())
                {
                    result.Recommendations = recommendations;
                }

                // 4. 역사적 컨텍스트 추출 시도
                var context = ExtractHistoricalContext(text);
                if (!string.IsNullOrEmpty(context))
                {
                    result.HistoricalContext = context;
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"비정형 응답 구조화 실패: {ex.Message}");

                // 기본 결과 반환
                result.DefectTypes.Add(new DefectDetectionPage.DefectTypeInfo
                {
                    TypeName = "미확인 불량",
                    Count = 1,
                    Confidence = 50
                });

                result.Recommendations.Add("PCB 검사 결과를 기술 전문가에게 확인받으세요.");
                result.HistoricalContext = "AI 분석 결과를 구조화하는 데 문제가 발생했습니다.";

                return result;
            }
        }

        // 비정형 텍스트에서 불량 유형 추출
        private List<DefectDetectionPage.DefectTypeInfo> ExtractDefectTypes(string text)
        {
            var result = new List<DefectDetectionPage.DefectTypeInfo>();

            // 간단한 패턴 매칭으로 추출
            string[] possibleTypes = { "이물질", "패드", "스크래치", "미세균열", "불량", "오염", "부식", "미접합", "산화", "마이크로크랙" };

            foreach (var type in possibleTypes)
            {
                if (text.Contains(type))
                {
                    // 숫자 추출 시도
                    int count = 1;
                    int confidence = 70;

                    // 유형 뒤에 숫자가 있는지 확인
                    var countMatch = System.Text.RegularExpressions.Regex.Match(text, $"{type}[^0-9]*([0-9]+)");
                    if (countMatch.Success)
                    {
                        int.TryParse(countMatch.Groups[1].Value, out count);
                    }

                    // 신뢰도 추출 시도
                    var confidenceMatch = System.Text.RegularExpressions.Regex.Match(text, $"{type}[^0-9%]*([0-9]+)%");
                    if (confidenceMatch.Success)
                    {
                        int.TryParse(confidenceMatch.Groups[1].Value, out confidence);
                    }

                    result.Add(new DefectDetectionPage.DefectTypeInfo
                    {
                        TypeName = type,
                        Count = count,
                        Confidence = confidence
                    });
                }
            }

            // 결과가 없으면 기본값 추가
            if (!result.Any())
            {
                result.Add(new DefectDetectionPage.DefectTypeInfo
                {
                    TypeName = "미확인 불량",
                    Count = 1,
                    Confidence = 50
                });
            }

            return result;
        }

        // 심각도 추출
        private string ExtractSeverity(string text)
        {
            if (text.Contains("심각") || text.Contains("위험") || text.Contains("높음"))
            {
                return "높음";
            }
            else if (text.Contains("중간") || text.Contains("보통"))
            {
                return "중간";
            }
            else if (text.Contains("낮음") || text.Contains("경미"))
            {
                return "낮음";
            }

            return "중간"; // 기본값
        }

        // 권장사항 추출
        private List<string> ExtractRecommendations(string text)
        {
            var recommendations = new List<string>();

            // 권장사항 구문 찾기
            string[] markers = { "권장사항", "권장", "추천", "조치", "개선" };

            foreach (var marker in markers)
            {
                var index = text.IndexOf(marker);
                if (index >= 0)
                {
                    // 마커 이후 텍스트 추출
                    var remainingText = text.Substring(index);

                    // 다음 섹션 또는 끝까지
                    var nextSectionIndex = remainingText.IndexOfAny(new[] { '\n', '\r' }, remainingText.IndexOfAny(new[] { '\n', '\r' }) + 1);

                    if (nextSectionIndex > 0)
                    {
                        remainingText = remainingText.Substring(0, nextSectionIndex);
                    }

                    // 라인별로 분리하여 권장사항 찾기
                    var lines = remainingText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines.Skip(1).Take(3)) // 첫 줄 제외, 최대 3개
                    {
                        var trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine) && trimmedLine.Length > 10)
                        {
                            // 번호 마커 제거
                            trimmedLine = System.Text.RegularExpressions.Regex.Replace(trimmedLine, @"^\d+[\.\)\-]\s*", "");
                            recommendations.Add(trimmedLine);
                        }
                    }

                    if (recommendations.Any())
                    {
                        break;
                    }
                }
            }

            // 권장사항이 추출되지 않았으면 기본 권장사항 제공
            if (!recommendations.Any())
            {
                recommendations.Add("PCB 표면에 대한 추가 검사를 실시하세요.");
                recommendations.Add("생산 환경의 청정도를 점검하세요.");
                recommendations.Add("불량이 발생한 위치의 패턴을 분석하세요.");
            }

            return recommendations.Take(3).ToList(); // 최대 3개로 제한
        }

        // 역사적 컨텍스트 추출
        private string ExtractHistoricalContext(string text)
        {
            string[] markers = { "과거 데이터", "역사적", "패턴", "추세", "경향" };

            foreach (var marker in markers)
            {
                var index = text.IndexOf(marker);
                if (index >= 0)
                {
                    // 마커 이후 텍스트 추출
                    var startIndex = text.IndexOf('\n', index);
                    if (startIndex > 0)
                    {
                        var endIndex = text.IndexOf("\n\n", startIndex);
                        if (endIndex > startIndex)
                        {
                            return text.Substring(startIndex, endIndex - startIndex).Trim();
                        }
                        else
                        {
                            return text.Substring(startIndex).Trim();
                        }
                    }
                }
            }

            return "충분한 과거 데이터가 없습니다.";
        }

        // OpenAI API 응답 클래스
        private class OpenAIResponse
        {
            public string id { get; set; }
            public string @object { get; set; }
            public long created { get; set; }
            public string model { get; set; }
            public List<Choice> choices { get; set; }
            public Usage usage { get; set; }

            public class Choice
            {
                public int index { get; set; }
                public Message message { get; set; }
                public string finish_reason { get; set; }
            }

            public class Message
            {
                public string role { get; set; }
                public string content { get; set; }
            }

            public class Usage
            {
                public int prompt_tokens { get; set; }
                public int completion_tokens { get; set; }
                public int total_tokens { get; set; }
            }
        }

        // 오류 응답 클래스
        private class ErrorResponse
        {
            public ErrorDetail error { get; set; }

            public class ErrorDetail
            {
                public string message { get; set; }
                public string type { get; set; }
                public string param { get; set; }
                public string code { get; set; }
            }
        }
    }
}