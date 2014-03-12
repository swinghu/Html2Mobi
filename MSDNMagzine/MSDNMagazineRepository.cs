using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using HtmlAgilityPack;
using Magazine;

namespace MSDNMagzine
{
    public class MsdnMagazineRepository : MsMagazineRepository
    {
        private const string Url = "http://msdn.microsoft.com/magazine/rss/default.aspx?z=z&iss=1";

        public MsdnMagazineRepository(string outputfolder)
            : base(outputfolder)
        {
            Issue = string.Format("MSDN Magazine {0} {1}", DateTime.Now.Month, DateTime.Now.Year);
            OutPutFolder = Path.Combine(outputfolder, Utility.GetValidFileName(Issue));
        }

        public override IEnumerable<Article> GetArticles()
        {
            // read articles list from RSS
            var rawHtml = Utility.GetHtml(new Uri(Url));
            var rss = new XmlDocument();
            rss.LoadXml(rawHtml);
            var articles = new List<MsdnArticle>();
            var items = rss.SelectNodes("//item");
            Debug.Assert(items != null, "items != null");
            foreach (XmlNode item in items)
            {
                var title = item.SelectSingleNode("title");
                var link = item.SelectSingleNode("link");
                Debug.Assert(link != null, "link != null");
                Debug.Assert(title != null, "title != null");
                var indexOfSplitChar = title.InnerText.IndexOf(':');
                var category = title.InnerText.Substring(0,indexOfSplitChar).Trim();
                var articleTitle = title.InnerText.Substring(indexOfSplitChar+1).Trim();
                articles.Add(new MsdnArticle(OutPutFolder, new Uri(link.InnerText), articleTitle, category));
            }
            return articles;
        }
    }

    public class MsdnArticle : Article
    {
        public MsdnArticle(string issueFolder, Uri articleUrl, string title, string category)
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
