#region USING_DIRECTIVES

using System.ComponentModel;
using System.Resources;

#endregion USING_DIRECTIVES

namespace VisualStudioBackground.Localized
{
    internal class LocalManager
    {
        internal static ResourceManager _resourceManager = null;

        private static ResourceManager GetInstance()
        {
            if (_resourceManager == null)
            {
                _resourceManager = ResLocalized.ResourceManager;
            }

            return _resourceManager;
        }

        internal class LocalizedDescriptionAttribute : DescriptionAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedDescriptionAttribute(string _key)
                : base(Localize(_key))
            {
                // empty constructor
            }
        }

        internal class LocalizedCategoryAttribute : CategoryAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedCategoryAttribute(string _key)
                : base(Localize(_key))
            {
                // empty constructor
            }
        }

        internal class LocalizedDisplayNameAttribute : DisplayNameAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedDisplayNameAttribute(string _key)
                : base(Localize(_key))
            {
                // empty constructor
            }
        }
    }
}