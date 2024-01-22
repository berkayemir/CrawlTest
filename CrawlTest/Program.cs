using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        await RunAsync();
    }

    static async Task RunAsync()
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("https://tr.linkedin.com/");

        string kullaniciAdi = "brky_emr92@windowslive.com";
        string sifre = "Gökçe22101992";

        IWebElement kullaniciAdiAlan = driver.FindElement(By.Id("session_key"));
        kullaniciAdiAlan.SendKeys(kullaniciAdi);

        IWebElement sifreAlan = driver.FindElement(By.Name("session_password"));
        sifreAlan.SendKeys(sifre);

        IWebElement girisYapButton = driver.FindElement(By.CssSelector("button[type='submit']"));
        girisYapButton.Click();

        await Task.Delay(30000);
        IWebElement element = driver.FindElement(By.ClassName("identity-headline"));

        Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
        Bitmap fullScreen = new Bitmap(new MemoryStream(screenshot.AsByteArray));

        Rectangle region = new Rectangle(element.Location.X, element.Location.Y, element.Size.Width, element.Size.Height);
        Bitmap regionScreenshot = fullScreen.Clone(region, fullScreen.PixelFormat);

        string screenshotPath = "C:\\Images\\Screenshot.png";
        regionScreenshot.Save(screenshotPath);

        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = new TimeSpan(0, 1, 0);

        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent("K85123235688957"), "apikey");
        //form.Add(new StringContent("tur"),"language");

        form.Add(new StringContent("2"), "ocrengine");
        form.Add(new StringContent("true"), "scale");
        form.Add(new StringContent("true"), "istable");

        byte[] imageData = File.ReadAllBytes(screenshotPath);
        form.Add(new ByteArrayContent(imageData, 0, imageData.Length), "image", "image.jpg");

        HttpResponseMessage response = await httpClient.PostAsync("https://api.ocr.space/Parse/Image", form);

        string strContent = await response.Content.ReadAsStringAsync();

        var text = JsonConvert.DeserializeObject<OCRSpaceResponse>(strContent);

        foreach (var parsedResult in text.ParsedResults)
        {
            foreach (var textOverlay in parsedResult.TextOverlay.Lines)
            {
                foreach (var line in textOverlay.Words)
                {
                    Console.WriteLine(line.WordText);
                }
            }
        }

        Console.ReadLine();

        driver.Quit();
    }
    public class OCRSpaceResponse
    {
        public List<OCRSpaceParsedResult> ParsedResults { get; set; }
    }

    public class OCRSpaceParsedResult
    {
        public OCRSpaceTextOverlay TextOverlay { get; set; }
    }

    public class OCRSpaceTextOverlay
    {
        public List<OCRSpaceLine> Lines { get; set; }
    }

    public class OCRSpaceLine
    {
        public List<OCRSpaceWord> Words { get; set; }
    }

    public class OCRSpaceWord
    {
        public string WordText { get; set; }
    }
}
