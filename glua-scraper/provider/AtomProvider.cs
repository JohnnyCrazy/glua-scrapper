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
    public class AtomProvider : IProvider
    {
        public string GetName()
        {
            return "AtomIO";
        }

        public void OnStart()
        {
            
        }

        public void OnFinish()
        {
            
        }

        public void SaveHooks(Dictionary<string, List<Hook>> hooks)
        {
            JObject ob = new JObject();

            foreach (string nameSpace in hooks.Keys)
            {
                JArray array = new JArray();
                foreach (Hook hook in hooks[nameSpace])
                {
                    array.Add(new JObject()
                    {
                        {"name", hook.Name},
                        {"displayText", $"{hook.Parent}:{hook.Name}"},
                        {"snippet", BuildHookSnippet(hook)},
                        {"leftLabel", string.Join(",", hook.ReturnValues?.Select(ret => ret.Type) ?? new[] {"func"})},
                        {"rightLabel", $"{hook.Realm}-Hook"},
                        {"type", "property"},
                        {"desc", hook.Description},
                        {"descUrl", hook.DescriptionUrl}
                    });
                }
                ob.Add(Hook.HookDic[nameSpace], array);
            }
            File.WriteAllText("hooks.json", ob.ToString(Formatting.None));
            File.WriteAllText("hooksPretty.json", ob.ToString(Formatting.Indented));
        }

        public void SaveGlobals(Dictionary<string, List<Function>> globals)
        {
            JObject ob = new JObject();

            foreach (string nameSpace in globals.Keys)
            {
                foreach (Function func in globals[nameSpace])
                {
                    ob.Add(func.Name, new JObject()
                    {
                        {"name", func.Name},
                        {"displayText", $"{func.Name}"},
                        {"snippet", BuildGlobalSnippet(func)},
                        {"leftLabel", string.Join(",", func.ReturnValues?.Select(ret => ret.Type) ?? new[] {"func"})},
                        {"rightLabel", $"{func.Realm}-Global"},
                        {"type", "function"},
                        {"desc", func.Description},
                        {"descUrl", func.DescriptionUrl}
                    });
                }
            }
            File.WriteAllText("globals.json", ob.ToString(Formatting.None));
            File.WriteAllText("globalsPretty.json", ob.ToString(Formatting.Indented));
        }

        public void SaveLibFuncs(Dictionary<string, List<Function>> libFuncs)
        {
            JObject ob = new JObject();

            foreach (string nameSpace in libFuncs.Keys)
            {
                JArray array = new JArray();
                foreach (Function func in libFuncs[nameSpace])
                {
                    array.Add(new JObject()
                    {
                        {"name", func.Name},
                        {"displayText", $"{func.Parent}.{func.Name}"},
                        {"snippet", BuildLibSnippet(func)},
                        {"leftLabel", string.Join(",", func.ReturnValues?.Select(ret => ret.Type) ?? new[] {"func"})},
                        {"rightLabel", $"{func.Realm}-Libfunc"},
                        {"type", "function"},
                        {"desc", func.Description},
                        {"descUrl", func.DescriptionUrl}
                    });
                }
                ob.Add(nameSpace, array);
            }
            File.WriteAllText("funcs.json", ob.ToString(Formatting.None));
            File.WriteAllText("funcsPretty.json", ob.ToString(Formatting.Indented));
        }

        public void SaveClassFuncs(Dictionary<string, List<Function>> classFuncs)
        {
            SaveGenericFuncs(classFuncs, "classFuncs");
        }

        public void SavePanelFuncs(Dictionary<string, List<Function>> panelFuncs)
        {
            SaveGenericFuncs(panelFuncs, "panelFuncs");
        }

        private void SaveGenericFuncs(Dictionary<string, List<Function>> funcs, string fileName)
        {
            JArray array = new JArray();


            foreach (string nameSpace in funcs.Keys)
            {
                foreach (Function func in funcs[nameSpace])
                {
                    bool replaced = false;
                    string snippet = BuildGlobalSnippet(func);
                    foreach (JObject ob in array)
                    {
                        if (ob["snippet"].Value<string>() == snippet)
                        {
                            Match m = Regex.Match(ob["displayText"].Value<string>(), @"(?:\[?(.*?)\]?\:(.*))");
                            if (m.Success)
                            {
                                string newDisplay = m.Groups[1].Value;
                                newDisplay = $"[{newDisplay},{func.Parent}]:{func.Name}";
                                ob["displayText"] = newDisplay;
                                replaced = true;
                                break;
                            }
                            Console.WriteLine($"FAILED SAVING {func.Parent}:{func.Name}");
                        }
                    }
                    if (replaced)
                        continue;

                    array.Add(new JObject()
                    {
                        {"name", func.Name},
                        {"displayText", $"{func.Parent}:{func.Name}"},
                        {"snippet", BuildGlobalSnippet(func)},
                        {"leftLabel", string.Join(",", func.ReturnValues?.Select(ret => ret.Type) ?? new[] {"func"})},
                        {"rightLabel", $"{func.Realm}-Classfunc"},
                        {"type", "method"},
                        {"desc", func.Description},
                        {"descUrl", func.DescriptionUrl}
                    });
                }
            }
            File.WriteAllText(fileName + ".json", array.ToString(Formatting.None));
            File.WriteAllText(fileName + "Pretty.json", array.ToString(Formatting.Indented));
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

        public string BuildHookSnippet(Hook hook)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"function {hook.Parent}:{hook.Name}(");
            if (hook.Args != null && hook.Args.Count > 0)
            {
                for (int i = 0; i < hook.Args.Count; i++)
                    builder.Append($"${{{i + 1}:{hook.Args[i].Type} {hook.Args[i].Name}{(hook.Args[i].Default != "" ? "=" + hook.Args[i].Default : "")}}},");
                builder.Length--;
            }

            builder.AppendLine(")");
            builder.Append("    ");

            if (hook.ReturnValues != null && hook.ReturnValues.Count > 0)
            {
                builder.Append("return ");
                for (int i = 0; i < hook.ReturnValues.Count; i++)
                    builder.Append($"${{{i + 1 + hook.Args?.Count ?? 0}:{hook.ReturnValues[i].Type}}},");
                builder.Length--;
            }
            else
                builder.Append($"${{{hook.Args?.Count + 1 ?? 1}:-- body}}");
            builder.AppendLine();
            builder.Append("end");

            return builder.ToString();
        }
    }
}