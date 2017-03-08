using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MasterFudgeMk2.Common
{
    /* http://stackoverflow.com/a/36972187 */
    class UiStateBoolean : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool state;

        public bool IsTrue
        {
            get { return state; }
            set { state = value; OnPropertyChanged(); }
        }

        public bool IsFalse
        {
            get { return !state; }
            set { state = !value; OnPropertyChanged(); }
        }

        public UiStateBoolean(bool state)
        {
            IsTrue = state;
        }

        public void Toggle()
        {
            IsTrue = !IsTrue;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
