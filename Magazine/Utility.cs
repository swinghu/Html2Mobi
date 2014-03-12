using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Magazine
{
    public class Utility
    {
        public static string GetHtml(Uri uri)
        {
            var req = ((HttpWebRequest)(WebRequest.Create(uri)));
            string htmlContent;
            using (var wr = req.GetResponse())
            {
                var response = wr.GetResponseStream();
                Debug.Assert(response != null, "htmlresponse != null");
                var sr = new StreamReader(response, Encoding.UTF8);
                htmlContent = sr.ReadToEnd();
            }
            return htmlContent;
        }

        public static string GetImageExtension(string imageUrl)
        {
            var ext = Path.GetExtension(imageUrl);
            if (ext == null || ext.Length > 5)
                return String.Empty;
            return Path.GetExtension(imageUrl);
        }

        public static string DownloadImage(string url, string filePath)
        {
            var wc = new WebClient();
            wc.DownloadFile(url, filePath);
            return filePath;
        }

        public static List<string> GetAllImagesFromUrl(string rawHtml)
        {
            const string regExPattern = @"< \s* img [^\>]* src \s* = \s* [\""\']? ( [^\""\'\s>]* )";
            var r = new Regex(regExPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            var matches = r.Matches(rawHtml);
            return (from Match m in matches select m.Groups[1].Value).ToList();
        }

        public static string GetValidFileName(string rawFileName)
        {
            var fileName = new StringBuilder(rawFileName);
            var invalidFileName = new[] { '?', '\\', '*', '"', '<', '>', '|', '/', '.',':' };
            foreach (var chr in invalidFileName)
            {
                fileName.Replace(chr, '_');
            }
            return fileName.ToString();
        }

        public static string CreateTableOfContent(IEnumerable<Article> articles)
        {
            var toc = new XElement("html",
                                new XElement("head",
                                        new XElement("Title", "Table Of Contents")
                                ),
                                new XElement("body",
                                     new XElement("div",
                                        new XElement("h1",
                                            new XElement("b", "TABLE OF CONTENTS")
                                ))));
            var body = toc.Element("body");
            Debug.Assert(body != null, "body != null");
            var div = body.Element("div");
            var articlesInGroup = from a in articles
                                  group a by a.Category into g
                                  orderby g.Key
                                  select g;

            foreach (var c in articlesInGroup)
            {
                var category = new XElement("b",c.Key);
                var ul=new XElement("ul");
                foreach (var article in c)
                {
                    ul.Add(new XElement("li",
                                new XElement("a", new XAttribute("href", article.OutFileName), article.Title)));

                }
                Debug.Assert(div != null, "div != null");
                div.Add(category,ul);
            }
            return toc.ToString();
        }

        public static string CreateOpf(IEnumerable<Article> articles, string title)
        {
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            var odf = new XElement("package", new XAttribute(XNamespace.Xmlns + "dc", "http://purl.org/dc/elements/1.1/"),
                                   new XElement("metadata"),
                                   new XElement("manifest"),
                                   new XElement("spine"),
                                   new XElement("guide"));
            var metadata = odf.Element("metadata");
            Debug.Assert(metadata != null, "metadata != null");
            metadata.Add(new XElement(dc + "title", title));
            metadata.Add(new XElement(dc + "language", "en-us"));
            metadata.Add(new XElement(dc + "date", DateTime.Now.ToShortDateString()));
            var manifeast = odf.Element("manifest");
            Debug.Assert(manifeast != null, "manifeast != null");
            manifeast.Add(new XElement("item", new XAttribute("id", "item1"), new XAttribute("media-type", "application/xhtml+xml"), new XAttribute("href", "toc.html")));

            var spine = odf.Element("spine");
            Debug.Assert(spine != null, "spine != null");
            spine.Add(new XElement("itemref", new XAttribute("idref", "item1")));
            var guide = odf.Element("guide");
            Debug.Assert(guide != null, "guide != null");
            guide.Add(new XElement("reference", new XAttribute("type", "toc"), new XAttribute("title", "able of Contents"), new XAttribute("href", "toc.html")
                                     ));
            var num = 2;
            foreach (var article in articles.OrderBy(a=>a.Category))
            {
                manifeast.Add(new XElement("item", new XAttribute("id", String.Format("item{0}", num)), new XAttribute("media-type", "application/xhtml+xml"), new XAttribute("href", article.OutFileName)));
                spine.Add(new XElement("itemref", new XAttribute("idref", String.Format("item{0}", num))));
                num++;
            }
            return odf.ToString();
        }
    }
}
