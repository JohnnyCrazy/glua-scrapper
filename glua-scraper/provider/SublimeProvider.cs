using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace glua_scraper.provider
{
    public class SublimeProvider : IProvider
    {
        private JObject ob;

        public string GetName()
        {
            return "SublimeText";
        }

        public void OnStart()
        {
            ob = new JObject
            {
                {"scope", "source.lua - keyword.control.lua - constant.language.lua - string"},
                {"completions", new JArray("in", "else", "return", "false", "true", "break", "or", "and")}
            };

        }

        public void OnFinish()
        {
            Directory.CreateDirectory(GetName());
            File.WriteAllText($"{GetName()}/lua.sublime-completionsPretty", ob.ToString(Formatting.Indented));
            File.WriteAllText($"{GetName()}/lua.sublime-completions", ob.ToString(Formatting.None));
        }

        public void SaveHooks(Dictionary<string, List<Hook>> hooks)
        {
            JArray arr = ob["completions"] as JArray;
            foreach (string nameSpace in hooks.Keys)
            {
                foreach (Hook hook in hooks[nameSpace])
                {
                    if(hook.Name != "")
                        arr?.Add(hook.Name);
                }
            }
        }

        public void SaveGlobals(Dictionary<string, List<Function>> globals)
        {
            JArray arr = ob["completions"] as JArray;
            foreach (string nameSpace in globals.Keys)
            {
                foreach (Function func in globals[nameSpace])
                {
                    arr?.Add(new JObject
                    {
                        {"trigger", func.Name},
                        {"contents", BuildGlobalSnippet(func)}
                    });
                }
            }
        }

        public void SaveLibFuncs(Dictionary<string, List<Function>> libFuncs)
        {
            JArray arr = ob["completions"] as JArray;
            foreach (string nameSpace in libFuncs.Keys)
            {
                foreach (Function func in libFuncs[nameSpace])
                {
                    arr?.Add(new JObject
                    {
                        {"trigger", $"{func.Parent}.{func.Name}"},
                        {"contents", BuildLibSnippet(func)}
                    });
                }
            }
        }

        public void SaveClassFuncs(Dictionary<string, List<Function>> classFuncs)
        {
            JArray arr = ob["completions"] as JArray;
            foreach (string nameSpace in classFuncs.Keys)
            {
                foreach (Function func in classFuncs[nameSpace])
                {
                    arr?.Add(new JObject
                    {
                        {"trigger", $"{func.Name}"},
                        {"contents", BuildGlobalSnippet(func)}
                    });
                }
            }
        }

        public void SavePanelFuncs(Dictionary<string, List<Function>> panelFuncs)
        {
            JArray arr = ob["completions"] as JArray;
            foreach (string nameSpace in panelFuncs.Keys)
            {
                foreach (Function func in panelFuncs[nameSpace])
                {
                    arr?.Add(new JObject
                    {
                        {"trigger", $"{func.Name}"},
                        {"contents", BuildGlobalSnippet(func)}
                    });
                }
            }
        }

        private string BuildLibSnippet(Function func)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{func.Parent}.{func.Name}(");
            if (func.Args != null && func.Args.Count > 0)
            {
                for (int i = 0; i < func.Args.Count; i++)
                    builder.Append($"${{{i + 1}:{func.Args[i].Type} {func.Args[i].Name}{(func.Args[i].Default != "" ? "=" + func.Args[i].Default : "")}}},");
                builder.Length--;
            }
            builder.Append($")");
            //builder.Append($")${{{Args?.Count + 1 ?? 1}:}}");
            return builder.ToString();
        }

        private string BuildGlobalSnippet(Function func)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{func.Name}(");
            if (func.Args != null && func.Args.Count > 0)
            {
                for (int i = 0; i < func.Args.Count; i++)
                    builder.Append($"${{{i + 1}:{func.Args[i].Type} {func.Args[i].Name}{(func.Args[i].Default != "" ? "=" + func.Args[i].Default : "")}}},");
                builder.Length--;
            }
            builder.Append($")");
            //builder.Append($")${{{Args?.Count + 1 ?? 1}:}}");
            return builder.ToString();
        }
    }
}
