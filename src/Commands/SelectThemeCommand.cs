using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace ThemeSwitcher
{
    [Command(PackageIds.FirstTheme)]
    internal sealed class SelectThemeCommand : BaseCommand<SelectThemeCommand>
    {
        private static readonly List<OleMenuCommand> _commands = new();
        protected override void BeforeQueryStatus(EventArgs e)
        {
            if (_commands.Any())
            {
                return; // The commands were already set up
            }

            IEnumerable<Theme> themes = ThemeStore.Themes.Value;
            OleMenuCommandService mcs = Package.GetService<IMenuCommandService, OleMenuCommandService>();
            var i = 1;

            SetupCommand(Command, themes.First());

            foreach (Theme theme in themes.Skip(1))
            {
                CommandID cmdId = new(PackageGuids.ThemeSwitcher, PackageIds.FirstTheme + i++);
                OleMenuCommand command = new(Execute, cmdId);
                SetupCommand(command, theme);
                mcs.AddCommand(command);
            }
        }

        private void SetupCommand(OleMenuCommand command, Theme theme)
        {
            command.Enabled = command.Visible = true;
            command.Text = theme.Name;
            command.Checked = theme.IsActive;
            command.Properties["guid"] = theme.Guid;
            _commands.Add(command);
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
