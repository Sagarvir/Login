namespace Backend_API_s.Models
{
    public class TranslationKey
    {
        public int Id { get; set; }

        public string Key { get; set; }           // LOGIN_BUTTON
        public string DefaultText { get; set; }   // "Login"

        public DateTime CreatedAt { get; set; }

        public ICollection<Translation>? Translations { get; set; }
        public ICollection<KeyTag>? KeyTags { get; set; }
    }
}
