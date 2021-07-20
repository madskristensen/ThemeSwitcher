using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace ThemeSwitcher
{
    [Command(PackageIds.FirstTheme)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        private static readonly List<OleMenuCommand> _commands = new();
        protected override void BeforeQueryStatus(EventArgs e)
        {
            Command.Enabled = true;

            IEnumerable<Theme> themes = ThemeStore.Themes.Value;
            OleMenuCommandService mcs = Package.GetService<IMenuCommandService, OleMenuCommandService>();
            var i = 1;

            Theme firstTheme = themes.First();
            Command.Text = firstTheme.Name;
            Command.Checked = firstTheme.IsActive;
            Command.Properties["guid"] = firstTheme.Guid;

            foreach (Theme theme in themes.Skip(1))
            {
                CommandID cmdId = new(PackageGuids.ThemeSwitcher, PackageIds.FirstTheme + i++);

                if (!_commands.Any(c => c.CommandID.ID == cmdId.ID))
                {
                    OleMenuCommand command = new(Execute, cmdId);
                    command.Properties["guid"] = theme.Guid;
                    command.Text = theme.Name;
                    command.Checked = theme.IsActive;
                    mcs.AddCommand(command);

                    _commands.Add(command);
                }
            }
        }

        protected override void Execute(object sender, EventArgs e)
        {
            var command = (OleMenuCommand)sender;

            if (command.Properties.Contains("guid"))
            {
                var guid = (Guid)command.Properties["guid"];
                ThemeStore.SetActiveThemeAsync(guid).FireAndForget();

                foreach (OleMenuCommand cmd in _commands)
                {
                    cmd.Checked = false;
                }

                command.Checked = true;
            }
        }
    }
}
