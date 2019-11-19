#region USING_DIRECTIVES
using System;
using System.IO;
using System.Windows.Media.Imaging;
using VisualStudioBackground.Settings;
#endregion

namespace VisualStudioBackground.Helpers
{
    public class ImageProvider : IImageProvider
    {
        private BitmapImage _bitmap;
        private Setting _setting;
        public event EventHandler NewImageAvailable;

        public ImageProvider(Setting setting)
        {
            _setting = setting;
            _setting.OnChanged.AddEventHandler(ReloadSettings);

            LoadImage();
        }

        ~ImageProvider()
        {
            if (_setting != null)
            {
                _setting.OnChanged.RemoveEventHandler(ReloadSettings);
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            LoadImage();
            NewImageAvailable?.Invoke(this, EventArgs.Empty);
        }

        public BitmapSource GetBitmap()
        {
            if (_setting.ImageStretch == ImageStretch.None && (_bitmap.Width != _bitmap.PixelHeight || _bitmap.Height != _bitmap.PixelHeight))
            {
                return BitmapTool.ConvertToDpi96(_bitmap);
            } else { return _bitmap; }
        }

        public BitmapSource GetBitMap()
        {
            throw new NotImplementedException(); // I shouldn't need this but otherwise it threw an error saying I did not implement GetBitmap even thought its right above...
        }

        private void LoadImage()
        {
            var fileUri = new Uri(_setting.BackgroundImageAbsolutePath, UriKind.RelativeOrAbsolute);
            var fileInfo = new FileInfo(_setting.BackgroundImageAbsolutePath);

            if (fileInfo.Exists)
            {
                _bitmap = new BitmapImage();
                _bitmap.BeginInit();
                _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                _bitmap.CreateOptions = BitmapCreateOptions.None;
                _bitmap.UriSource = fileUri;
                _bitmap.EndInit();
                _bitmap.Freeze();

                if (_setting.ImageStretch == ImageStretch.None)
                {
                    _bitmap = BitmapTool.EnsureMaxWidthHeight(_bitmap, _setting.MaxWidth, _setting.MaxHeight);
                }
            } else { _bitmap = null; }
        }

        public ImageBackgroundType ProviderType
        {
            get { return ImageBackgroundType.Single; }
        }
    }
}