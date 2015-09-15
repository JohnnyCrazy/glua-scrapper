using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using CommandLine.Text;
using glua_scraper.provider;
using Newtonsoft.Json;

namespace glua_scraper
{
    public class Options
    {
        [Option('p', "provider", Required = true, HelpText = "The provider used to save the data")]
        public string ProviderName { get; set; }

        [OptionList('m', "modes", Required = true, HelpText = "Which functions we should get [all,hooks,libfuncs,globals,classfuncs,panelfuncs]")]
        public IEnumerable<string> Modes { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            StringBuilder builder = new StringBuilder(HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current)));

            builder.AppendLine("Providers: " + string.Join(",", GetProviders().Select(prov => prov.GetName())));

            return builder.ToString();
        }

        public static List<IProvider> GetProviders()
        {
            List<IProvider> list = new List<IProvider>();

            IEnumerable<Type> types = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "glua_scraper.provider");
            foreach (Type type in types)
            {
                if (type.GetInterface("IProvider") == null) continue;
                IProvider prov = Activator.CreateInstance(type) as IProvider;
                list.Add(prov);
            }

            return list;
        }

        public static IProvider GetProvider(string name)
        {
            IEnumerable<Type> types = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "glua_scraper.provider");
            foreach (Type type in types)
            {
                if (type.GetInterface("IProvider") == null) continue;
                IProvider prov = Activator.CreateInstance(type) as IProvider;
                if (string.Equals(prov?.GetName(), name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return prov;
                }
            }
            return null;
        }

        private static IEnumerable<Type> GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }
    }
}