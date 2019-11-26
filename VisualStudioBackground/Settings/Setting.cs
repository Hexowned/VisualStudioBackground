#region USING_DIRECTIVES

using EnvDTE;
using EnvDTE80;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#endregion USING_DIRECTIVES

namespace VisualStudioBackground.Settings
{
    public class Setting
    {
        private static readonly string ConfigurationFile = "Resources\\configuration.txt";
        private const string DefaultBackgroundImage = "Resources\\background.jpg";
        private const string DefaultBackgroundFolder = "Resources";

        public ImageBackgroundType ImageBackgroundType { get; set; }
        public double Opacity { get; set; }
        public PositionV PositionVertical { get; set; }
        public PositionH PositionHorizontal { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public ImageStretch ImageStretch { get; set; }
        public bool ExpandToFillIDE { get; set; }
        public string BackgroundImageAbsolutePath { get; set; }
        public string Extensions { get; set; }

        internal IServiceProvider ServiceProvider { get; set; }

        public WeakEvent<EventArgs> OnChanged = new WeakEvent<EventArgs>();

        public static Setting Instance { get; } = new Setting();

        public Setting()
        {
            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            BackgroundImageAbsolutePath = Path.Combine(string.IsNullOrWhiteSpace(assemblyLocation) ? "" : assemblyLocation, DefaultBackgroundFolder);
            Opacity = 0.30;
            PositionHorizontal = PositionH.Right;
            PositionVertical = PositionV.Bottom;
            ImageStretch = ImageStretch.None;
            Extensions = ".png, .jpg";
            ImageBackgroundType = ImageBackgroundType.Single;
            MaxWidth = 0;
            MaxHeight = 0;
            ExpandToFillIDE = false;
        }

        public static Setting Initialize(IServiceProvider serviceProvider)
        {
            var settings = Instance;
            if (settings.ServiceProvider != serviceProvider)
            {
                settings.ServiceProvider = serviceProvider;
            }
            try
            {
                settings.Load();
            } catch
            {
                return Deserialize();
            }

            return settings;
        }

        public void Serialize()
        {
            var configuration = JsonSerializer<Setting>.Serialize(this);
            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configurationPath = Path.Combine(string.IsNullOrEmpty(assemblyLocation) ? "" : assemblyLocation, ConfigurationFile);

            using (var s = new StreamWriter(configurationPath, false, Encoding.ASCII))
            {
                s.Write(configuration);
                s.Close();
            }
        }

        public static Setting Deserialize()
        {
            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configurationPath = Path.Combine(string.IsNullOrEmpty(assemblyLocation) ? "" : assemblyLocation, ConfigurationFile);
            string configuration = "";

            using (var s = new StreamReader(configurationPath, Encoding.ASCII, false))
            {
                configuration = s.ReadToEnd();
                s.Close();
            }

            var ret = JsonSerializer<Setting>.Deserialize(configuration);
            ret.BackgroundImageAbsolutePath = ToFullPath(ret.BackgroundImageAbsolutePath, DefaultBackgroundImage);

            return ret;
        }

        public static async Task<Setting> InitializeAsync(IServiceProvider serviceProvider)
        {
            var settings = Instance;
            if (settings.ServiceProvider != serviceProvider)
            {
                settings.ServiceProvider = serviceProvider;
            }
            try { await settings.LoadAsync(); } catch { return Deserialize(); }

            return settings;
        }

        public async Task LoadAsync()
        {
            var asyncServiceProvider = await ((Microsoft.VisualStudio.Shell.AsyncPackage)ServiceProvider).GetServiceAsync(typeof(Microsoft.VisualStudio.Shell.Interop.SAsyncServiceProvider)) as Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
            var testService = await asyncServiceProvider.GetServiceAsync(typeof(DTE)) as DTE;
            var props = testService.Properties["VisualStudioBackground", "General"];

            Load(props);
        }

        private void Load(Properties props)
        {
            BackgroundImageAbsolutePath = Setting.ToFullPath((string)props.Item("").Value, DefaultBackgroundImage);
            Opacity = (double)props.Item("Opacity").Value;
            PositionHorizontal = (PositionH)props.Item("PositionHorizontal").Value;
            PositionVertical = (PositionV)props.Item("PositionVertical").Value;
            ImageStretch = (ImageStretch)props.Item("ImageStretch").Value;
            Extensions = (string)props.Item("Extensions").Value;
            ImageBackgroundType = (ImageBackgroundType)props.Item("ImageBackgroundType").Value;
            MaxWidth = (int)props.Item("MaxWidth").Value;
            MaxHeight = (int)props.Item("MaxHeight").Value;
            ExpandToFillIDE = (bool)props.Item("ExpandToFillIDE").Value;
        }

        public void Load()
        {
            var _DTE2 = (DTE2)ServiceProvider.GetService(typeof(DTE));
            var properties = _DTE2.Properties["VisualStudioBackground", "General"];

            Load(properties);
        }

        public void OnApplyChanged()
        {
            try
            {
                Load();
                OnChanged?.RaiseEvent(this, EventArgs.Empty);
            } catch
            {
                // nothing for now TODO: just incase theres an instance where I need to catch an exception
            }
        }

        public static string ToFullPath(string path, string defaultPath)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }

