namespace login1.Models;

public class Language
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    public List<TranslationValue> TranslationValues { get; set; } = new();
}
