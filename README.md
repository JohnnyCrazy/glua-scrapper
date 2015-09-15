# Garry's mod Wiki Extractor

This small tool will extract function info out of [wiki.garrysmod.com](http://wiki.garrysmod.com) and prepare it for some text-editor

## Usage
> /bin/Release/glua-scraper.exe

>glua-scraper 1.0.0.0
>Copyright Jonas Dellinger c - 2015
>
>  -p, --provider    Required. The provider used to save the data
>
>  -m, --modes       Required. Which functions we should get
>                    [all,hooks,libfuncs,globals,classfuncs,panelfuncs]
>
>  --help            Display this help screen.
>
> Providers: AtomIO,SublimeText

Extract all functions and convert it into a sublime-text format used in  [Sublime-GLua-Highlight](https://github.com/FPtje/Sublime-GLua-Highlight):
> glua-scraper.exe -p SublimeText -m all

Extract all functions and convert it into a atom.io format used in  [glua-autocomplete](https://github.com/JohnnyCrazy/glua-autocomplete):
> glua-scraper.exe -p AtomIO -m all

Extract only hooks and convert it into a atom.io format used in  [glua-autocomplete](https://github.com/JohnnyCrazy/glua-autocomplete):
> glua-scraper.exe -p AtomIO -m hooks

## Releases

Releases will be made if there are any big changes. All libraries will be merged into the main execetuable with every release.
