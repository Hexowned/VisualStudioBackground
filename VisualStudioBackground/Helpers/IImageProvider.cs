#region USING_DIRECTIVES

using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using VisualStudioBackground.Settings;

#endregion USING_DIRECTIVES

namespace VisualStudioBackground.Helpers
{
    public interface IImageProvider
    {
        BitmapSource GetBitmap();

        event EventHandler NewImageAvailable;

        ImageBackgroundType ProviderType { get; }
    }

    public class ProviderHolder
    {
        private ProviderHolder()
        {
        }

        public List<IImageProvider> Providers { get; private set; }

        private static Lazy<ProviderHolder> _instance = new Lazy<ProviderHolder>(() => new ProviderHolder());

        public static ProviderHolder Instance
        {
            get { return _instance.Value; }
        }

        public static void Initialize(Setting setting, List<IImageProvider> providers)
        {
            if (_instance.Value.Providers == null)
            {
                _instance.Value.Providers = providers;
            }
        }
    }
}