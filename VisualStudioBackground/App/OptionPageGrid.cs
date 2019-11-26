#region USING_DIRECTIVES

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using VisualStudioBackground.Localized;
using VisualStudioBackground.Settings;

#endregion USING_DIRECTIVES

namespace VisualStudioBackground.App
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    [Guid("44877e19-3fa9-497e-8cd3-f84bf19dfd8d")]
    public class OptionPageGrid : DialogPage
    {
        public OptionPageGrid()
        {
            BackgroundImageAbsolutePath = "Resources\\background.jpg";
            Opacity = 0.35;
            PositionHorizontal = PositionH.Right;
            PositionVertical = PositionV.Bottom;
            Extensions = ".png, .jpg";
            MaxWidth = 0;
            MaxHeight = 0;
            ImageStretch = ImageStretch.None;
            ExpandToFillIDE = false;
        }

        [LocalManager.LocalizedCategory("Image")]
        [LocalManager.LocalizedDisplayName("BackgroundType")]
        [LocalManager.LocalizedDescription("BackgroundTypeDesc")]
        [PropertyPageTypeConverter(typeof(ImageBackgroundTypeConverter))]
        [TypeConverter(typeof(ImageBackgroundTypeConverter))]
        public ImageBackgroundType ImageBackgroundType { get; set; }

        [LocalManager.LocalizedCategory("Image")]
        [LocalManager.LocalizedDisplayName("OpacityType")]
        [LocalManager.LocalizedDescription("OpacityTypeDesc")]
        public double Opacity { get; set; }

        [LocalManager.LocalizedCategory("Image")]
        [LocalManager.LocalizedDisplayName("FilePathType")]
        [LocalManager.LocalizedDescription("FilePathTypeDesc")]
        [Editor(typeof(BrowseFile), typeof(UITypeEditor))]
        public string BackgroundImageAbsolutePath { get; set; }

        [LocalManager.LocalizedCategory("Image")]
        [LocalManager.LocalizedDisplayName("ImageExtensionsType")]
        [LocalManager.LocalizedDescription("ImageExtensionsTypeDesc")]
        public string Extensions { get; set; }

        [LocalManager.LocalizedCategory("Layout")]
        [LocalManager.LocalizedDisplayName("ExpandToFillIDEType")]
        [LocalManager.LocalizedDescription("ExpandToFillIDETypeDesc")]
        public bool ExpandToFillIDE { get; set; }

        [LocalManager.LocalizedCategory("Layout")]
        [LocalManager.LocalizedDisplayName("HorizontalAlignmentType")]
        [LocalManager.LocalizedDescription("HorizontalAlignmentTypeDesc")]
        [PropertyPageTypeConverter(typeof(PositionHTypeConverter))]
        [TypeConverter(typeof(PositionHTypeConverter))]
        public PositionH PositionHorizontal { get; set; }

        [LocalManager.LocalizedCategory("Layout")]
        [LocalManager.LocalizedDisplayName("VerticalAlignmentType")]
        [LocalManager.LocalizedDescription("VerticalAlignmentTypeDesc")]
        [PropertyPageTypeConverter(typeof(PositionVTypeConverter))]
        [TypeConverter(typeof(PositionVTypeConverter))]
        public PositionV PositionVertical { get; set; }

        [LocalManager.LocalizedCategory("Layout")]
        [LocalManager.LocalizedDisplayName("ImageStretchType")]
        [LocalManager.LocalizedDescription("ImageStretchTypeDesc")]
        [PropertyPageTypeConverter(typeof(ImageStretchTypeConverter))]
        [TypeConverter(typeof(ImageStretchTypeConverter))]
        public ImageStretch ImageStretch { get; set; }

        [LocalManager.LocalizedCategory("Layout")]
        [LocalManager.LocalizedDisplayName("MaxHeightType")]
        [LocalManager.LocalizedDescription("MaxHeightTypeDesc")]
        public int MaxHeight { get; set; }

        [LocalManager.LocalizedCategory("Layout")]
        [LocalManager.LocalizedDisplayName("MaxWidthType")]
        [LocalManager.LocalizedDescription("MaxWidthTypeDesc")]
        public int MaxWidth { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            try
            {
                Setting.Instance.OnApplyChanged();
            } catch
            {
            }
            base.OnApply(e);
        }
    }

    public class ImageBackgroundTypeConverter : EnumConverter
    {
        public ImageBackgroundTypeConverter()
            : base(typeof(ImageBackgroundType))
        {
            // empty constructor
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;

            if (str != null)
            {
                if (str == "Single") return ImageBackgroundType.Single;

                return ImageBackgroundType.Single;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;

                if ((int)value == 0) { result = "Single"; }

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class PositionHTypeConverter : EnumConverter
    {
        public PositionHTypeConverter()
            : base(typeof(PositionH))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;

            if (str != null)
            {
                if (str == "Right") return PositionH.Right;
                if (str == "Left") return PositionH.Left;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;

                if ((int)value == 0) { result = "Left"; } else if ((int)value == 1) { result = "Right"; }

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class PositionVTypeConverter : EnumConverter
    {
        public PositionVTypeConverter()
            : base(typeof(PositionV))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;

            if (str != null)
            {
                if (str == "Top") return PositionV.Top;
                if (str == "Bottom") return PositionV.Bottom;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;

                if ((int)value == 0) { result = "Top"; } else if ((int)value == 1) { result = "Bottom"; }

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class ImageStretchTypeConverter : EnumConverter
    {
        public ImageStretchTypeConverter()
            : base(typeof(ImageStretch))
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;

            if (str != null)
            {
                if (str == "None") return ImageStretch.None;
                if (str == "Uniform") return ImageStretch.Uniform;
                if (str == "UniformToFill") return ImageStretch.UniformToFill;
                if (str == "Fill") return ImageStretch.Fill;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = null;

                if ((int)value == 0) { result = "None"; } else if ((int)value == 1) { result = "Uniform"; } else if ((int)value == 2) { result = "UniformToFill"; } else if ((int)value == 3) { result = "Fill"; }

                if (result != null) return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    // this might not be needed (BrowseDirectory for slide show images?) I just want static images
    public class BrowseDirectory : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null)
            {
                var open = new FolderBrowserDialog();
                if (open.ShowDialog() == DialogResult.OK) { return open.SelectedPath; }
            }

            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

    internal class BrowseFile : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null)
            {
                var open = new OpenFileDialog();
                open.FileName = Path.GetFileName((string)value);
                try
                {
                    open.InitialDirectory = Path.GetDirectoryName((string)value);
                } catch (Exception)
                {
                    // nothing to print
                }

                if (open.ShowDialog() == DialogResult.OK) { return open.FileName; }
            }

            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
