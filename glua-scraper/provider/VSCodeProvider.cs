using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace glua_scraper.provider
{
    public class VSCodeProvider : IProvider
    {
        public string GetName()
        {
            return "VSCode";
        }

        public void OnStart()
        {
            Directory.CreateDirectory(GetName());
        }

        public void OnFinish()
        {
            
        }

        public void SaveHooks(Dictionary<string, List<Hook>> hooks)
        {
            JObject ob = new JObject();

            foreach (string nameSpace in hooks.Keys)
            {
                foreach (Hook hook in hooks[nameSpace])
                {
                    string key = $"{hook.Parent}:{hook.Name}"; // This should probably be fixed somewhere else... oh well.
                    if (ob.GetValue(key) != null)
                        continue;

                    ob.Add(key, new JObject()
                    {
                        {"prefix", hook.Name}, // Until we cam use wild cards we're going to have to pollute global functions :(
                        {"body", BuildHookSnippet(hook)},
                        {"description", $"({(hook.Realm.Contains("Shared") && hook.Realm.Contains("Menu") ? "All" : hook.Realm)}) Method: {key}() " + hook.Description},
                    });
                }
            }

            File.WriteAllText(GetName() + "/hooks.json", ob.ToString(Formatting.Indented));
        }

        public void SaveGlobals(Dictionary<string, List<Function>> globals)
        {
            JObject ob = new JObject();

            foreach (string nameSpace in globals.Keys)
            {
                foreach (Function func in globals[nameSpace])
                {
                    ob.Add("_G." + func.Name, new JObject()
                    {
                        {"prefix", func.Name},
                        {"body", BuildGlobalSnippet(func)},
                        {"description", $"({(func.Realm.Contains("Shared") && func.Realm.Contains("Menu") ? "All" : func.Realm)}) Function: _G.{func.Name}() " + func.Description},
                    });
                }
            }
            File.WriteAllText(GetName() + "/globals.json", ob.ToString(Formatting.Indented));
        }

        public void SaveLibFuncs(Dictionary<string, List<Function>> libFuncs)
        {
            JObject ob = new JObject();

            foreach (string nameSpace in libFuncs.Keys)
            {
                JArray array = new JArray();
                foreach (Function func in libFuncs[nameSpace])
                {
                    string key = func.Parent + "." + func.Name;
                    if (ob.GetValue(key) != null)
                        continue;

                    ob.Add(key, new JObject()
                    {
                        {"prefix", func.Parent + "." + func.Name},
                        {"body", BuildLibSnippet(func)},
                        {"description", $"({(func.Realm.Contains("Shared") && func.Realm.Contains("Menu") ? "All" : func.Realm)}) Function: {func.Parent}.{func.Name}() " + func.Description},
                    });
                }
                ob.Add(nameSpace, array);
            }
            File.WriteAllText(GetName() + "/funcs.json", ob.ToString(Formatting.Indented));
        }

        public void SaveClassFuncs(Dictionary<string, List<Function>> classFuncs)
        {
            SaveGenericFuncs(classFuncs, "classFuncs", "Classfunc");
        }

        public void SavePanelFuncs(Dictionary<string, List<Function>> panelFuncs)
        {
            SaveGenericFuncs(panelFuncs, "panelFuncs", "Panelfunc");
        }

        private void SaveGenericFuncs(Dictionary<string, List<Function>> funcs, string fileName, string funcType)
        {
            JObject ob = new JObject();

            foreach (string nameSpace in funcs.Keys)
            {
                foreach (Function func in funcs[nameSpace])
                {
                    string key = $"{func.Parent}:{func.Name}";
                    if (ob.GetValue(key) != null)
                        continue;

                    if (string.IsNullOrWhiteSpace(func.Realm))
                        func.Realm = "Client";

                    ob.Add(key, new JObject()
                    {
                        {"prefix", func.Name},
                        {"body", BuildGlobalSnippet(func)},
                        {"description", $"({(func.Realm.Contains("Shared") && func.Realm.Contains("Menu") ? "All" : func.Realm)}) Method: {key}() " + func.Description},
                    });
                }
            }

            File.WriteAllText(GetName() +  "/" + fileName + ".json", ob.ToString(Formatting.Indented));
        }

        private string BuildLibSnippet(Function func)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{func.Parent}.{func.Name}(");
            if (func.Args != null && func.Args.Count > 0)
            {
                for (int i = 0; i < func.Args.Count; i++)
                    builder.Append($"${{{i + 1}:{func.Args[i].Type} {func.Args[i].Name}{(func.Args[i].Default != "" ? "=" + func.Args[i].Default : "")}}}, ");
                builder.Length += -2;
            }
            builder.Append(")$0");
            return builder.ToString();
        }

        private string BuildGlobalSnippet(Function func)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{func.Name}(");
            if (func.Args != null && func.Args.Count > 0)
            {
                for (int i = 0; i < func.Args.Count; i++)
                    builder.Append($"${{{i + 1}:{func.Args[i].Type} {func.Args[i].Name}{(func.Args[i].Default != "" ? "=" + func.Args[i].Default : "")}}}, ");
                builder.Length += -2;
            }
            builder.Append(")$0");
            return builder.ToString();
        }

        public string BuildHookSnippet(Hook hook)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{hook.Name}(");
            if (hook.Args != null && hook.Args.Count > 0)
            {
                for (int i = 0; i < hook.Args.Count; i++)
                    builder.Append($"${{{i + 1}:{hook.Args[i].Type} {hook.Args[i].Name}{(hook.Args[i].Default != "" ? "=" + hook.Args[i].Default : "")}}}, ");
                builder.Length += -2;
            }
            builder.AppendLine(")$0");
            return builder.ToString();
        }
    }
}