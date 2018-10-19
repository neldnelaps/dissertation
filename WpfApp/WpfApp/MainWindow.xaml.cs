using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
namespace WpfApp
{
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowCNF windowCnf = new WindowCNF(listBoxViewFiles.SelectedItem.ToString());
            windowCnf.Show();
        }
    }

    class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> ListBoxItemcColllections { get; set; }       

        private string selectedItem { get; set; }

        public string SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                NotifyPropertyChanged("SelectedItem");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewModel()
        {
            DirectoryInfo info = new System.IO.DirectoryInfo(@"..\..\Blif\");
            DirectoryInfo[] dirs = info.GetDirectories();
            FileInfo[] files = info.GetFiles();

            ListBoxItemcColllections = new ObservableCollection<string>();
            for (int i = 0; i < files.Length; i++)
                ListBoxItemcColllections.Add(files[i].Name.Remove(files[i].Name.Length - 5));
        }
    }
}
