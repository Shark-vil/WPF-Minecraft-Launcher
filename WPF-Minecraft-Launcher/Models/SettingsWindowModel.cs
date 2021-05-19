using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WPF_Minecraft_Launcher.Components;

namespace WPF_Minecraft_Launcher.Models
{
    public class SettingsWindowModel : INotifyPropertyChanged
    {
        public int _MaxRAM = Global.LauncherConfig.MaxRAM;

        public int MaxRAM
        {
            get { return _MaxRAM; }
            set
            {
                _MaxRAM = Global.LauncherConfig.MaxRAM = value;
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
