using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace semes
{
    public partial class IndustryNewsPage : Page
    {
        public class NewsItem
        {
            public string Title { get; set; }
            public string Link { get; set; }
            public string Thumbnail { get; set; }
            public string Description { get; set; }
            public string Source { get; set; }
            public string Time { get; set; }
        }

        private ObservableCollection<NewsItem> newsList = new ObservableCollection<NewsItem>();

        public IndustryNewsPage()
        {
            InitializeComponent();

            // 테스트 데이터로 UI 확인
            newsList.Add(new NewsItem
            {
                Title = "반도체 산업 최신 동향: 삼성전자, 새로운 공정 기술 발표",
                Description = "삼성전자가 차세대 반도체 공정 기술을 발표했습니다. 이번 기술은 기존 대비 전력 소모량을 30% 줄이고 성능은 25% 향상시킨 것으로...",
                Thumbnail = "https://via.placeholder.com/120x90.png?text=News",
                Source = "전자신문",
                Time = "3시간 전",
                Link = "https://example.com/news1"
            });

            NewsListView.ItemsSource = newsList;

            // 실제 뉴스 로드
            LoadNews();
        }

        private async void LoadNews()
        {
            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("X-Naver-Client-Id", "8yVAIufnQS_mKGlEgmjx");
                client.DefaultRequestHeaders.Add("X-Naver-Client-Secret", "gUcQlDP9I4");

                string query = WebUtility.UrlEncode("반도체");
                string url = $"https://openapi.naver.com/v1/search/news.json?query={query}&display=10&sort=sim";

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(json);
                var items = doc.RootElement.GetProperty("items");

                // 테스트 데이터 제거
                Dispatcher.Invoke(() => newsList.Clear());

                foreach (var item in items.EnumerateArray())
                {
                    string rawTitle = item.GetProperty("title").GetString();
                    string rawDesc = item.GetProperty("description").GetString();

                    string title = CleanHtml(WebUtility.HtmlDecode(rawTitle));
                    string description = CleanHtml(WebUtility.HtmlDecode(rawDesc));
                    string link = item.GetProperty("link").GetString();

                    // 출처 추출
                    string source = "뉴스";
                    if (item.TryGetProperty("originallink", out var linkElement))
                    {
                        string originalLink = linkElement.GetString();
                        try
                        {
                            Uri uri = new Uri(originalLink);
                            source = uri.Host.Replace("www.", "");

                            // 도메인에서 .com, .co.kr 등 제거
                            int dotIndex = source.IndexOf('.');
                            if (dotIndex > 0)
                            {
                                source = source.Substring(0, dotIndex);
                            }
                        }
                        catch { }
                    }

                    // 날짜 처리
                    string pubDate = "방금 전";
                    if (item.TryGetProperty("pubDate", out var pubDateElement))
                    {
                        string dateStr = pubDateElement.GetString();
                        DateTime dt;
                        if (DateTime.TryParse(dateStr, out dt))
                        {
                            TimeSpan diff = DateTime.Now - dt;
                            if (diff.TotalDays >= 1)
                                pubDate = $"{(int)diff.TotalDays}일 전";
                            else if (diff.TotalHours >= 1)
                                pubDate = $"{(int)diff.TotalHours}시간 전";
                            else if (diff.TotalMinutes >= 1)
                                pubDate = $"{(int)diff.TotalMinutes}분 전";
                        }
                    }

                    // 기본 썸네일 이미지
                    string thumbnail = "https://via.placeholder.com/120x90.png?text=News";

                    var newsItem = new NewsItem
                    {
                        Title = title,
                        Link = link,
                        Thumbnail = thumbnail,
                        Description = description,
                        Source = source,
                        Time = pubDate
                    };

                    Dispatcher.Invoke(() => newsList.Add(newsItem));
                }

                // 썸네일 이미지를 비동기적으로 가져오기
                _ = LoadThumbnailsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("뉴스를 불러오는 데 실패했습니다: " + ex.Message);
            }
        }

        // 썸네일 이미지를 비동기적으로 가져오는 메서드
        private async Task LoadThumbnailsAsync()
        {
            var tasks = new List<Task>();

            // 최대 동시 요청 수 제한 (예: 4개)
            SemaphoreSlim semaphore = new SemaphoreSlim(4);

            for (int i = 0; i < newsList.Count; i++)
            {
                await semaphore.WaitAsync();

                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var item = newsList[index];
                        string thumbnail = await FetchOgImageAsync(item.Link);
                        if (!string.IsNullOrEmpty(thumbnail))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                item.Thumbnail = thumbnail;
                                var temp = newsList[index];
                                newsList[index] = null;
                                newsList[index] = temp;
                            });
                        }
                    }
                    catch { }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        // HTML 태그 제거
        private static string CleanHtml(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        // 뉴스 링크 HTML에서 og:image 추출
        private async Task<string> FetchOgImageAsync(string url)
        {
            try
            {
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5); // 타임아웃 설정

                var html = await client.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var meta = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                if (meta != null)
                {
                    string imageUrl = meta.GetAttributeValue("content", "");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        if (imageUrl.StartsWith("//"))
                            imageUrl = "https:" + imageUrl;
                        return imageUrl;
                    }
                }
            }
            catch { }

            return "";
        }

        // 뉴스 제목 클릭 시 링크 열기
        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is NewsItem item)
            {
                try
                {
                    // 제목과 링크를 함께 넘겨서 본문 파싱 후 렌더링
                    var detailPage = new IndustryNewsDetailPage(item.Title, item.Link);
                    NavigationService?.Navigate(detailPage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("뉴스 본문을 열 수 없습니다: " + ex.Message);
                }
            }
        }
    }
}
