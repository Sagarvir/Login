namespace Translation.Contracts.DTO.Translation
{
    // Request payload for creating multiple translation keys.
    public class CreateKeysRequest
    {
        public List<CreateKeyItem> Keys { get; set; }
    }

    // Item describing a single key to create in bulk.
    public class CreateKeyItem
    {
        public string KeyName { get; set; }
        public string OriginalText { get; set; }
        public int ProjectId { get; set; }
    }
}
