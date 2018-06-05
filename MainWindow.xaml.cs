using LiteDB;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
                && scheme.HyperlinkToSource.Length >= "http://#.##".Length)
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

            var imageUri = "";
            if (scheme.HasLocalFiles()) // путь к файлам указан
            {
                openFileBtn.ToolTip = scheme.FilesPath;

                if (schemeInterface.SchemeFileExist())
                    imageUri = schemeInterface.GetIconUriByFileExtension(scheme.FilesPath);
                else
                    imageUri = "images\\warning.png";
            }
            else // только источник в интернете, т.е. без локальных файлов
            {
                openFileBtn.ToolTip = scheme.HyperlinkToSource + " (только интернет-источник)";

                imageUri = schemeInterface.GetIconUriBySiteUrl(scheme.HyperlinkToSource);
            }
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
            var scheme = schemeInterface.GetSelectedScheme();
            if (scheme.HasLocalFiles()) // путь к файлам указан
            {
                var filePath = schemeInterface.GetSchemeFilePath();
                if (schemeInterface.SchemeFileExist())
                    System.Diagnostics.Process.Start(filePath);
                else
                    MessageBox.Show(this, "Файл не найден!\n\nОтредактируйте схему и укажите верный путь к файлу с описанием",
                        "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else // только источник в интернете, т.е. без локальных файлов
            {
                HyperlinkToSource_Click(this, new RoutedEventArgs());
            }
            
        }

        private void AboutAppButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var aboutAppWnd = new AboutApp();
            aboutAppWnd.Owner = this;
            aboutAppWnd.ShowDialog();
        }
    }
}
