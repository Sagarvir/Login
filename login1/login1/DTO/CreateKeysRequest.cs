namespace TranslationService.DTO
{
    public class CreateKeysRequest
    {
        public List<CreateKeyItem> Keys { get; set; }
    }

    public class CreateKeyItem
    {
        public string KeyName { get; set; }
        public string OriginalText { get; set; }
        public int ProjectId { get; set; }
    }
}
