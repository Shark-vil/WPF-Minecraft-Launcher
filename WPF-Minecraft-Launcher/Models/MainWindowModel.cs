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
        private string _username = "";
        private string _password = "";
        private string _logs = "";
        private int _fileChangedMin = 0;
        private int _fileChangedMax = 100;
        private int _fileChangedValue = 0;
        private string _fileChangedText = "";
        private bool _fileIsLoop = false;
        private int _progressChangedMin = 0;
        private int _progressChangedMax = 100;
        private int _progressChangedValue = 0;
        private string _progressChangedText = "";

        internal string username
        {
            get { return _username; }
            set
            { 
                _username = value;
                OnPropertyChanged();
            }
        }

        internal string password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        internal string logs
        {
            get { return _logs; }
            set
            {
                _logs = value;
                OnPropertyChanged();
            }
        }

        internal int fileChangedMin
        {
            get { return _fileChangedMin; }
            set
            {
                _fileChangedMin = value;
                OnPropertyChanged();
            }
        }

        internal int fileChangedMax
        {
            get { return _fileChangedMax; }
            set
            {
                _fileChangedMax = value;
                OnPropertyChanged();
            }
        }

        internal int fileChangedValue
        {
            get { return _fileChangedValue; }
            set
            {
                _fileChangedValue = value;
                OnPropertyChanged();
            }
        }

        internal string fileChangedText
        {
            get { return _fileChangedText; }
            set
            {
                _fileChangedText = value;
                OnPropertyChanged();
            }
        }

        internal bool fileIsLoop
        {
            get { return _fileIsLoop; }
            set
            {
                _fileIsLoop = value;
                OnPropertyChanged();
            }
        }

        internal int progressChangedMin
        {
            get { return _progressChangedMin; }
            set
            {
                _progressChangedMin = value;
                OnPropertyChanged();
            }
        }

        internal int progressChangedMax
        {
            get { return _progressChangedMax; }
            set
            {
                _progressChangedMax = value;
                OnPropertyChanged();
            }
        }

        internal int progressChangedValue
        {
            get { return _progressChangedValue; }
            set
            {
                _progressChangedValue = value;
                OnPropertyChanged();
            }
        }

        internal string progressChangedText
        {
            get { return _progressChangedText; }
            set
            {
                _progressChangedText = value;
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
