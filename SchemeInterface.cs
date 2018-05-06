using LiteDB;
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
    }
}
