using HtmlAgilityPack;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace semes
{
    public partial class IndustryNewsDetailPage : Page
    {
        public IndustryNewsDetailPage(string title, string url)
        {
            InitializeComponent();
            HeaderTitle.Text = title;
            LoadPageWithHtmlAgilityPack(title, url);
        }

        private async void LoadPageWithHtmlAgilityPack(string fallbackTitle, string url)
        {
            await Task.Run(async () =>
            {
                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

                    var html = await client.GetStringAsync(url);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    string title = doc.DocumentNode
                        ?.SelectSingleNode("//h2[contains(@class,'headline')]")
                        ?.InnerText?.Trim() ?? fallbackTitle;

                    string time = doc.DocumentNode
                        ?.SelectSingleNode("//span[contains(@class,'datestamp_time')]")
                        ?.InnerText?.Trim() ?? "";

                    var bodyNode = doc.DocumentNode.SelectSingleNode("//*[@id='dic_area']");

                    string bodyText = "";
                    string firstImage = "";

                    if (bodyNode != null)
                    {
                        foreach (var node in bodyNode.ChildNodes)
                        {
                            if (node.Name == "p" || node.Name == "br")
                            {
                                bodyText += "\n";
                            }
                            else if (node.Name == "img" && string.IsNullOrEmpty(firstImage))
                            {
                                firstImage = node.GetAttributeValue("src", "")
                                            ?? node.GetAttributeValue("data-src", "")
                                            ?? node.GetAttributeValue("data-original", "");
                            }
                            else
                            {
                                bodyText += node.InnerText.Trim();
                            }
                        }

                        bodyText = HtmlEntity.DeEntitize(bodyText).Trim();
                    }

                    // 🧠 fallback: og:image
                    if (string.IsNullOrWhiteSpace(firstImage))
                    {
                        var og = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                        if (og != null)
                            firstImage = og.GetAttributeValue("content", "");
                    }

                    // 🧠 fallback: 기본 이미지
                    if (string.IsNullOrWhiteSpace(firstImage))
                    {
                        firstImage = "https://via.placeholder.com/800x400.png?text=No+Image";
                    }

                    // 절대 경로 보정
                    if (firstImage.StartsWith("//"))
                        firstImage = "https:" + firstImage;
                    else if (firstImage.StartsWith("/"))
                    {
                        var baseUri = new Uri(url);
                        firstImage = baseUri.Scheme + "://" + baseUri.Host + firstImage;
                    }

                    // 디버깅 로그 (원하면 주석 해제)
                    // Console.WriteLine("▶ 대표 이미지 URL: " + firstImage);

                    Dispatcher.Invoke(() =>
                    {
                        HeaderTitle.Text = title;
                        NewsSubTitle.Text = time;
                        NewsBody.Text = bodyText;

                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(firstImage, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            // 실패 감지 (디버깅용)
                            bitmap.DownloadFailed += (s, e) =>
                            {
                                MessageBox.Show("❌ 이미지 다운로드 실패: " + e.ErrorException?.Message);
                                MainImage.Visibility = Visibility.Collapsed;
                            };

                            MainImage.Source = bitmap;
                            MainImage.Visibility = Visibility.Visible;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("이미지 로딩 오류: " + ex.Message);
                            MainImage.Visibility = Visibility.Collapsed;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        NewsBody.Text = $"[본문 로딩 실패] {ex.Message}";
                        MainImage.Visibility = Visibility.Collapsed;
                    });
                }
            });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}
