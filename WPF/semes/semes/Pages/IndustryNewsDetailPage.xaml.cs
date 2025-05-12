using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

namespace semes
{
    public partial class IndustryNewsDetailPage : Page
    {
        public IndustryNewsDetailPage(string title, string url)
        {
            InitializeComponent();
            NewsTitle.Text = title;
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
                    try
                    {
                        parsedTitle = driver.FindElement(By.CssSelector("h2.media_end_head_headline")).Text;
                    }
                    catch { }

                    string subtitle = "";
                    try
                    {
                        subtitle = driver.FindElement(By.CssSelector("span.media_end_head_info_datestamp_time")).Text;
                    }
                    catch { }

                    string body = "";
                    try
                    {
                        body = driver.FindElement(By.CssSelector("#dic_area")).Text;
                    }
                    catch { }

                    var imageElements = driver.FindElements(By.CssSelector("#dic_area img"));

                    Dispatcher.Invoke(() =>
                    {
                        if (!string.IsNullOrWhiteSpace(parsedTitle))
                            NewsTitle.Text = parsedTitle;

                        NewsSubTitle.Text = subtitle;
                        NewsBody.Text = body;

                        // 대표 이미지 1장만 표시
                        if (imageElements.Count > 0)
                        {
                            var src = imageElements[0].GetAttribute("src");
                            if (src.StartsWith("//")) src = "https:" + src;

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
    }
}
