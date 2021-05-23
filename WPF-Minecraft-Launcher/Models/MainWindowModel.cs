using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Minecraft_Launcher.Models
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private string _UserName = "";
        private string _UserPassword = "";
        private string _Logs = "";
        private int _FileChangeMinimum = 0;
        private int _FileChangeMaximum = 100;
        private int _FileChangeValue = 0;
        private string _FileChangeText = "";
        private int _ProgressChangeMinimum = 0;
        private int _ProgressChangeMaximum = 100;
        private int _ProgressChangeValue = 0;
        private string _ProgressChangeText = "";

        internal string UserName
        {
            get { return _UserName; }
            set
            { 
                _UserName = value;
                OnPropertyChanged();
            }
        }

        internal string UserPassword
        {
            get { return _UserPassword; }
            set
            {
                _UserPassword = value;
                OnPropertyChanged();
            }
        }

        internal string Logs
        {
            get { return _Logs; }
            set
            {
                _Logs = value;
                OnPropertyChanged();
            }
        }

        internal int FileChangeMinimum
        {
            get { return _FileChangeMinimum; }
            set
            {
                _FileChangeMinimum = value;
                OnPropertyChanged();
            }
        }

        internal int FileChangeMaximum
        {
            get { return _FileChangeMaximum; }
            set
            {
                _FileChangeMaximum = value;
                OnPropertyChanged();
            }
        }

        internal int FileChangeValue
        {
            get { return _FileChangeValue; }
            set
            {
                _FileChangeValue = value;
                OnPropertyChanged();
            }
        }

        internal string FileChangeText
        {
            get { return _FileChangeText; }
            set
            {
                _FileChangeText = value;
                OnPropertyChanged();
            }
        }

        internal int ProgressChangeMinimum
        {
            get { return _ProgressChangeMinimum; }
            set
            {
                _ProgressChangeMinimum = value;
                OnPropertyChanged();
            }
        }

        internal int ProgressChangeMaximum
        {
            get { return _ProgressChangeMaximum; }
            set
            {
                _ProgressChangeMaximum = value;
                OnPropertyChanged();
            }
        }

        internal int ProgressChangeValue
        {
            get { return _ProgressChangeValue; }
            set
            {
                _ProgressChangeValue = value;
                OnPropertyChanged();
            }
        }

        internal string ProgressChangeText
        {
            get { return _ProgressChangeText; }
            set
            {
                _ProgressChangeText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
