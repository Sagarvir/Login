namespace Backend_API_s.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string TagName { get; set; }

        public ICollection<KeyTag> KeyTags { get; set; }
    }
}
