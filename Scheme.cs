namespace Вязание.Сборник_схем
{
    public class Scheme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HyperlinkToSource { get; set; }
        public string FilesPath { get; set; }
        public string PreviewImageId { get; set; }
        public string TypeName { get; set; }
    }

    public class SchemeType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
