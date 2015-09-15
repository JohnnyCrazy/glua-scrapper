using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using glua_scraper.provider;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace glua_scraper
{
    public class Program
    {
        private static int _proccesedFuncs = 0;
        private static int _maxFuncs = 0;

        private static IProvider _provider = new AtomProvider();

        private static void Main(string[] args)
        {
            Options options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                _provider = Options.GetProvider(options.ProviderName);
                if (_provider == null)
                {
                    Console.WriteLine("Unknown Provider");
                    Console.WriteLine("Providers: " + string.Join(",", Options.GetProviders().Select(prov => prov.GetName())));
                    return;
                }
                Console.WriteLine($"Provider: {_provider.GetName()}");
                Console.WriteLine($"Modes: {string.Join(",", options.Modes)}");

                _provider.OnStart();

                if (options.Modes.Contains("all") || options.Modes.Contains("hooks"))
                {
                    Dictionary<string, List<Hook>> hooks = LoadHooks();
                    _provider.SaveHooks(hooks);
                }

                if (options.Modes.Contains("all") || options.Modes.Contains("libfuncs"))
                {
                    Dictionary<string, List<Function>> libFuncs = LoadLibraries();
                    _provider.SaveLibFuncs(libFuncs);
                }

                if (options.Modes.Contains("all") || options.Modes.Contains("globals"))
                {
                    Dictionary<string, List<Function>> globals = LoadGlobals();
                    _provider.SaveGlobals(globals);
                }

                if (options.Modes.Contains("all") || options.Modes.Contains("classfuncs"))
                {
                    Dictionary<string, List<Function>> classFuncs = LoadClassFuncs();
                    _provider.SaveClassFuncs(classFuncs);
                }

                if (options.Modes.Contains("all") || options.Modes.Contains("panelfuncs"))
                {
                    Dictionary<string, List<Function>> panelFuncs = LoadPanelFuncs();
                    _provider.SavePanelFuncs(panelFuncs);
                }

                _provider.OnFinish();

                Console.ReadKey();
            }
        }

        public static void SetConsoleTitle()
        {
            using (WebClient wc = new WebClient())
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Class_Functions"));
                var htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                _maxFuncs += htmlNodes.Count;

                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Panel_Functions"));
                htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                _maxFuncs += htmlNodes.Count;

                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Hooks"));
                htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                _maxFuncs += htmlNodes.Count;

                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Library_Functions"));
                htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                _maxFuncs += htmlNodes.Count;

                Console.WriteLine($"Found {_maxFuncs} total Funcs, Let's go");
                Console.Title = $"Scraper | [0/{_maxFuncs}]";
            }
        }

        private static Dictionary<string, List<Function>> LoadPanelFuncs()
        {
            Dictionary<string, List<Function>> funcs = new Dictionary<string, List<Function>>();

            using (WebClient wc = new WebClient())
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Panel_Functions"));

                var htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                Console.WriteLine($"Found {htmlNodes.Count()} panelfuncs");

                for (int i = 0; i < htmlNodes.Count; i++)
                {
                    var href = htmlNodes[i].ChildNodes.First().Attributes["href"].Value;
                    string nameSpace = htmlNodes[i].InnerText.Split('/').First().Replace(" ", "_");

                    Console.WriteLine($"Processing [{i}/{htmlNodes.Count}] {nameSpace}: {"http://wiki.garrysmod.com" + href}");

                    Function func = Function.ProccessPanelFunction(nameSpace, "http://wiki.garrysmod.com" + href);
                    if (func == null)
                    {
                        Console.WriteLine($"INVALID: {nameSpace}:{"http://wiki.garrysmod.com" + href}");
                        continue;
                    }
                    _proccesedFuncs++;
                    Console.Title = $"Scraper | [{_proccesedFuncs}/{_maxFuncs}]";
                    if (funcs.ContainsKey(nameSpace))
                        funcs[nameSpace].Add(func);
                    else
                        funcs[nameSpace] = new List<Function> { func };
                }
            }

            return funcs;
        }

        private static Dictionary<string, List<Function>> LoadClassFuncs()
        {
            Dictionary<string, List<Function>> funcs = new Dictionary<string, List<Function>>();

            using (WebClient wc = new WebClient())
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Class_Functions"));

                var htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                Console.WriteLine($"Found {htmlNodes.Count()} classfuncs");

                for (int i = 0; i < htmlNodes.Count; i++)
                {
                    var href = htmlNodes[i].ChildNodes.First().Attributes["href"].Value;
                    string nameSpace = htmlNodes[i].InnerText.Split('/').First().Replace(" ", "_");

                    Console.WriteLine($"Processing [{i}/{htmlNodes.Count}] {nameSpace}: {"http://wiki.garrysmod.com" + href}");

                    Function func = Function.ProccessFunction(nameSpace, "http://wiki.garrysmod.com" + href);
                    if (func == null)
                    {
                        Console.WriteLine($"INVALID: {nameSpace}:{"http://wiki.garrysmod.com" + href}");
                        continue;
                    }
                    _proccesedFuncs++;
                    Console.Title = $"Scraper | [{_proccesedFuncs}/{_maxFuncs}]";
                    if (funcs.ContainsKey(nameSpace))
                        funcs[nameSpace].Add(func);
                    else
                        funcs[nameSpace] = new List<Function> { func };
                }
            }

            return funcs;
        }

        private static Dictionary<string, List<Function>> LoadGlobals()
        {
            Dictionary<string, List<Function>> funcs = new Dictionary<string, List<Function>>();

            using (WebClient wc = new WebClient())
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Global"));

                var htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                Console.WriteLine($"Found {htmlNodes.Count()} globals");

                for (int i = 0; i < htmlNodes.Count; i++)
                {
                    var href = htmlNodes[i].ChildNodes.First().Attributes["href"].Value;
                    string nameSpace = htmlNodes[i].InnerText.Split('/').First().Replace(" ", "_");

                    Console.WriteLine($"Processing [{i}/{htmlNodes.Count}] {nameSpace}: {"http://wiki.garrysmod.com" + href}");

                    Function func = Function.ProccessFunction(nameSpace, "http://wiki.garrysmod.com" + href);
                    if (func == null)
                    {
                        Console.WriteLine($"INVALID: {nameSpace}:{"http://wiki.garrysmod.com" + href}");
                        continue;
                    }
                    _proccesedFuncs++;
                    Console.Title = $"Scraper | [{_proccesedFuncs}/{_maxFuncs}]";
                    if (funcs.ContainsKey(nameSpace))
                        funcs[nameSpace].Add(func);
                    else
                        funcs[nameSpace] = new List<Function> { func };
                }
            }

            return funcs;
        }

        private static Dictionary<string, List<Hook>> LoadHooks()
        {
            Dictionary<string, List<Hook>> hooks = new Dictionary<string, List<Hook>>();

            using (WebClient wc = new WebClient())
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Hooks"));

                var htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                Console.WriteLine($"Found {htmlNodes.Count()} hooks");

                for (int i = 0; i < htmlNodes.Count; i++)
                {
                    var href = htmlNodes[i].ChildNodes.First().Attributes["href"].Value;
                    string nameSpace = htmlNodes[i].InnerText.Split('/').First().Replace(" ", "_");

                    Console.WriteLine($"Processing [{i}/{htmlNodes.Count}] {nameSpace}: {"http://wiki.garrysmod.com" + href}");

                    Hook hook = Hook.ProcessHook(nameSpace, "http://wiki.garrysmod.com" + href);
                    if (hook == null)
                    {
                        Console.WriteLine($"INVALID: {nameSpace}:{"http://wiki.garrysmod.com" + href}");
                        continue;
                    }
                    _proccesedFuncs++;
                    Console.Title = $"Scraper | [{_proccesedFuncs}/{_maxFuncs}]";
                    if (hooks.ContainsKey(nameSpace))
                        hooks[nameSpace].Add(hook);
                    else
                        hooks[nameSpace] = new List<Hook> { hook };
                }
            }
            return hooks;
        }

        private static Dictionary<string, List<Function>> LoadLibraries()
        {
            Dictionary<string, List<Function>> funcs = new Dictionary<string, List<Function>>();

            using (WebClient wc = new WebClient())
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wc.DownloadString("http://wiki.garrysmod.com/page/Category:Library_Functions"));

                var htmlNodes = doc.DocumentNode.Descendants("li").Where(node => node.Id == "").Reverse().Skip(2).Reverse().ToList();
                Console.WriteLine($"Found {htmlNodes.Count()} libraries");

                for (int i = 0; i < htmlNodes.Count; i++)
                {
                    var href = htmlNodes[i].ChildNodes.First().Attributes["href"].Value;
                    string nameSpace = htmlNodes[i].InnerText.Split('/').First().Replace(" ", "_");

                    if (nameSpace == "Global")
                    {
                        continue;
                    }

                    Console.WriteLine($"Processing [{i}/{htmlNodes.Count}] {nameSpace}: {"http://wiki.garrysmod.com" + href}");

                    Function func = Function.ProccessFunction(nameSpace, "http://wiki.garrysmod.com" + href);
                    if(func == null)
                    {
                        Console.WriteLine($"INVALID: {nameSpace}:{"http://wiki.garrysmod.com" + href}");
                        continue;
                    }
                    _proccesedFuncs++;
                    Console.Title = $"Scraper | [{_proccesedFuncs}/{_maxFuncs}]";
                    if (funcs.ContainsKey(nameSpace))
                        funcs[nameSpace].Add(func);
                    else
                        funcs[nameSpace] = new List<Function> { func };
                }
            }

            return funcs;
        }
    }
}