using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using HtmlAgilityPack;
using Magazine;

namespace TechnetMagazine
{
    public class TechnetMagazineRepository : MsMagazineRepository
    {
       private const string Url = "http://technet.microsoft.com/en-us/magazine/rss/default.aspx?issue=tue";

        public TechnetMagazineRepository(string outputfolder)
           : base(outputfolder)
       {
           Issue = string.Format("Technet Magazine {0} {1}", DateTime.Now.Month, DateTime.Now.Year);
           OutPutFolder = Path.Combine(outputfolder, Utility.GetValidFileName(Issue));
       }

        public override IEnumerable<Article> GetArticles()
        {
            // read articles list from RSS
            var rawHtml = Utility.GetHtml(new Uri(Url));
            var rss = new XmlDocument();
            rss.LoadXml(rawHtml);
            var articles = new List<TechnetArticle>();
            var items = rss.SelectNodes("//item");
            Debug.Assert(items != null, "items != null");
            foreach (XmlNode item in items)
            {
                var title = item.SelectSingleNode("title");
                var link = item.SelectSingleNode("link");
                Debug.Assert(link != null, "link != null");
                Debug.Assert(title != null, "title != null");
                var indexOfSplitChar = title.InnerText.IndexOf(':');
                var category = title.InnerText.Substring(0, indexOfSplitChar).Trim();
                var articleTitle = title.InnerText.Substring(indexOfSplitChar+1).Trim();
                articles.Add(new TechnetArticle(OutPutFolder, new Uri(link.InnerText), articleTitle, category));
            }
            return articles;
        }
    }

    public class TechnetArticle : Article
    {
        public TechnetArticle(string issueFolder, Uri articleUrl, string title, string category)
            : base(issueFolder, articleUrl, title, category)
        { }

        public override string GetArticleContent(string rawHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);
            var mainContent = doc.DocumentNode.SelectSingleNode("//div[@id='MainContent']");

            var html = string.Format(@"<HTML><BODY>{0}</BODY></HTML>", mainContent.OuterHtml);
            return html;
        }
    }
}
