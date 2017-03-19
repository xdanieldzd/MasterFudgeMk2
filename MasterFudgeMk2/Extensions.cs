using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

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
    }
}
