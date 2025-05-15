using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
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
            HeaderTitle.Text = title;     // fallback 텍스트 우선 표시

            LoadPageWithSelenium(title, url);
        }

        private async void LoadPageWithSelenium(string fallbackTitle, string url)
        {
            await Task.Run(() =>
            {
                var options = new ChromeOptions();
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");

                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                using var driver = new ChromeDriver(service, options);
                try
                {
                    driver.Navigate().GoToUrl(url);
                    Task.Delay(1500).Wait(); // 페이지 로딩 대기

                    string parsedTitle = "";
                    string subtitle = "";
                    string body = "";

                    try
                    {
                        parsedTitle = driver.FindElement(By.CssSelector("h2.media_end_head_headline")).Text;
                    }
                    catch { }

                    try
                    {
                        subtitle = driver.FindElement(By.CssSelector("span.media_end_head_info_datestamp_time")).Text;
                    }
                    catch { }

                    try
                    {
                        body = driver.FindElement(By.CssSelector("#dic_area")).Text;
                    }
                    catch { }

                    var imageElements = driver.FindElements(By.CssSelector("#dic_area img"));

                    Dispatcher.Invoke(() =>
                    {
                        // 제목 설정
                        if (!string.IsNullOrWhiteSpace(parsedTitle))
                        {
                            HeaderTitle.Text = parsedTitle;
                    
                        }
                        else
                        {
                            HeaderTitle.Text = fallbackTitle;
                            
                        }

                        // 날짜 및 본문
                        NewsSubTitle.Text = subtitle;
                        NewsBody.Text = body;

                        // 이미지 (대표 1장)
                        if (imageElements.Count > 0)
                        {
                            var src = imageElements[0].GetAttribute("src");
                            if (src.StartsWith("//"))
                                src = "https:" + src;

                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(src);
                            bitmap.EndInit();

                            MainImage.Source = bitmap;
                            MainImage.Visibility = Visibility.Visible;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        NewsBody.Text = $"[본문 로딩 실패] {ex.Message}";
                    });
                }
            });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private void PrevNews_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("이전 뉴스로 이동 (로직 구현 필요)");
        }

        private void NextNews_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("다음 뉴스로 이동 (로직 구현 필요)");
        }
    }
}
