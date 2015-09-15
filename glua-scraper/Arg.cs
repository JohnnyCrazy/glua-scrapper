namespace glua_scraper
{
    public class Arg
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Default { get; set; }

        public Arg(string raw)
        {
            Type = WikiArticle.GetValue(raw, "type");
            Name = WikiArticle.GetValue(raw, "name");
            Desc = WikiArticle.GetValue(raw, "desc");
            Default = WikiArticle.GetValue(raw, "default");
        }
    }
}