#region USING_DIRECTIVES

using System;
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

        [AttributeUsage(AttributeTargets.All)]
        internal class LocalizedDescriptionAttribute : DescriptionAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedDescriptionAttribute(string _key)
                : base(Localize(_key))
            {
            }
        }

        [AttributeUsage(AttributeTargets.All)]
        internal class LocalizedCategoryAttribute : CategoryAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedCategoryAttribute(string _key)
                : base(Localize(_key))
            {
            }
        }

        [AttributeUsage(AttributeTargets.Class
                      | AttributeTargets.Method
                      | AttributeTargets.Property
                      | AttributeTargets.Event)]
        internal class LocalizedDisplayNameAttribute : DisplayNameAttribute
        {
            private static string Localize(string _key)
            {
                return GetInstance().GetString(_key);
            }

            internal LocalizedDisplayNameAttribute(string _key)
                : base(Localize(_key))
            {
            }
        }
    }
}
