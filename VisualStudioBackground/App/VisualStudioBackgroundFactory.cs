#region USING_DIRECTIVES

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VisualStudioBackground.Helpers;
using VisualStudioBackground.Settings;

#endregion USING_DIRECTIVES

namespace VisualStudioBackground
{
    #region ADORNMENT_FACTORY

    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [ContentType("BuildOutput")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class VisualStudioAdornmentFactory : IWpfTextViewCreationListener
    {
        private List<IImageProvider> ImageProviders;

        [Import(typeof(SVsServiceProvider))]
        internal System.IServiceProvider ServiceProvider { get; set; }

        [Export(typeof(AdornmentLayerDefinition))]
        [Name("VisualStudioBackground")]
        [Order(Before = PredefinedAdornmentLayers.DifferenceChanges)]
        public AdornmentLayerDefinition EditorAdornmentLayer { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            var settings = Setting.Initialize(ServiceProvider);
            if (ImageProviders == null)
            {
                if (ProviderHolder.Instance.Providers == null)
                {
                    ProviderHolder.Initialize(settings, new List<IImageProvider>
                    {
                        new ImageProvider(settings)
                    });
                }

                ImageProviders = ProviderHolder.Instance.Providers;
            }

            new VisualStudioBackground(textView, ImageProviders);
        }
    }

    #endregion ADORNMENT_FACTORY
}
