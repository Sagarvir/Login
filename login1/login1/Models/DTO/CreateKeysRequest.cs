namespace login1.Models.DTO
{
    public class CreateKeysRequest
    {
        public List<CreateKeyItem> Keys { get; set; } = new();
    }

    public class CreateKeyItem
    {
        public string KeyName { get; set; } = string.Empty;
        public string OriginalText { get; set; } = string.Empty;
        public int ProjectId { get; set; }
    }
}
