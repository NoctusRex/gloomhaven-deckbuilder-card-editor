using GloomhavenDeckbuilder.CardEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GloomhavenDeckbuilder.CardEditor.Windows
{
    /// <summary>
    /// Interaction logic for CardEnhancementWindow.xaml
    /// </summary>
    public partial class CardEnhancementWindow : Window
    {
        private CardEnhancement Enhancement { get; set; } = new();

        public CardEnhancementWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> items = Enum.GetNames(typeof(AbilityLine)).ToList();
            AbilityLineComboBox.ItemsSource = items;
            AbilityLineComboBox.SelectedItem = items.First();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Enhancement.CanTargetAllies = CanTargetAlliesCheckBox.IsChecked.HasValue && CanTargetAlliesCheckBox.IsChecked.Value;
            Enhancement.CanTargetEnemies = CanTargetEnemiesCheckBox.IsChecked.HasValue && CanTargetEnemiesCheckBox.IsChecked.Value;
            Enhancement.IsNumeric = IsNumericCheckBox.IsChecked.HasValue && IsNumericCheckBox.IsChecked.Value;
            Enhancement.IsMovement = IsMovementCheckbox.IsChecked.HasValue && IsMovementCheckbox.IsChecked.Value;
            Enhancement.AbilityLine = (AbilityLine)Enum.Parse(typeof(AbilityLine), (string)AbilityLineComboBox.SelectedItem);

            DialogResult = true;
            Close();
        }

        public CardEnhancement? GetEnhancement(int x, int y)
        {
            Enhancement = new() { X = x, Y = y };

            bool? result = ShowDialog();
            if (result.HasValue && result.Value) return Enhancement;

            return null;
        }

        private void AbilityLineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;

            switch ((AbilityLine)Enum.Parse(typeof(AbilityLine), (string)AbilityLineComboBox.SelectedItem))
            {
                case AbilityLine.Hex:
                case AbilityLine.Counter:
                    CanTargetAlliesCheckBox.IsChecked = false;
                    CanTargetAlliesCheckBox.IsEnabled = false;

                    CanTargetEnemiesCheckBox.IsChecked = false;
                    CanTargetEnemiesCheckBox.IsEnabled = false;

                    IsNumericCheckBox.IsChecked = false;
                    IsNumericCheckBox.IsEnabled = false;

                    IsMovementCheckbox.IsChecked = false;
                    IsMovementCheckbox.IsEnabled = false;
                    break;

                case AbilityLine.Summon:
                    CanTargetAlliesCheckBox.IsChecked = false;
                    CanTargetAlliesCheckBox.IsEnabled = false;

                    CanTargetEnemiesCheckBox.IsChecked = false;
                    CanTargetEnemiesCheckBox.IsEnabled = false;

                    IsNumericCheckBox.IsChecked = true;
                    IsNumericCheckBox.IsEnabled = false;

                    IsMovementCheckbox.IsChecked = false;
                    IsMovementCheckbox.IsEnabled = false;
                    break;

                default:
                    CanTargetAlliesCheckBox.IsEnabled = true;
                    CanTargetEnemiesCheckBox.IsEnabled = true;
                    IsNumericCheckBox.IsEnabled = true;
                    IsMovementCheckbox.IsEnabled = true;
                    break;
            }
        }
    }
}
