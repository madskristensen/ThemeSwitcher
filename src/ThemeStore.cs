using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Task = System.Threading.Tasks.Task;

namespace ThemeSwitcher
{
    public static class ThemeStore
    {
        public static Lazy<IEnumerable<Theme>> Themes = new(GetInstalledThemes);
        private static IEnumerable<Theme> GetInstalledThemes()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SettingsStore store = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetReadOnlySettingsStore(SettingsScope.Configuration);
            List<Theme> themes = new();

            if (store.CollectionExists("Themes"))
            {
                IEnumerable<string> guids = store.GetSubCollectionNames("Themes");

                foreach (var guid in guids)
                {
                    var collection = $@"Themes\{guid}";

                    if (store.PropertyExists(collection, ""))
                    {
                        var name = store.GetString(collection, "");

                        if (name == "Additional Contrast")
                        {
                            name = "Blue (Extra Contrast)";
                        }

                        themes.Add(new Theme(name, new Guid(guid)));
                    }
                }
            }

            Guid activeGuid = GetActiveTheme();
            Theme activeTheme = themes.SingleOrDefault(t => t.Guid == activeGuid);

            if (activeTheme != null)
            {
                activeTheme.IsActive = true;
            }

            return themes.OrderBy(t => t.Name);
        }

        public static Guid GetActiveTheme()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            const string COLLECTION_NAME = @"ApplicationPrivateSettings\Microsoft\VisualStudio";
            const string PROPERTY_NAME = "ColorTheme";

            SettingsStore store = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (store.CollectionExists(COLLECTION_NAME))
            {
                if (store.PropertyExists(COLLECTION_NAME, PROPERTY_NAME))
                {
                    // The value is made up of three parts, separated
                    // by a star. The third part is the GUID of the theme.
                    var parts = store.GetString(COLLECTION_NAME, PROPERTY_NAME).Split('*');
                    if (parts.Length == 3)
                    {
                        if (Guid.TryParse(parts[2], out Guid value))
                        {
                            return value;
                        }
                    }
                }
            }

            return Guid.Empty;
        }

        public static async Task SetActiveThemeAsync(Guid themeGuid)
        {
            var settingsFile = string.Format(_vsSettings, "{" + themeGuid + "}");
            var path = Path.Combine(Path.GetTempPath(), "temp.vssettings");

            System.IO.File.WriteAllText(path, settingsFile);

            await KnownCommands.Tools_ImportandExportSettings.ExecuteAsync($@"/import:""{path}""");
            
            foreach (Theme theme in Themes.Value)
            {
                theme.IsActive = false;

                if (theme.Guid == themeGuid)
                {
                    theme.IsActive = true;
                }
            }
        }

        public const string _vsSettings = @"<UserSettings>
    <ApplicationIdentity version=""17.0""/>
    <ToolsOptions>
        <ToolsOptionsCategory name=""Environment"" RegisteredName=""Environment""/>
    </ToolsOptions>
    <Category name=""Environment_Group"" RegisteredName=""Environment_Group"">
        <Category name=""Environment_FontsAndColors"" Category=""{{1EDA5DD4-927A-43a7-810E-7FD247D0DA1D}}"" Package=""{{DA9FB551-C724-11d0-AE1F-00A0C90FFFC3}}"" RegisteredName=""Environment_FontsAndColors"" PackageName=""Visual Studio Environment Package"">
            <PropertyValue name=""Version"">2</PropertyValue>
            <FontsAndColors Version=""2.0"">
                <Theme Id=""{0}""/>
            </FontsAndColors>
        </Category>
    </Category>
</UserSettings>";
    }
}
