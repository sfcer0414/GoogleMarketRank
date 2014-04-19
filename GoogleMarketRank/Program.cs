using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMarketRank
{
    public class AppInfo
    {
        public string Name;
        public string Url;
        public string DevSite;
        public string Email;

        public AppInfo(string name, string url)
        {
            this.Name = name;
            this.Url = url;
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            //下載APP詳細頁面
            string ret = wc.DownloadString(Url);
            //載入HAP
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(ret);
            var nav = doc.CreateNavigator();
            //使用XPATH取得每個聯絡資訊連結
            var apps = nav.Select("//a[@class='dev-link']");
            foreach (System.Xml.XPath.XPathNavigator node in apps)
            {
                //從資料面發現不一定都會有下列的連結，所以要判斷有存在，才能抓取該連結並且放到對應的成員變數中。
                if (node.Value.Contains("造訪開發人員的網站"))
                    DevSite =  node.GetAttribute("href", "");
                else if (node.Value.Contains("傳送電子郵件給開發人員"))
                    Email = node.GetAttribute("href", "");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //因為走HTTPS，所以要先設定接受憑證。
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            //下載分類排行榜頁面
            string ret = wc.DownloadString("https://play.google.com/store/apps/category/HEALTH_AND_FITNESS/collection/topselling_paid");
            //載入HAP
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(ret);
            var nav = doc.CreateNavigator();
            //使用XPATH取得每個App的連結
            var apps = nav.Select("//div[@class='card-content id-track-click id-track-impression']//a[@class='title']");

            List<AppInfo> appInfos = new List<AppInfo>();
            foreach (System.Xml.XPath.XPathNavigator node in apps)
            {
                string name = node.Value;
                string url = "https://play.google.com" + node.GetAttribute("href", "");
                string appDetail = wc.DownloadString(url);
                AppInfo appInfo = new AppInfo(name, url);
                appInfos.Add(appInfo);
            }
        }
    }
}
