using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Вязание.Сборник_схем
{
    public class DatabaseManager
    {
        private const string DatabasePath = "schemes.db";

        public LiteDatabase database;
        public LiteCollection<Scheme> schemesCollection;
        public LiteCollection<SchemeType> schemeTypesCollection;

        public DatabaseManager()
        {
            database = new LiteDatabase(DatabasePath);

            schemesCollection = database.GetCollection<Scheme>("Schemes");
            schemeTypesCollection = database.GetCollection<SchemeType>("SchemeTypes");
        }

        public BitmapImage GetImageSourceFromDb(string liteStorageFileId)
        {
            var imageStream = new MemoryStream();
            database.FileStorage.FindById(liteStorageFileId).CopyTo(imageStream);
            imageStream.Position = 0;

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = imageStream;
            imageSource.EndInit();

            return imageSource;
        }
    }
}
