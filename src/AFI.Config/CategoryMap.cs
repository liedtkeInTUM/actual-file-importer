using System.Collections.Immutable;
using System.Text.Json;

namespace AFI.Config;

public class CategoryMap
{
    public IDictionary<string, string> Categories { get; }

    public CategoryMap(string path)
    {
        var file = Path.Join(path, "categories.json");
        if (File.Exists(file))
        {
            Console.WriteLine($"Loading categories from '{path}.'");
            var definition = File.ReadAllText(file);

            var categoryMapping = JsonSerializer.Deserialize<CustomCategories>(definition);
            if (categoryMapping != null)
            {
                //var mappingCategories = new Dictionary<string, string>();
                //foreach (var line in categories)
                //{
                //    mappingCategories.Add(line.Key, line.Value);
                //}
                //}

                //var directories = Directory.EnumerateDirectories(path);
                //foreach (var directory in directories)
                //{
                //    var info = BuildAccountInfo(directory);
                //    if (info != null)
                //    {
                //        accounts[directory] = info;
                //    }
                //}

                if (categoryMapping.mapping!.Count > 0)
                {
                    categoryMapping.mapping.Add("default", categoryMapping.fallback);
                    Categories = categoryMapping.mapping!.ToImmutableDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value);
                }
            }
            if (Categories == null)
            {
                Categories = new Dictionary<string, string>();
                Categories.Add("default", "default");
            }
        }
    }
}
