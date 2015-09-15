using System.Collections.Generic;

namespace glua_scraper.provider
{
    public interface IProvider
    {
        string GetName();
        void OnStart();
        void OnFinish();
        void SaveHooks(Dictionary<string, List<Hook>> hooks);
        void SaveGlobals(Dictionary<string, List<Function>> globals);
        void SaveLibFuncs(Dictionary<string, List<Function>> libFuncs);
        void SaveClassFuncs(Dictionary<string, List<Function>> classFuncs);
        void SavePanelFuncs(Dictionary<string, List<Function>> panelFuncs);
    }
}