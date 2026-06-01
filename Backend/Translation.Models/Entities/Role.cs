namespace Translation.Models.Entities
{
    // Role assigned to users for authorization.
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Admin, Translator, Creator, Viewer
    }
}