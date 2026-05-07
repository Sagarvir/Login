namespace Backend_API_s.Models
{
    public class KeyTag
    {
        public int TranslationKeyId { get; set; }
        public TranslationKey TranslationKey { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
