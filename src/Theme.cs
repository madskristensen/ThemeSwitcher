using System;

namespace ThemeSwitcher
{
    public class Theme
    {
        public Theme(string name, Guid guid)
        {
            Name = name;
            Guid = guid;
        }

        public string Name { get; }
        public Guid Guid { get; }
        public bool IsActive { get; set; }
    }
}
