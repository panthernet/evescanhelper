using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EVEScanHelper.Classes;
using MahApps.Metro.Controls.Dialogs;

namespace EVEScanHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: INotifyPropertyChanged
    {
        private string _selectedSystem;
        private string _outputText;
        public const string VERSION = "1.0.1";
        private bool _skipProcessing;
        private string _timestampText;
        private SigDataItem _selectedSig;

        public ObservableCollection<SigDataItem> SigsList { get; } = new ObservableCollection<SigDataItem>();
        public ObservableCollection<string> SystemsList { get; }

        public SigDataItem SelectedSig
        {
            get => _selectedSig;
            set { _selectedSig = value;  OnPropertyChanged();}
        }

        public string SelectedSystem
        {
            get => _selectedSystem;
            set { _selectedSystem = value; OnPropertyChanged();
                OnSelectedSystemChanged();
            }
        }

        public string TimestampText
        {
            get => _timestampText;
            set { _timestampText = value; OnPropertyChanged(); }
        }

        public string OutputText
        {
            get => _outputText;
            set { _outputText = value; OnPropertyChanged(); }
        }

        public ICommand CopySigCommand { get; set; }
        public ICommand CopyRawCommand { get; set; }
        public ICommand AboutCommand { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Title = $"EVE Scan Helper {VERSION}";

            if(!SQLHelper.Prepare())
                Application.Current.Shutdown();

            SystemsList = new ObservableCollection<string>(SettingsManager.GetSystems().OrderBy(s => s));

            SelectedSystem = SystemsList.FirstOrDefault();
            
            CopySigCommand = new SimpleCommand(o => SelectedSig != null, o => Clipboard.SetText(SelectedSig.Number));
            CopyRawCommand = new SimpleCommand(o => dataGrid.SelectedItems.Count > 0, o =>
            {
                var res = new StringBuilder();
                foreach (SigDataItem item in dataGrid.SelectedItems)
                {
                    res.Append(item.RawData);
                    res.Append("\n");
                }
                Clipboard.SetText(res.ToString());
            });

            AboutCommand = new SimpleCommand(async o =>
                {
                   // await this.ShowMessageAsync("About",
                   //     $"EVE Scan Helper v{VERSION}\nCoded by PantheR `panthernet software`\n\nISK donations are welcome to Duke Veldspar character!");
                    var dlg = new AboutDialog();
                    dlg.CloseButton.Click += async (sender, args) => await this.HideMetroDialogAsync(dlg);
                    await this.ShowMetroDialogAsync(dlg);
                });
            
        }

        private void StyleOutput(Color color, int size, bool isBold)
        {
            tbOutput.Foreground = new SolidColorBrush(color);
            tbOutput.FontSize = size;
            tbOutput.FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal;
        }

        private void ProcessInputData(string text)
        {
            if(_skipProcessing || string.IsNullOrEmpty(text)) return;
            StyleOutput(Colors.Black, 14, false);
            SigsList.Clear();
            if (!IsDataValid(text))
            {
                StyleOutput(Colors.Red, 20, true);
                OutputText = "ERROR: Input text has invalid format!";
                return;
            }

            var value = GetValueFromDatabase(SelectedSystem);
            if (string.IsNullOrEmpty(value))
            {
                StyleOutput(Colors.Green, 20, false);
                SQLHelper.InsertOrUpdate("sigs", new Dictionary<string, object> { {"system", SelectedSystem}, {"value", text}});
                OutputText = "New system data accepted!";
                ReloadTimestamp(SelectedSystem);
                UpdateSigsList(text);
                return;
            }

            var oldData = ParseData(value);
            var newData = ParseData(text);
            var newItems = newData.Where(a => oldData.All(b => b != a)).ToList();
            OutputText = newItems.Count == 0 ? "Where are NO new signatures" : $"New sigs:\n{string.Join("\n", newItems)}";

            SQLHelper.InsertOrUpdate("sigs", new Dictionary<string, object> { {"system", SelectedSystem}, {"value", text}}); 
            ReloadTimestamp(SelectedSystem);
            UpdateSigsList(text);
        }

        private void UpdateSigsList(string text)
        {
            if(string.IsNullOrEmpty(text)) return;
            SigsList.Clear();
            foreach (var item in text.Split('\n').Select(a =>
            {
                var data = a.Split('\t');
                return new SigDataItem
                {
                    Number = data[0],
                    CosmicType = data[1],
                    TypeName = data[2],
                    Name = data[3],
                    RawData = a
                };
            }))
            {
                SigsList.Add(item);
            }
        }

        private string GetValueFromDatabase(string system)
        {
            return SQLHelper.SQLiteDataQuery<string>("sigs", "value", "system", system);
        }

        private void ReloadTimestamp(object system)
        {
            var res = SQLHelper.SQLiteDataQuery<DateTime>("sigs", "lastUpdate", "system", system);
            TimestampText = $"Last Update: {(res == DateTime.MinValue ? "-" : res.ToString("dd.MM.yyyy hh:mm"))}";
        }

        private static List<string> ParseData(string value)
        {
            return value.Split('\n').Select(a => a.Split('\t')[0]).ToList();
        }

        private bool IsDataValid(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return false;
                var lines = value.Split('\n');
                if (lines.Any(a => a.Split('\t').Length != 6)) return false;
                if (!lines.All(a => a.Split('\t')[0].Contains("-"))) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UploadButton_OnClick(object sender, RoutedEventArgs e)
        {
            //ProcessInputData();
            ProcessInputData(Clipboard.GetText());
        }

        private void OnSelectedSystemChanged()
        {
           // _skipProcessing = true;
            OutputText = null;
            ProcessInputData(GetValueFromDatabase(_selectedSystem));
            _skipProcessing = false;
        }
    }
}
