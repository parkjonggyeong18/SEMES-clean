using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;

namespace semes.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint = "https://api.openai.com/v1/chat/completions";

        public OpenAIService()
        {
            _httpClient = new HttpClient();


            _apiKey = ConfigurationManager.AppSettings["OpenAIApiKey"] ?? "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";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<DefectDetectionPage.AnalysisResult> GetAnalysisFromGPT(string prompt)
        {
            var requestData = new
            {
                model = "chatgpt-4o-latest", 
                messages = new[]
                {
                    new { role = "system", content = "You are a PCB defect analysis expert." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 1500
            };

            var requestBody = JsonSerializer.Serialize(requestData);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API 호출 실패: {response.StatusCode} - {responseBody}");
            }

            // API 응답 파싱
            var apiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseBody);
            string resultText = apiResponse?.choices?[0]?.message?.content;

            if (string.IsNullOrEmpty(resultText))
            {
                throw new Exception("API 응답에서 결과를 찾을 수 없습니다.");
            }

            // JSON 응답 추출
            int startIndex = resultText.IndexOf('{');
            int endIndex = resultText.LastIndexOf('}');

            if (startIndex < 0 || endIndex < 0 || endIndex <= startIndex)
            {
                throw new Exception("API 응답에서 유효한 JSON을 찾을 수 없습니다.");
            }

            string jsonResult = resultText.Substring(startIndex, endIndex - startIndex + 1);

            // JSON 파싱 및 결과 반환
            var result = JsonSerializer.Deserialize<DefectDetectionPage.AnalysisResult>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result;
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
    }
}