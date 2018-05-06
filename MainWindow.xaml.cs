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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Вязание.Сборник_схем
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SchemeInterface schemeInterface;

        private DatabaseManager databaseManager;

        public MainWindow()
        {
            InitializeComponent();

            databaseManager = new DatabaseManager();
            schemeInterface = new SchemeInterface(schemesTreeView, databaseManager);

            UpdateSchemesList();
        }

        public void UpdateSchemesList(Scheme schemeToSelect)
        {
            schemesTreeView.Items.Clear();

            var schemeTypes = databaseManager.schemeTypesCollection.FindAll().OrderBy(x => x.Name);
            foreach (var schemeType in schemeTypes)
            {
                var schemeTypeItem = new TreeViewItem
                {
                    Tag = schemeType.Name
                };
                schemeTypeItem.Selected += SchemeTypeItem_Selected;

                schemesTreeView.Items.Add(schemeTypeItem);

                var schemeTypeLabel = new Label()
                {
                    Content = schemeType.Name,
                    FontSize = 14
                };
                schemeTypeItem.Header = schemeTypeLabel;

                var schemes = databaseManager.schemesCollection.Find(Query.EQ("TypeName", schemeType.Name)).OrderBy(x => x.Name);
                foreach (var scheme in schemes)
                {
                    var schemeItem = new TreeViewItem
                    {
                        Tag = scheme.Name
                    };
                    schemeItem.Selected += SchemeItem_Selected;

                    var schemeLabel = new Label()
                    {
                        Content = scheme.Name,
                        FontSize = 14
                    };
                    schemeItem.Header = schemeLabel;

                    schemeTypeItem.Items.Add(schemeItem);

                    // раскроем дерево для выделения указанной схемы
                    if (schemeToSelect != null
                        && schemeToSelect.Name == scheme.Name)
                    {
                        schemeTypeItem.IsExpanded = true;
                        schemeItem.IsSelected = true;
                    }
                }
            }
        }

        public void UpdateSchemesList()
        {
            UpdateSchemesList(null);
        }

        private void AddSchemeBtn_Click(object sender, RoutedEventArgs e)
        {
            schemesTreeView.Focus(); // чтобы после закрытия окна фокус был на дереве для подсветки выбранного только что созданного элемента

            var addSchemeWnd = new AddSchemeWindow(databaseManager);
            addSchemeWnd.Owner = this;
            addSchemeWnd.ShowDialog();
        }

        private void EditSchemeBtn_Click(object sender, RoutedEventArgs e)
        {
            var scheme = schemeInterface.GetSelectedScheme();
            var addSchemeWnd = new AddSchemeWindow(databaseManager, scheme);
            addSchemeWnd.Owner = this;
            addSchemeWnd.ShowDialog();
        }

        private void RemoveSchemeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Действительно удалить данную схему?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.No)
                return;

            var scheme = schemeInterface.GetSelectedScheme();

            databaseManager.database.FileStorage.Delete(scheme.PreviewImageId);
            databaseManager.schemesCollection.Delete(Query.EQ("Name", scheme.Name));

            UpdateSchemesList();
            // выделим и развернём родительский для удаляемого элемент (Тип схемы)
            foreach (TreeViewItem item in schemesTreeView.Items)
            {
                if (item.Tag.ToString() == scheme.TypeName)
                {
                    item.IsSelected = true;
                    item.IsExpanded = true;
                    schemesTreeView.Focus();
                }
            }
        }

        private void SchemeTypeItem_Selected(object sender, RoutedEventArgs e)
        {
            // когда выбираешь вложенный элемент, ивент проходит и в родительском
            if (schemeInterface.GetSelectedSchemeType() == null)
                return;

            schemeName.Content = ((TreeViewItem)schemesTreeView.SelectedItem).Tag.ToString();//"Название схемы";
            hyperlinkToSource.Inlines.Clear();
            hyperlinkToSource.Inlines.Add("Ссылка на источник в Интернете");
            hyperlinkToSourceTextBlock.IsEnabled = false;
            openFileImage.Source = new BitmapImage(new Uri("images\\knitting.ico", UriKind.Relative));
            openFileImage.Opacity = 0.5f;
            openFileBtn.IsEnabled = false;
            previewImage.Source = new BitmapImage(new Uri("images\\preview.png", UriKind.Relative));
            previewImage.Opacity = 0.15f;
            editSchemeBtn.IsEnabled = false;
            removeSchemeBtn.IsEnabled = false;
        }

        private void SchemeItem_Selected(object sender, RoutedEventArgs e)
        {
            var scheme = schemeInterface.GetSelectedScheme();

            schemeName.Content = scheme.Name;
            hyperlinkToSource.Inlines.Clear();
            if (scheme.HyperlinkToSource != null
                && scheme.HyperlinkToSource.Length > "http://...".Length)
            {
                hyperlinkToSource.Inlines.Add("Ссылка на источник в Интернете");
                hyperlinkToSourceTextBlock.IsEnabled = true;
                hyperlinkToSourceTextBlock.ToolTip = scheme.HyperlinkToSource;
            }
            else
            {
                hyperlinkToSource.Inlines.Add("Ссылка на источник не указана");
                hyperlinkToSourceTextBlock.IsEnabled = false;
            }

            openFileBtn.IsEnabled = true;
            openFileBtn.ToolTip = scheme.FilesPath;

            var imageUri = "";
            if (SchemeFileExist())
                imageUri = GetIconUriByFileExtension(scheme.FilesPath);
            else
                imageUri = "images\\warning.png";
            openFileImage.Source = new BitmapImage(new Uri(imageUri, UriKind.Relative));
            openFileImage.Opacity = 1;

            previewImage.Source = databaseManager.GetImageSourceFromDb(scheme.PreviewImageId);

            previewImage.Opacity = 1;
            editSchemeBtn.IsEnabled = true;
            removeSchemeBtn.IsEnabled = true;
        }

        private void HyperlinkToSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(schemeInterface.GetSelectedScheme().HyperlinkToSource);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Неправильный адрес", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var filePath = GetSchemeFilePath();
            if (SchemeFileExist())
                System.Diagnostics.Process.Start(filePath);
            else
                MessageBox.Show(this, "Файл не найден!\n\nОтредактируйте схему и укажите верный путь к файлу с описанием",
                    "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void aboutAppButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var aboutAppWnd = new AboutApp();
            aboutAppWnd.Owner = this;
            aboutAppWnd.ShowDialog();
        }

        /// <summary>
        /// Выбирает иконку в зависимости от того, папка это или файл, а также его расширения
        /// </summary>
        /// <param name="filesPath">Путь к файлам, указанный пользователем при добавлении схемы</param>
        /// <returns>Путь (Uri) иконки</returns>
        private string GetIconUriByFileExtension(string filePath)
        {
            var imageUri = "";
            var attr = File.GetAttributes(filePath); // get the file attributes for file or directory
            if (attr.HasFlag(FileAttributes.Directory)) // директория
                imageUri = "images\\folder.png";
            else // файл
            {
                var tempParts = filePath.Split('.');
                var ext = tempParts[tempParts.Length - 1];
                switch (ext)
                {
                    case "doc":
                        imageUri = "images\\doc.png";
                        break;
                    case "docx":
                        imageUri = "images\\doc.png";
                        break;
                    case "rtf":
                        imageUri = "images\\doc.png";
                        break;
                    case "pdf":
                        imageUri = "images\\pdf.png";
                        break;
                    case "mp4":
                        imageUri = "images\\video.png";
                        break;
                    case "avi":
                        imageUri = "images\\video.png";
                        break;
                    case "mov":
                        imageUri = "images\\video.png";
                        break;
                    case "m4v":
                        imageUri = "images\\video.png";
                        break;
                    case "mkv":
                        imageUri = "images\\video.png";
                        break;
                    default:
                        imageUri = "images\\knitting.ico";
                        break;
                }
            }
            return imageUri;
        }

        /// <summary>
        /// Вычисляет абсолютный путь до файла или папки, указанных пользователем при добавлении схемы
        /// </summary>
        /// <returns></returns>
        private string GetSchemeFilePath()
        {
            var appUri = new Uri(System.Reflection.Assembly.GetEntryAssembly().Location);
            var fileUri = new Uri(appUri, schemeInterface.GetSelectedScheme().FilesPath); // Absolute path
            return fileUri.LocalPath;
        }

        /// <summary>
        /// Проверяет существует ли файл или папка, указанные пользователем при добавлении схемы
        /// </summary>
        private bool SchemeFileExist()
        {
            var filePath = GetSchemeFilePath();
            if (File.Exists(filePath) || Directory.Exists(filePath))
                return true;
            else
                return false;
        }
    }
}
