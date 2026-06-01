namespace Translation.Contracts.DTO.Languages
{
    // Request payload to add or update a language entry.
    public class AddLanguage
    {
        public int id { get; set; }
        public string? code { get; set; }

        public string? name { get; set; }

    }
}
