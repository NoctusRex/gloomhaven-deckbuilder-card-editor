using GloomhavenDeckbuilder.CardEditor.Models;
using GloomhavenDeckbuilder.CardEditor.Utils;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace GloomhavenDeckbuilder.CardEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Card Card { get; set; } = new();
        private List<Card> Cards { get; set; } = new List<Card>();
        private string Directory { get; set; } = string.Empty;
        private List<string> ImagePaths { get; set; } = new List<string>();
        private int CurrentImageIndex { get; set; }
        private string SaveFilePath => Path.Combine(Directory, $"{Directory.Split('\\').Last()}.json");
        private bool DoUpdateJson { get; set; } = true;

        public string CardTitle
        {
            get { return Card.Title; }
            set
            {
                Card.Title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardTitle)));
                UpdateJson();
            }
        }

        public string CardLevel
        {
            get
            {
                if (Card.Level is null) return "";

                return Card.Level.Value.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Card.Level = null;
                else
                    Card.Level = int.Parse(value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardLevel)));
                UpdateJson();
            }
        }

        public string CardInitiative
        {
            get
            {
                if (Card.Initiative is null) return "";

                return Card.Initiative.Value.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Card.Initiative = null;
                else
                    Card.Initiative = int.Parse(value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardInitiative)));
                UpdateJson();
            }
        }

        public string CardCounter
        {
            get
            {
                if (Card.Counter is null) return "";

                return Card.Counter.Value.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    Card.Counter = null;
                else
                    Card.Counter = int.Parse(value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardCounter)));
                UpdateJson();
            }
        }

        public bool CardLosable
        {
            get
            {
                return Card.Losable;
            }
            set
            {
                Card.Losable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CardLosable)));
                UpdateJson();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NextImageButton.IsEnabled = false;
            PreviousImageButton.IsEnabled = false;

            TitleTextBox.IsEnabled = false;
            LevelTextBox.IsEnabled = false;
            InitiativeTextBox.IsEnabled = false;
            CounterTextBox.IsEnabled = false;
            LosableCheckBox.IsEnabled = false;
        }

        private void UpdateAllSources()
        {
            UpdateSource(TitleTextBox);
            UpdateSource(LevelTextBox);
            UpdateSource(InitiativeTextBox);
            UpdateSource(CounterTextBox);
        }

        private static void UpdateSource(TextBox textBox) => textBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();

        private void UpdateJson()
        {
            if (!DoUpdateJson) return;
            if (string.IsNullOrEmpty(Directory)) return;
            File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(Cards, Formatting.Indented));
        }

        private void ResetForm()
        {
            DoUpdateJson = false;

            CardTitle = Card.Title;
            TitleTextBox.Text = CardTitle;

            CardCounter = Card.Counter.HasValue ? Card.Counter.Value.ToString() : "0";
            CounterTextBox.Text = CardCounter;

            CardLevel = Card.Level.HasValue ? Card.Level.Value.ToString() : "";
            LevelTextBox.Text = CardLevel;

            CardLosable = Card.Losable;
            LosableCheckBox.IsChecked = CardLosable;

            CardInitiative = Card.Initiative.HasValue ? Card.Initiative.Value.ToString() : "";
            InitiativeTextBox.Text = CardInitiative;

            DoUpdateJson = true;
        }

        private void OpenDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new()
            {
                Description = "Select Folder",
                ShowNewFolderButton = false,
                UseDescriptionForTitle = true
            };

            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Directory = dialog.SelectedPath;
                ImagePaths = System.IO.Directory.GetFiles(Directory, "*.png").ToList();
                LoadImage(0);
                NextImageButton.IsEnabled = true;
                TitleTextBox.IsEnabled = true;
                LevelTextBox.IsEnabled = true;
                InitiativeTextBox.IsEnabled = true;
                CounterTextBox.IsEnabled = true;
                LosableCheckBox.IsEnabled = true;
            }
        }

        private void NextImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImageIndex == ImagePaths.Count - 1)
            {
                NextImageButton.IsEnabled = false;
                return;
            }

            PreviousImageButton.IsEnabled = true;
            LoadImage(CurrentImageIndex + 1);

            LevelTextBox.Focus();
        }

        private void PreviousImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentImageIndex == 0)
            {
                PreviousImageButton.IsEnabled = false;
                return;
            }

            NextImageButton.IsEnabled = true;
            LoadImage(CurrentImageIndex - 1);
        }

        private void LoadImage(int index)
        {
            if (!ImagePaths.Any()) return;

            CurrentImageIndex = index;
            CardImage.Source = ImageUtils.BitmapToImageSource((Bitmap)System.Drawing.Image.FromFile(ImagePaths[index]));

            Card = new();
            ResetForm();

            if (!File.Exists(SaveFilePath)) UpdateJson(); // To create the file

            List<Card> cards = JsonConvert.DeserializeObject<List<Card>>(File.ReadAllText(SaveFilePath)) ?? new List<Card>();
            Cards = cards;

            Card? card = Cards.FirstOrDefault(x => x.ImgName == Path.GetFileNameWithoutExtension(ImagePaths[index]));
            if (card is null)
            {
                card = new();
                card.ImgName = Path.GetFileNameWithoutExtension(ImagePaths[index]);

                // example -> "gh-blood-pact" turns to "Blood Pact"
                card.Title = string.Join(" ", card.ImgName.Split('-').Skip(1).Select(x => x.First().ToString().ToUpper() + x[1..]));

                if (int.TryParse(OcrUtils.DoMagic(ImageUtils.CaptureArea(173, 297, 228 - 173, 342 - 297, (BitmapImage)CardImage.Source)), out int initiative))
                    card.Initiative = initiative;
                else
                    card.Initiative = null;

                Cards.Add(card);
            }

            Card = card;
            ResetForm();
            UpdateJson();

            UpdateAllSources();
        }
    }
}
