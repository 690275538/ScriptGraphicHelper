using Avalonia.Media.Imaging;
using ReactiveUI;
using ScriptGraphicHelper.ViewModels.Core;
using ScriptGraphicHelper.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace ScriptGraphicHelper.Models
{
    public class TabItems<item> : ObservableCollection<TabItem>
    {
        public new void Add(TabItem item)
        {
            if (base.Count >= 8)
            {
                base.RemoveAt(0);
            }
            base.Add(item);

            int width = (int)((MainWindow.Instance.Width - 450) / (this.Count < 8 ? this.Count : 8));
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Width = width < 160 ? width : 160;
            }
        }
    }


    public class TabItem : INotifyPropertyChanged
    {
        private int width;
        public int Width
        {
            get { return width; }
            set
            {
                width = value;
                NotifyPropertyChanged("Width");
            }
        }

        public string Header { get; set; } = string.Empty;

        public Bitmap Img { get; set; }

        public ICommand? Command { get; set; }

        public TabItem(Bitmap img)
        {
            Header = DateTime.Now.ToString("HH-mm-ss");
            Img = img;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
