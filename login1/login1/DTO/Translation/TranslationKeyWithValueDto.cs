namespace TranslationService.DTO.Translation
{
    public class TranslationKeyWithValueDto
    {
        public int KeyId { get; set; }
        public string Key { get; set; }
        public string OriginalText { get; set; }
        public int ProjectId { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
