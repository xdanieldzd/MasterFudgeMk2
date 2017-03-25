using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Reflection;

namespace MasterFudgeMk2
{
    /* http://stackoverflow.com/a/783986 */
    public static class Extensions
    {
        public static TResult InvokeIfRequired<TControl, TResult>(this TControl control, Func<TControl, TResult> func) where TControl : Control
        {
            return ((!control.IsDisposed && control.InvokeRequired) ? (TResult)control.Invoke(func, control) : func(control));
        }

        public static void InvokeIfRequired<TControl>(this TControl control, Action<TControl> func) where TControl : Control
        {
            control.InvokeIfRequired(c => { func(c); return c; });
        }

        public static void InvokeIfRequired<TControl>(this TControl control, Action action) where TControl : Control
        {
            control.InvokeIfRequired(c => action());
        }

        public static string GetFullyQualifiedName<TEnum>(this TEnum @enum)
        {
            return string.Format("{0}.{1}", @enum.GetType().FullName, @enum.ToString());
        }

        public static Enum GetEnumFromFullyQualifiedName(this string name)
        {
            if (name == null || name == string.Empty) return null;
            string @class = name.Substring(0, name.LastIndexOf('.'));
            string @enum = name.Substring(name.LastIndexOf('.') + 1);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith(@class.Substring(0, @class.IndexOf('.')))))
            {
                Type classType = assembly.GetType(@class);
                if (classType != null)
                    return (Enum)Enum.Parse(classType, @enum);
            }
            return null;
        }

        public static void SetCommonImageFilter(this FileDialog fileDialog, string defaultExtension)
        {
            List<string> separateFilters = new List<string>();

            List<ImageCodecInfo> codecs = ImageCodecInfo.GetImageEncoders().ToList();
            string imageExtensions = string.Join(";", codecs.Select(ici => ici.FilenameExtension));
            foreach (ImageCodecInfo codec in codecs)
                separateFilters.Add(string.Format("{0} Files ({1})|{1}", codec.FormatDescription, codec.FilenameExtension.ToLowerInvariant()));

            fileDialog.Filter = string.Format("{0}|Image Files ({1})|{1}|All Files (*.*)|*.*", string.Join("|", separateFilters), imageExtensions.ToLowerInvariant());

            if (defaultExtension != null)
                fileDialog.FilterIndex = (codecs.IndexOf(codecs.FirstOrDefault(x => x.FormatDescription.ToLowerInvariant().Contains(defaultExtension.ToLowerInvariant()))) + 1);
            else
                fileDialog.FilterIndex = (codecs.Count + 1);
        }

        public static IEnumerable<Type> GetImplementationsFromAssembly(this Type type)
        {
            if (!type.IsInterface) throw new Exception("Type is not interface");
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);
        }
    }
}
