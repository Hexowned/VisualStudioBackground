#region USING_DIRECTIVES

using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VisualStudioBackground.Helpers;
using VisualStudioBackground.Settings;

#endregion USING_DIRECTIVES

namespace VisualStudioBackground.App
{
    public class VisualStudioBackground
    {
        private readonly List<IImageProvider> _imageProviders;
        private IImageProvider _imageProvider;
        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _adornmentLayer;
        private Canvas _editorCanvas = new Canvas() { IsHitTestVisible = false };
        private Setting _settings = Setting.Instance;
        private bool _isMainWindow;
        private DependencyObject _wpfTextViewHost = null;
        private Dictionary<int, DependencyObject> _defaultThemeColor = new Dictionary<int, DependencyObject>();
        private bool _hasImage = false;
        private bool _isRootWindow = false;

        public VisualStudioBackground(IWpfTextView view, List<IImageProvider> imageProviders)
        {
            try
            {
                _imageProviders = imageProviders;
                _imageProvider = imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);

                if (_imageProvider == null)
                {
                    _imageProvider = new ImageProvider(_settings);
                }

                _view = view;
                _adornmentLayer = view.GetAdornmentLayer("VisualStudioBackground");
                _view.LayoutChanged += (s, e) =>
                {
                    if (!_hasImage) { ChangeImage(); }
                    else
                    {
                        RefreshBackground();
                    }
                };

                _view.Closed += (s, e) =>
                {
                    _imageProviders.ForEach(x => x.NewImageAvailable -= InvokeChangeImage);
                    if (_settings != null)
                    {
                        _settings.OnChanged.RemoveEventHandler(ReloadSettings);
                    }
                };

                _view.BackgroundBrushChanged += (s, e) =>
                {
                    _hasImage = false;
                    SetCanvasBackground();
                };

                _settings.OnChanged.AddEventHandler(ReloadSettings);
                _imageProviders.ForEach(x => x.NewImageAvailable += InvokeChangeImage);

                SetCanvasBackground();
                ChangeImage();
                RefreshAdornment();
            }
            catch
            {
                // nothing to catch for now
            }
        }

