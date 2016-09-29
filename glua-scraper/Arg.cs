using System.Text.RegularExpressions;

namespace glua_scraper
{
    public class Arg
    {
        public Arg(string raw)
        {
            Type = WikiArticle.GetValue(raw, "type");
            Name = WikiArticle.GetValue(raw, "name");
            Desc = WikiArticle.GetValue(raw, "desc");
            Default = ConvertDefaultValue(WikiArticle.GetValue(raw, "default"));
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Default { get; set; }

        private string ConvertDefaultValue(string defaultValue)
        {
            Match m = Regex.Match(defaultValue, @"\{\{(.*)\|(.*)\}\}\(\)");
            if (m.Success)
            {
                switch (m.Groups[1].Value)
                {
                    case "GlobalFunction":
                        return $"{m.Groups[2].Value}()";
                    // TODO Add more cases if necessary
                    default:
                        return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}