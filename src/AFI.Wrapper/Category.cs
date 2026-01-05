using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

// https://actualbudget.github.io/docs/developers/API/#transaction

namespace AFI.Wrapper;

public class CategoryGroup
{

    #region " Required "


    [JsonPropertyName("Name")] // Name, required
    public String? Name { get; set; }

    #endregion

    #region " Optional "

    [JsonPropertyName("id")] // id
    public Guid? Id { get; set; }

    [JsonPropertyName("is_income")] // Defaults to false
    public bool? IsIncome { get; set; }


    [JsonPropertyName("categories")]
    public Category[]? Categories { get; set; }

    #endregion

}
public class Category
{
    
    #region " Required "
    
    
    [JsonPropertyName("Name")] // Name, required
    public String Name { get; set; }

    [JsonPropertyName("group_id")] // group Id
    public Guid GroupId { get; set; }
    #endregion

    #region " Optional "

    [JsonPropertyName("id")] // id
    public Guid? Id { get; set; }
    
    [JsonPropertyName("is_income")] // Defaults to false
    public bool? IsIncome { get; set; }
    
    #endregion
    
}