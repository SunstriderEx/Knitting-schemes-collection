using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Вязание.Сборник_схем
{
    /// <summary>
    /// Логика взаимодействия для AboutApp.xaml
    /// </summary>
    public partial class AboutApp : Window
    {
        public AboutApp()
        {
            InitializeComponent();
            
            aboutTextBlock.Inlines.Clear();

            aboutTextBlock.Inlines.Add(new Run("Вязание. Сборник схем") { FontWeight = FontWeights.SemiBold, FontSize = 18 });

            var ver = App.ResourceAssembly.GetName(false).Version;
            aboutTextBlock.Inlines.Add(new Run($"\nверсия {ver.Major}.{ver.Minor}") { Foreground = Brushes.Gray });

            aboutTextBlock.Inlines.Add("\n\nПрограмма для быстрого и удобного просмотра собственной коллекции схем по вязанию.\n\n" +
                "Это программное обеспечение лицензировано по ");

            var licenseHyperlink = new Hyperlink(new Run("GNU GPL v3.0"));
            licenseHyperlink.Click += LicenseHyperlink_Click;
            aboutTextBlock.Inlines.Add(licenseHyperlink);

            aboutTextBlock.Inlines.Add(".\nИсходный код доступен на ");

            var sourceCodeHyperlink = new Hyperlink(new Run("GitHub"));
            sourceCodeHyperlink.Click += SourceCodeHyperlink_Click;
            aboutTextBlock.Inlines.Add(sourceCodeHyperlink);

            aboutTextBlock.Inlines.Add(".\n\n© Георгий Карпов, 2018");
        }

        private void LicenseHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/SunstriderEx/Knitting-schemes-collection/blob/master/LICENSE");
        }

        private void SourceCodeHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/SunstriderEx/Knitting-schemes-collection");
        }
    }
}
