#region USING_DIRECTIVES

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VisualStudioBackground.Helpers;
using VisualStudioBackground.Settings;
using Task = System.Threading.Tasks.Task;

#endregion USING_DIRECTIVES

namespace VisualStudioBackground
{
    public sealed class VisualStudioBackgroundPackage : AsyncPackage
    {
        private Setting _settings;
        private Window _mainWindow;
        private List<IImageProvider> _imageProviders;
        private IImageProvider _imageProvider;
        private Image _current = null;

        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            Application.Current.MainWindow.Loaded += (s, e) =>
            {
                _mainWindow = (Window)s;
                _settings = Setting.Initialize(this);
                _settings.OnChanged.AddEventHandler(ReloadSettings);

                if (ProviderHolder.Instance.Providers == null)
                {
                    ProviderHolder.Initialize(_settings, new List<IImageProvider>
                    {
                        new ImageProvider(_settings)
                    });
                }

                _imageProviders = ProviderHolder.Instance.Providers;
                _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
                _imageProviders.ForEach(x => x.NewImageAvailable += InvokeChangeImage);

                ReloadSettings(null, null);
            };

            Application.Current.MainWindow.Closing += (s, e) =>
            {
                _imageProviders.ForEach(x => x.NewImageAvailable -= InvokeChangeImage);
                if (_settings != null)
                {
                    _settings.OnChanged.RemoveEventHandler(ReloadSettings);
                }
            };

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                    _settings = await Setting.InitializeAsync(this);

                    if (_settings == null) return;
                    _mainWindow = (Window)Application.Current.MainWindow;
                    _settings.OnChanged.AddEventHandler(ReloadSettings);

                    if (ProviderHolder.Instance.Providers == null)
                    {
                        ProviderHolder.Initialize(_settings, new List<IImageProvider>
                        {
                            new ImageProvider(_settings)
                        });
                    }

                    _imageProviders = ProviderHolder.Instance.Providers;
                    _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
                    _imageProviders.ForEach(x => x.NewImageAvailable += InvokeChangeImage);

                    ReloadSettings(null, null);
                } catch
                {
                    // nothing for now
                }
            }).FileAndForget(""); // TODO: package name
        }

        private void InvokeChangeImage(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ChangeImage();
                    GC.Collect();
                });
            } catch
            {
                // nothing for now
            }
        }

        private void ChangeImage()
        {
            try
            {
                var rRootGrid = (Grid)_mainWindow.Template.FindName("RootGrid", _mainWindow);
                var newImage = _imageProvider.GetBitmap();

                foreach (UIElement element in rRootGrid.Children)
                {
                    if (element.GetType() != typeof(Image)) continue;
                    if (_current == null) _current = element as Image;
                    if (_settings.ImageBackgroundType == ImageBackgroundType.Single || !_settings.ExpandToFillIDE)
                    {
                        rRootGrid.Children.Remove(element);
                        _current = null;
                    }
                    break;
                }

                if (!_settings.ExpandToFillIDE) return;
                if (_settings.ImageBackgroundType == ImageBackgroundType.Single || _current == null)
                {
                    var rImageControl = new Image()
                    {
                        Source = newImage,
                        Stretch = _settings.ImageStretch.ConvertTo(),
                        HorizontalAlignment = _settings.PositionHorizontal.ConvertToHorizontalAlignment(),
                        VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment(),
                        Opacity = _settings.Opacity
                    };

                    Grid.SetRowSpan(rImageControl, 4);
                    RenderOptions.SetBitmapScalingMode(rImageControl, BitmapScalingMode.Fant);

                    rRootGrid.Children.Insert(0, rImageControl);

                    var dockTargets = rRootGrid.Descendants<DependencyObject>().Where(x => x.GetType().FullName == "Microsoft.VisualStudio.PlatformUI.Shell.Controls.DockTarget");
                    foreach (var dockTarget in dockTargets)
                    {
                        var grids = dockTarget?.Descendants<Grid>();
                        foreach (var g in grids)
                        {
                            if (g == null) continue;
                            var prop = g.GetType().GetProperty("Background");
                            var bg = prop.GetValue(g) as SolidColorBrush;
                            if (bg == null || bg.Color.A == 0x00) continue;

                            prop.SetValue(g, new SolidColorBrush(new Color()
                            {
                                A = 0x00,
                                B = bg.Color.B,
                                G = bg.Color.G,
                                R = bg.Color.R
                            }));
                        }
                    }
                }
            } catch
            {
                // nothing for now
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ChangeImage();
            });
        }
    }
}
