namespace AFI.Config;

// account.json
public class CustomCategories
{
    ////// Required.
    public string fallback { get; set; }
    
    // Skip by default.
    public Dictionary<string, string>? mapping { get; set; }
}