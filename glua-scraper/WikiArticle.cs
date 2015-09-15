using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace glua_scraper
{
    public class WikiArticle
    {
        public static readonly Dictionary<string, string> WikiDictionary = new Dictionary<string, string>
        {
            {@"{{Type\|(.*)}}", "<a href=\"http://wiki.garrysmod.com/page/Category:{0}\">{0}</a>"}
        }; 

        public string Raw { get; set; }

        public WikiArticle(string raw)
        {
            Raw = raw;
        }

        public Hook GetHook(string nameSpace)
        {
            Match m = Regex.Match(Raw, @"{{Hook([\s\S]*?)\n}}");
            if (m.Success)
            {
                Hook hook = new Hook(m.Groups[1].Value, nameSpace);
                List<Arg> args = GetArgs();
                if (args != null)
                {
                    hook.Args = args;
                }
                List<Ret> returnValues = GetReturnValues();
                if (returnValues != null)
                {
                    hook.ReturnValues = returnValues;
                }
                return hook;
            }
            return null;
        }

        public Function GetFunction(string nameSpace)
        {
            Match m = Regex.Match(Raw, @"{{Func([\s\S]*?)\n}}");
            if (m.Success)
            {
                Function func = new Function(m.Groups[1].Value, nameSpace);
                List<Arg> args = GetArgs();
                if (args != null)
                {
                    func.Args = args;
                }
                List<Ret> returnValues = GetReturnValues();
                if (returnValues != null)
                {
                    func.ReturnValues = returnValues;
                }
                return func;
            }
            return null;
        }

        public Function GetPanelFunction(string nameSpace)
        {
            Match m = Regex.Match(Raw, @"{{PanelFunc([\s\S]*?)\n}}");
            if (m.Success)
            {
                Function func = new Function(m.Groups[1].Value, nameSpace);
                List<Arg> args = GetArgs();
                if (args != null)
                {
                    func.Args = args;
                }
                List<Ret> returnValues = GetReturnValues();
                if (returnValues != null)
                {
                    func.ReturnValues = returnValues;
                }
                return func;
            }
            return null;
        }

        public List<Arg> GetArgs()
        {
            MatchCollection matches = Regex.Matches(Raw, @"{{Arg([\s\S]*?)\n}}");
            List<Arg> args = (from Match m in matches where m.Success select new Arg(m.Groups[1].Value)).ToList();

            return args.Count > 0 ? args : null;
        }

        public List<Ret> GetReturnValues()
        {
            MatchCollection matches = Regex.Matches(Raw, @"{{Ret([\s\S]*?)\n}}");
            List<Ret> rets = (from Match m in matches where m.Success select new Ret(m.Groups[1].Value)).ToList();

            return rets.Count > 0 ? rets : null;
        }

        public static string GetValue(string str, string selector)
        {
            Match match = Regex.Match(str, @"\|" + selector + "=(.*)");
            return match.Success ? match.Groups[1].Value : "";
        }

        public static string ParseDescription(string str)
        {
            foreach (string key in WikiDictionary.Keys)
            {
                Match m = Regex.Match(str, key);
                if (m.Success)
                    str = str.Replace(m.Groups[0].Value, string.Format(WikiDictionary[key], m.Groups[1].Value));
            }
            return str;
        }
    }
}