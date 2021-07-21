using System.Diagnostics;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace ThemeSwitcher
{
    [Command(PackageIds.GetMoreThemes)]
    internal sealed class GetMoreThemesCommand : BaseCommand<GetMoreThemesCommand>
    {
        private const string _url = "https://marketplace.visualstudio.com/search?term=theme&target=VS&vsVersion=";

        protected override async System.Threading.Tasks.Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            EnvDTE80.DTE2 dte = await VS.GetServiceAsync<EnvDTE.DTE, EnvDTE80.DTE2>();

            var vs = dte.Version switch
            {
                "16.0" => "vs2019",
                "17.0" => "vs2022",
                _ => ""
            };

            Process.Start(_url + vs);
        }
    }
}