        private void InvokeChangeImage(object sender, System.EventArgs e)
        {
            try
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ChangeImage();
                    GC.Collect();
                });
            }
            catch
            {
                // nothing for now
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
            _hasImage = false;
            ChangeImage();
        }

        private void ChangeImage()
        {
            try
            {
                SetCanvasBackground();
                FindWpfTextView(_editorCanvas as DependencyObject);
                if (_wpfTextViewHost == null) return;

                var newImage = _imageProvider.GetBitmap();
                var opacity = _settings.ExpandToFillIDE && _isMainWindow ? 0.0 : _settings.Opacity;

                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        if (_isRootWindow)
                        {
                            var grid = new Grid()
                            {
                                Name = "BackgroundIdeImage",
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                IsHitTestVisible = false
                            };

                            var ni = new Image()
                            {
                                Source = newImage,
                                Stretch = _settings.ImageStretch.ConvertTo(),
                                HorizontalAlignment = _settings.PositionHorizontal.ConvertToHorizontalAlignment(),
                                VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment(),
                                Opacity = opacity,
                                IsHitTestVisible = false
                            };

                            grid.Children.Insert(0, ni);
                            Grid.SetRowSpan(grid, 3);
                            Grid.SetColumnSpan(grid, 3);

                            var p = VisualTreeHelper.GetParent(_wpfTextViewHost) as Grid;
                            if (p != null)
                            {
                                foreach (var c in p.Children)
                                {
                                    if ((c as Grid)?.Name == "BackgroundIdeImage")
                                    {
                                        p.Children.Remove(c as UIElement);
                                        break;
                                    }
                                }

                                p.Children.Insert(0, grid);
                            }
                        }
                        else
                        {
                            var nib = new ImageBrush(newImage)
                            {
                                Stretch = _settings.ImageStretch.ConvertTo(),
                                AlignmentX = _settings.PositionHorizontal.ConvertTo(),
                                AlignmentY = _settings.PositionVertical.ConvertTo(),
                                Opacity = opacity
                            };

                            _wpfTextViewHost.SetValue(Panel.BackgroundProperty, nib);
                        }

                        _hasImage = true;
                    }
                    catch
                    {
                        // nothing for now
                    }
                });
            }
            catch
            {
                // nothing for now
            }
        }

        private void RefreshBackground()
        {
            SetCanvasBackground();
            FindWpfTextView(_editorCanvas as DependencyObject);
            if (_wpfTextViewHost == null) return;

            var opacity = _settings.ExpandToFillIDE && _isMainWindow ? 0.0 : _settings.Opacity;
            var refd = _wpfTextViewHost.GetType();
            var prop = refd.GetProperty("Background");
            var background = prop.GetValue(_wpfTextViewHost) as ImageBrush;

            if (background == null && _isRootWindow)
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        var c = (_wpfTextViewHost as UIElement).Opacity;
                        (_wpfTextViewHost as UIElement).Opacity = c < 0.01 ? 0.01 : c - 0.01;
                        (_wpfTextViewHost as UIElement).Opacity = c;
                    }
                    catch
                    {
                        // nothing for now
                    }
                });
            }
            else
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        background.Opacity = opacity < 0.01 ? 0.01 : opacity - 0.01;
                        background.Opacity = opacity;
                    }
                    catch
                    {
                        // nothing for now
                    }
                });
            }
        }

        private void RefreshAdornment()
        {
            _adornmentLayer.RemoveAdornmentsByTag("VisualStudioBackground");
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative,
                null, "VisualStudioBackground", _editorCanvas, null);
        }

        private void SetCanvasBackground()
        {
            _isMainWindow = IsMainWindow();
            var isTransparent = true;
            var current = _editorCanvas as DependencyObject;

            while (current != null)
            {
                var refd = current.GetType();
                var nameprop = refd.GetProperty("Name");
                var objname = nameprop?.GetValue(current) as string;

                if (!string.IsNullOrEmpty(objname) && (objname.Equals("RootGrid", StringComparison.OrdinalIgnoreCase)
                    || objname.Equals("MainWindow", StringComparison.OrdinalIgnoreCase))) { return; }
                if (_isRootWindow && refd.FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView",
                    StringComparison.OrdinalIgnoreCase)) { return; }
                if (refd.FullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost",
                    StringComparison.OrdinalIgnoreCase)) { isTransparent = _settings.ExpandToFillIDE && _isMainWindow; }
                else if (refd.FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView",
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (FindUI(current, "Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost") == null) { return; }
                }
                else { SetBackgroundToTransparent(current, isTransparent); }

                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    current = LogicalTreeHelper.GetParent(current);
                }
            }
        }

        private bool IsMainWindow()
        {
            var initial = _view as DependencyObject;
            var current = initial;
            var result = initial;

            while (current != null)
            {
                result = current;
                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    current = LogicalTreeHelper.GetParent(current);
                }
            }

            if (result.GetType().FullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost", StringComparison.OrdinalIgnoreCase) ||
                result.GetType().FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextViewHost", StringComparison.OrdinalIgnoreCase))
            {
                _isRootWindow = true;
            }

            if (result.GetType().FullName.Equals("Microsoft.VisualStudio.PlatformUI.MainWindow", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FindWpfTextView(DependencyObject d)
        {
            if (_wpfTextViewHost == null)
            {
                if (_isRootWindow)
                {
                    _wpfTextViewHost = FindUI(d, "Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView");
                }
                else
                {
                    _wpfTextViewHost = FindUI(d, "Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost");
                }

                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        if (_wpfTextViewHost == null) return;
                        RenderOptions.SetBitmapScalingMode(_wpfTextViewHost, BitmapScalingMode.Fant);
                    }
                    catch
                    {
                        // nothing for now
                    }
                });
            }
        }

        private DependencyObject FindUI(DependencyObject d, string name)
        {
            var current = d;
            while (current != null)
            {
                if (current.GetType().FullName.Equals(name, StringComparison.OrdinalIgnoreCase)) { return current; }
                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    current = LogicalTreeHelper.GetParent(current);
                }
            }

            return null;
        }

        private void SetBackgroundToTransparent(DependencyObject d, bool isTransparent)
        {
            var prop = d.GetType().GetProperty("Background");
            var current = prop?.GetValue(d) as Brush;
            if (current == null) return;

            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (isTransparent)
                    {
                        if (!_defaultThemeColor.Any(x => x.Key == d.GetHashCode()))
                        {
                            _defaultThemeColor[d.GetHashCode()] = current as DependencyObject;
                        }

                        prop.SetValue(d, (Brush)Brushes.Transparent);
                    }
                    else
                    {
                        var d1 = _defaultThemeColor.FirstOrDefault(x => x.Key == current.GetHashCode());
                        if (d1.Value != null)
                        {
                            prop.SetValue(d, (Brush)d1.Value);
                        }
                    }
                }
                catch
                {
                    // nothing for now
                }
            });
        }
    }
}