            var assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(string.IsNullOrEmpty(assemblyLocation) ? "" : assemblyLocation, path);
            }

            return path;
        }
    }

    [CLSCompliant(false), ComVisible(true)]
    [Guid("")]
    public enum PositionV
    {
        Top,
        Bottom,
        Center
    }

    [CLSCompliant(false), ComVisible(true)]
    [Guid("")]
    public enum PositionH
    {
        Left,
        Right,
        Center
    }

    [CLSCompliant(false), ComVisible(true)]
    [Guid("")]
    public enum ImageBackgroundType
    {
        Single = 0,
    }

    [CLSCompliant(false), ComVisible(true)]
    [Guid("")]
    public enum ImageStretch
    {
        None = 0,
        Uniform = 1,
        UniformToFill = 2,
        Fill = 3
    }

    public static class ImageStretchConverter
    {
        public static System.Windows.Media.Stretch ConvertTo(this ImageStretch source)
        {
            switch (source)
            {
                case ImageStretch.Fill:
                    return System.Windows.Media.Stretch.Fill;

                case ImageStretch.None:
                    return System.Windows.Media.Stretch.None;

                case ImageStretch.Uniform:
                    return System.Windows.Media.Stretch.Uniform;

                case ImageStretch.UniformToFill:
                    return System.Windows.Media.Stretch.UniformToFill;
            }

            return System.Windows.Media.Stretch.None;
        }
    }

    public static class PositionConverter
    {
        public static System.Windows.Media.AlignmentY ConvertTo(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return System.Windows.Media.AlignmentY.Bottom;

                case PositionV.Center:
                    return System.Windows.Media.AlignmentY.Center;

                case PositionV.Top:
                    return System.Windows.Media.AlignmentY.Top;
            }

            return System.Windows.Media.AlignmentY.Bottom; // default
        }

        public static System.Windows.VerticalAlignment ConvertToVerticalAlignment(this PositionV source)
        {
            switch (source)
            {
                case PositionV.Bottom:
                    return System.Windows.VerticalAlignment.Bottom;

                case PositionV.Center:
                    return System.Windows.VerticalAlignment.Center;

                case PositionV.Top:
                    return System.Windows.VerticalAlignment.Top;
            }

            return System.Windows.VerticalAlignment.Bottom; // default
        }

        public static System.Windows.Media.AlignmentX ConvertTo(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return System.Windows.Media.AlignmentX.Left;

                case PositionH.Center:
                    return System.Windows.Media.AlignmentX.Center;

                case PositionH.Right:
                    return System.Windows.Media.AlignmentX.Right;
            }

            return System.Windows.Media.AlignmentX.Right; // default
        }

        public static System.Windows.HorizontalAlignment ConvertToHorizontalAlignment(this PositionH source)
        {
            switch (source)
            {
                case PositionH.Left:
                    return System.Windows.HorizontalAlignment.Left;

                case PositionH.Center:
                    return System.Windows.HorizontalAlignment.Center;

                case PositionH.Right:
                    return System.Windows.HorizontalAlignment.Right;
            }

            return System.Windows.HorizontalAlignment.Right; // default
        }
    }
}
