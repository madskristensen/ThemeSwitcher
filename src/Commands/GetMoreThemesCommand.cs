using System;
using Community.VisualStudio.Toolkit;

namespace ThemeSwitcher
{
    [Command(PackageIds.GetMoreThemes)]
    internal sealed class GetMoreThemesCommand : BaseCommand<GetMoreThemesCommand>
    {
        protected override void Execute(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://marketplace.visualstudio.com/search?target=VS&category=Tools&vsVersion=&subCategory=Themes&sortBy=Installs");
        }
    }
}
