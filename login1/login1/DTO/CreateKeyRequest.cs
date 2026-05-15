namespace TranslationService.DTO
{
    public class CreateKeyRequest
    {
        public string KeyName { get; set; }
        public string OriginalText { get; set; } // ✅ NEW
        public int ProjectId { get; set; }       // ✅ simplified
    }
}
