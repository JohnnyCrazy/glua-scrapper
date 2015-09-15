using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace glua_scraper
{
    public class Function
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public string Description { get; set; }
        public string DescriptionUrl { get; set; }
        public string Realm { get; set; }
        public bool IsClass { get; set; }

        public List<Arg> Args { get; set; }

        public List<Ret> ReturnValues { get; set; }

        public Function(string raw, string nameSpace)
        {
            Name = WikiArticle.GetValue(raw, "Name");
            Parent = WikiArticle.GetValue(raw, "Parent") != "" ? WikiArticle.GetValue(raw, "Parent") : nameSpace;
            Description = WikiArticle.ParseDescription(WikiArticle.GetValue(raw, "Description"));
            Realm = WikiArticle.GetValue(raw, "Realm");
            IsClass = WikiArticle.GetValue("raw", "IsClass") == "Yes";
        }

        public override string ToString()
        {
            return Name;
        }

        public static Function ProccessFunction(string nameSpace, string url)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    WikiArticle article = new WikiArticle(wc.DownloadString(url + "?action=raw"));
                    Function func = article.GetFunction(nameSpace);
                    if (func == null)
                        return null;
                    if (func.Name == "")
                        func.Name = url.Split('/').Last();
                    func.DescriptionUrl = url;
                    return func;
                }
                catch (WebException e)
                {
                    Console.WriteLine($"INVALID: {nameSpace}:{url}  {e.Message}");
                    return null;
                }
            }
        }

        public static Function ProccessPanelFunction(string nameSpace, string url)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    WikiArticle article = new WikiArticle(wc.DownloadString(url + "?action=raw"));
                    Function func = article.GetPanelFunction(nameSpace);
                    if (func == null)
                        return null;
                    if (func.Name == "")
                        func.Name = url.Split('/').Last();
                    func.DescriptionUrl = url;
                    return func;
                }
                catch (WebException e)
                {
                    Console.WriteLine($"INVALID: {nameSpace}:{url}  {e.Message}");
                    return null;
                }
            }
        }
    }
}