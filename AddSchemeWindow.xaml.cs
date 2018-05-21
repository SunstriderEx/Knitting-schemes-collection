using LiteDB;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Вязание.Сборник_схем
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class AddSchemeWindow : Window
    {
        private bool editingMode;
        private Scheme scheme;
        private DatabaseManager databaseManager;

        private string newPreviewImagePath;

        private bool noCloseEvent = false;

        public AddSchemeWindow(DatabaseManager databaseManager)
        {
            InitializeComponent();
            CommonInitialization(databaseManager);

            scheme = new Scheme();
        }

        public AddSchemeWindow(DatabaseManager databaseManager, Scheme scheme)
        {
            InitializeComponent();
            CommonInitialization(databaseManager);

            editingMode = true;
            this.scheme = scheme;

            schemeTypeComboBox.SelectedIndex = schemeTypeComboBox.Items.IndexOf(scheme.TypeName);
            schemeNameTextBox.Text = scheme.Name;
            hyperlinkToSourceTextBox.Text = scheme.HyperlinkToSource;
            filesPathTextBox.Text = scheme.FilesPath;

            previewImage.Source = databaseManager.GetImageSourceFromDb(scheme.PreviewImageId);

            okButton.Content = "Сохранить схему";
            previewImage.Opacity = 1;
        }

        private void CommonInitialization(DatabaseManager databaseManager)
        {
            this.databaseManager = databaseManager;
            UpdateTypesList();
        }

        public void UpdateTypesList()
        {
            schemeTypeComboBox.Items.Clear();

            var collection = databaseManager.schemeTypesCollection.FindAll().OrderBy(x => x.Name);
            foreach (var type in collection)
            {
                schemeTypeComboBox.Items.Add(type.Name);
            }
        }

        private void EditTypesCollection_Click(object sender, RoutedEventArgs e)
        {
            var schemeTypesWindow = new SchemeTypesWindow(databaseManager)
            {
                Owner = this
            };
            schemeTypesWindow.ShowDialog();
        }

        private void HyperlinkToSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(hyperlinkToSourceTextBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Неправильный адрес", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SpecifyFilesPathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog myDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Все файлы|*.*",
                CheckFileExists = true,
                Multiselect = false
            };
            if (myDialog.ShowDialog() == true)
            {
                //filesPathTextBox.Text = myDialog.FileName;

                //var folderOrFileMsg = MessageBox.Show(this, "Хотите выбрать папку, в которой находится данный файл?\n(Если нужен сам файл, то ответ 'Нет')",
                //    "Файл или папка?!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //if (folderOrFileMsg == MessageBoxResult.Yes)
                //{
                //    var pathParts = filesPathTextBox.Text.Split('\\');
                //    filesPathTextBox.Text = "";
                //    for (int i = 0; i < pathParts.Length - 1; i++)
                //        filesPathTextBox.Text += pathParts[i] + "\\";
                //}

                var fileUri = new Uri(myDialog.FileName);
                var appUri = new Uri(System.Reflection.Assembly.GetEntryAssembly().Location);
                filesPathTextBox.Text = appUri.MakeRelativeUri(fileUri).ToString();
            }
        }

        private void SpecifyPreviewImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog myDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.png;*.gif;*.bmp;*.jpeg)|*.jpg;*.png;*.gif;*.bmp;*.jpeg",
                CheckFileExists = true,
                Multiselect = false
            };
            if (myDialog.ShowDialog() == true)
            {
                newPreviewImagePath = myDialog.FileName;
                previewImage.Source = BitmapFromUri(new Uri(newPreviewImagePath)); //new BitmapImage(new Uri(newPreviewImagePath));
                previewImage.Opacity = 1;
            }
        }

        /// <summary>
        /// После использование изображения через "new BitmapImage(uri)" оно остаётся открытым и занятым текущим процессом,
        /// чтобы этого избежать необходимо добавить "bitmap.CacheOption = BitmapCacheOption.OnLoad;"
        /// https://stackoverflow.com/questions/10319447/release-handle-on-file-imagesource-from-bitmapimage
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (schemeTypeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(this, "Не выбран тип изделия", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (schemeNameTextBox.Text.Length <= 1
                || schemeNameTextBox.Text == "название схемы...")
            {
                MessageBox.Show(this, "Название схемы не может быть пустым", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (databaseManager.schemesCollection.FindOne(Query.EQ("Name", schemeNameTextBox.Text)) != null
                && !(editingMode && scheme.Name == schemeNameTextBox.Text)) // редактируем и имя осталось то же самое
            {
                MessageBox.Show(this, "Схема с таким названием уже имеется", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (databaseManager.schemeTypesCollection.FindOne(Query.EQ("Name", schemeNameTextBox.Text)) != null
                && !(editingMode && scheme.Name == schemeNameTextBox.Text)) // редактируем и имя осталось то же самое
            {
                MessageBox.Show(this, "Название схемы не может совпадать с Типом изделия", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (filesPathTextBox.Text.Length <= 1)
            {
                MessageBox.Show(this, "Не указан путь к файлу(-ам) с описанием", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (previewImage.Opacity < 1)
            {
                MessageBox.Show(this, "Не указано изображение в качестве иллюстрации", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            scheme.TypeName = schemeTypeComboBox.SelectedItem.ToString();
            scheme.Name = schemeNameTextBox.Text;
            scheme.HyperlinkToSource = hyperlinkToSourceTextBox.Text;
            scheme.FilesPath = filesPathTextBox.Text;
            if (newPreviewImagePath != null)
            {
                if (editingMode)
                    databaseManager.database.FileStorage.Delete(scheme.PreviewImageId); // удалим из хранилища старое изображение

                var typeName = scheme.TypeName.Replace("/", "");
                var schemeName = scheme.Name.Replace("/", "");
                var fileId = string.Format($"{typeName}/{schemeName}.{DateTime.Now.ToLongTimeString()}");
                fileId = new string((from c in fileId
                                     where (char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || c == '/')
                                     select c).ToArray());
                fileId = fileId.Replace(" ", "-");

                databaseManager.database.FileStorage.Upload(fileId, newPreviewImagePath);
                scheme.PreviewImageId = fileId;
            }

            if (editingMode)
                databaseManager.schemesCollection.Update(scheme);
            else
                databaseManager.schemesCollection.Insert(scheme);
            databaseManager.schemesCollection.EnsureIndex(x => x.Name);
            databaseManager.schemesCollection.EnsureIndex(x => x.TypeName);

            ((MainWindow)Owner).UpdateSchemesList(scheme);
            noCloseEvent = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Если окно было открыто, чтобы добавить Типы схем, а сама схема не создавалась,
        // то список с типами нужно обновить
        private void Window_Closed(object sender, EventArgs e)
        {
            if (noCloseEvent)
                return;

            var owner = (MainWindow)Owner;
            if (owner.schemesTreeView.SelectedItem != null)
                owner.UpdateSchemesList(owner.schemeInterface.GetSelectedScheme());
            else
                owner.UpdateSchemesList();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CancelButton_Click(this, new RoutedEventArgs());
        }
    }
}
