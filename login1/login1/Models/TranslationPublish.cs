namespace TranslationService.Models
{
    public class TranslationPublish
    {
        public int Id { get; set; }

        public string Version { get; set; }

        public DateTime PublishedAt { get; set; }

        public string PublishedBy { get; set; }

        public int FileCount { get; set; }
    }
}
