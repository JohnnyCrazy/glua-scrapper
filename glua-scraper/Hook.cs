using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace glua_scraper
{
    public class Hook
    {
        public static readonly Dictionary<string, string> HookDic = new Dictionary<string, string>()
            {
                {"WEAPON", "SWEP"},
                {"TOOL", "TOOL"},
                {"SANDBOX", "SANDBOX"},
                {"PLAYER", "PLAYER"},
                {"PANEL", "PANEL"},
                {"NEXTBOT", "NEXTBOT"},
                {"GM", "GM"},
                {"ENTITY", "ENT"},
                {"EFFECT", "EFFECT"},
            };

        public string Name { get; set; }
        public string Parent { get; set; }
        public string Description { get; set; }
        public string Realm { get; set; }
        public string DescriptionUrl { get; set; }

        public List<Arg> Args { get; set; }

        public List<Ret> ReturnValues { get; set; }

        public Hook(string raw, string nameSpace)
        {
            Name = WikiArticle.GetValue(raw, "Name");
            Parent = HookDic.Keys.Contains(WikiArticle.GetValue(raw, "Parent")) ? HookDic[WikiArticle.GetValue(raw, "Parent")] : HookDic[nameSpace];
            Description = WikiArticle.ParseDescription(WikiArticle.GetValue(raw, "Description"));
            Realm = WikiArticle.GetValue(raw, "Realm");
        }

        public static Hook ProcessHook(string nameSpace, string url)
        {
            using (WebClient wc = new WebClient())
            {
                WikiArticle article = new WikiArticle(wc.DownloadString(url + "?action=raw"));
                Hook hook = article.GetHook(nameSpace);
                if (hook == null)
                    return null;
                hook.DescriptionUrl = url;
                return hook;
            }
        }
    }
}