namespace Backend_API_s.Dtos
{
    public sealed class TranslationKeyCreateDto
    {
        public string Key { get; set; } = string.Empty;
        public string DefaultText { get; set; } = string.Empty;
    }
}
   