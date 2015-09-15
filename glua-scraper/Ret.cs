namespace glua_scraper
{
    public class Ret
    {
        public string Type { get; set; }

        public Ret(string raw)
        {
            Type = WikiArticle.GetValue(raw, "type");
        }
    }
}