using LiteDB;
using System;
using System.IO;
using System.Windows.Controls;

namespace Вязание.Сборник_схем
{
    public class SchemeInterface
    {
        private DatabaseManager databaseManager;
        private TreeView schemesTreeView;

        public SchemeInterface(TreeView schemesTreeView, DatabaseManager databaseManager)
        {
            this.schemesTreeView = schemesTreeView;
            this.databaseManager = databaseManager;
        }

        public Scheme GetSelectedScheme()
        {
            return databaseManager.schemesCollection.FindOne(Query.EQ("Name", ((TreeViewItem)schemesTreeView.SelectedItem).Tag.ToString()));
        }

        public SchemeType GetSelectedSchemeType()
        {
            return databaseManager.schemeTypesCollection.FindOne(Query.EQ("Name", ((TreeViewItem)schemesTreeView.SelectedItem).Tag.ToString()));
        }

        /// <summary>
        /// Вычисляет абсолютный путь до файла или папки, указанных пользователем при добавлении схемы
        /// </summary>
        /// <returns></returns>
        public string GetSchemeFilePath()
        {
            var appUri = new Uri(System.Reflection.Assembly.GetEntryAssembly().Location);
            var fileUri = new Uri(appUri, GetSelectedScheme().FilesPath); // Absolute path
            return fileUri.LocalPath;
        }

        /// <summary>
        /// Проверяет существует ли файл или папка, указанные пользователем при добавлении схемы
        /// </summary>
        public bool SchemeFileExist()
        {
            var filePath = GetSchemeFilePath();
            if (File.Exists(filePath) || Directory.Exists(filePath))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Выбирает иконку в зависимости от того, папка это или файл, а также его расширения
        /// </summary>
        /// <param name="filesPath">Путь к файлам, указанный пользователем при добавлении схемы</param>
        /// <returns>Путь (Uri) иконки</returns>
        public string GetIconUriByFileExtension(string filePath)
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
        /// Выбирает иконку в зависимости от сайта
        /// </summary>
        public string GetIconUriBySiteUrl(string url)
        {
            var imageUri = "";
            if (url.StartsWith("https://www.youtube.com/") || url.StartsWith("https://youtu.be/")) // YouTube
            {
                imageUri = "images\\youtube.png";
            }
            // else if (...) // other sites
            else // любой другой сайт
            {
                imageUri = "images\\internet.png";
            }
            return imageUri;
        }
    }
}
