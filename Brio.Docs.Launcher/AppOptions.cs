using CommandLine;

namespace Brio.Docs.Launcher
{
    public class AppOptions
    {
        public AppOptions()
            : this(false, null, null, null)
        {
        }

        public AppOptions(bool devMode, string dMExecutable, string languageTag, string passingArguments)
        {
            DevMode = devMode;
            DMExecutable = dMExecutable;
            LanguageTag = languageTag;
            PassingArguments = passingArguments;
        }

        [Option('d', "develop", Default = false, HelpText = "Set development mode")]
        public bool DevMode { get; }

        [Option('e', "executable", HelpText = "Path to DM service executable")]
        public string DMExecutable { get; }

        [Option('l', "language", HelpText = "UI language")]
        public string LanguageTag { get; }

        [Option('p', "pass", HelpText = "Arguments to pass to DM service")]
        public string PassingArguments { get; }
    }
}
