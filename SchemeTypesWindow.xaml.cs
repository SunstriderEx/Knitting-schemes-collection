using LiteDB;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Вязание.Сборник_схем
{
    /// <summary>
    /// Логика взаимодействия для SchemeTypesWindow.xaml
    /// </summary>
    public partial class SchemeTypesWindow : Window
    {
        private DatabaseManager databaseManager;

        public SchemeTypesWindow(DatabaseManager databaseManager)
        {
            InitializeComponent();

            this.databaseManager = databaseManager;
            UpdateSchemeTypesList();
        }

        private void UpdateSchemeTypesList()
        {
            schemeTypesListBox.Items.Clear();

            var collection = databaseManager.schemeTypesCollection.FindAll().OrderBy(x => x.Name);
            foreach (var type in collection)
            {
                schemeTypesListBox.Items.Add(type.Name);
            }
        }

        private void AddTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (addTypeTextBox.Text.Length <= 1)
            {
                MessageBox.Show(this, "Название типа изделия не может быть пустым", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (databaseManager.schemeTypesCollection.FindOne(Query.EQ("Name", addTypeTextBox.Text)) != null)
            {
                MessageBox.Show(this, "Такой тип изделия уже имеется", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newType = new SchemeType
            {
                Name = addTypeTextBox.Text
            };
            databaseManager.schemeTypesCollection.Insert(newType);
            databaseManager.schemeTypesCollection.EnsureIndex(x => x.Name);

            addTypeTextBox.Text = "";
            UpdateSchemeTypesList();
        }

        private void UpdateTypeButton_Click(object sender, RoutedEventArgs e)
        {
            var schemeType = databaseManager.schemeTypesCollection.FindOne(Query.EQ("Name", schemeTypesListBox.SelectedItem.ToString()));
            var oldSchemeTypeName = schemeType.Name;
            schemeType.Name = updateTypeTextBox.Text;
            databaseManager.schemeTypesCollection.Update(schemeType);
            databaseManager.schemeTypesCollection.EnsureIndex(x => x.Name);

            // у всех схем этого исходного типа нужно обновить его название
            var schemesOfUpdatedType = databaseManager.schemesCollection.Find(Query.EQ("TypeName", oldSchemeTypeName));
            foreach (var scheme in schemesOfUpdatedType)
            {
                scheme.TypeName = schemeType.Name;
                databaseManager.schemesCollection.Update(scheme);
            }
            databaseManager.schemesCollection.EnsureIndex(x => x.TypeName);

            UpdateSchemeTypesList();
        }

        private void DeleteTypeButton_Click(object sender, RoutedEventArgs e)
        {
            var schemeType = databaseManager.schemeTypesCollection.FindOne(Query.EQ("Name", schemeTypesListBox.SelectedItem.ToString()));

            // если существуют схемы этого типа, сначала их нужно перевести в другой тип или удалить
            var schemesOfDeletingType = databaseManager.schemesCollection.Find(Query.EQ("TypeName", schemeType.Name));
            if (schemesOfDeletingType.Count() > 0)
                MessageBox.Show(this, "Нельзя удалить Тип изделия, содержащий схемы. Сначала их нужно переместить в другой тип иди удалить",
                    "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                databaseManager.schemeTypesCollection.Delete(schemeType.Id);

            UpdateSchemeTypesList();
        }

        private void AddTypeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter
                && addTypeTextBox.Text.Length > 0)
            {
                AddTypeButton_Click(this, new RoutedEventArgs());
                //schemeTypesListBox.Focus();
            }
        }

        private void UpdateTypeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter
                && updateTypeTextBox.Text.Length > 0)
            {
                UpdateTypeButton_Click(this, new RoutedEventArgs());
                //schemeTypesListBox.Focus();
            }
        }

        private void SchemeTypesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var editingControlsState = schemeTypesListBox.SelectedItem != null;
            updateTypeButton.IsEnabled = editingControlsState;
            updateTypeTextBox.IsEnabled = editingControlsState;
            updateTypeTextBox.Text = editingControlsState ? schemeTypesListBox.SelectedItem.ToString() : "";
            deleteTypeButton.IsEnabled = editingControlsState;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((AddSchemeWindow)Owner).UpdateTypesList();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
