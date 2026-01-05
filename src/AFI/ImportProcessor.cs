using System.Globalization;
using AFI.Config;
using AFI.Wrapper;

namespace AFI;

internal class ImportProcessor : Processor
{
    private IDictionary<string, AccountInfo> Accounts { get; }
    private IDictionary<string, string> customCategoryMapping;
    private Actual Actual { get; }

    private Dictionary<string, Category> actualCategories;

    public ImportProcessor(AccountsInfo accounts, Actual actual, CategoryMap map)
    {
        Accounts = accounts.Accounts;
        Actual = actual;
        customCategoryMapping = map.Categories;
    }

    private EnumerationOptions CaseInsensitive { get; } = new() { MatchCasing = MatchCasing.CaseInsensitive };

    protected override void Process()
    {
        foreach (var kvp in Accounts)
        {
            ProcessAccount(kvp.Key, kvp.Value);
        }
    }

    private void ProcessAccount(string directory, AccountInfo account)
    {
        try
        {
            Console.WriteLine($"Checking online categories for assigning transactions."); 
            Category[] onlineCategories = Task.Run(async () => await Actual.GetCategories()).Result!;
            Console.WriteLine($"Found categories: '{onlineCategories.ToString()}'.");
            foreach (var category in onlineCategories)
            {
                actualCategories.Add(category.Name, category);
            }

            Console.WriteLine($"Checking account '{account.Account}' for files to import.");
            foreach (var file in Directory.EnumerateFiles(directory, "*.csv", CaseInsensitive))
            {
                ProcessAccountFile(file, account);
            }

            Console.WriteLine($"Done with account '{account.Account}.'");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void ProcessAccountFile(string file, AccountInfo account)
    {
        try
        {
            Console.WriteLine($"Processing file '{file}.'");
            var rows = File.ReadAllLines(file).Skip(account.HeaderRows!.Value);
            var transactions = new List<Transaction>();
            int indexForSubtransactions = 0;
            foreach (var row in rows)
            {
                var fields = row.Split(account.Delimiter);
                var transaction = new Transaction();

                if (account.DateColumn.HasValue)
                {
                    try
                    {
                        transaction.Date = DateTime.ParseExact(
                            fields[account.DateColumn!.Value],
                            account.DateFormat!,
                            CultureInfo.InvariantCulture
                        );
                    }
                    catch
                    {
                        // assume we have just parsed a subtransaction
                        continue;
                    }
                }

                if (account.PayeeColumn.HasValue)
                {
                    transaction.PayeeName = fields[account.PayeeColumn!.Value];
                }

                if (account.AmountColumn.HasValue)
                {
                    transaction.AmountInCents = Convert.ToInt32(
                        decimal.Parse(fields[account.AmountColumn!.Value]) * 100
                    );
                }

                if (account.CategoryColumn.HasValue)
                {
                    if (fields[account.CategoryColumn!.Value].Contains("--Splittbuchung--")) // german lexware finanzmanager extension
                    {
                        var subTransactions = new List<Transaction[]>();
                        var subtransactionfields = rows.ElementAt(indexForSubtransactions + 1).Split(account.Delimiter);
                        while (subtransactionfields[account.DateColumn!.Value].Count() == 0)
                        {
                            Transaction[] subtransaction = new Transaction[1];
                            subtransaction[0] = new Transaction();
                            subtransaction[0].Date = transaction.Date;
                            subtransaction[0].PayeeName = transaction.PayeeName;
                            subtransaction[0].AmountInCents = Convert.ToInt32(
                                decimal.Parse(subtransactionfields[account.AmountColumn!.Value]) * 100
                            );
                            if (account.CategoryColumn.HasValue)
                            {
                                subtransaction[0].Category = ConvertCategoryToGUID(subtransactionfields[account.CategoryColumn!.Value]);
                            }

                            if (account.TagColumn.HasValue)
                            {
                                if (!fields[account.TagColumn!.Value].Contains("--Splittbuchung--")) // german lexware finanzmanager extension
                                {
                                    subtransaction[0].Notes = subtransactionfields[account.TagColumn!.Value];
                                }
                            }

                            subTransactions.Add(subtransaction);

                            // check next element
                            indexForSubtransactions++;
                            subtransactionfields = rows.ElementAt(indexForSubtransactions + 1).Split(account.Delimiter);
                        }

                        transaction.SubTransactions = subTransactions.ToArray();

                    }
                    else
                    {
                        transaction.Category = ConvertCategoryToGUID(fields[account.CategoryColumn!.Value]);
                    }
                }

                if (account.TagColumn.HasValue)
                {
                    if (!fields[account.TagColumn!.Value].Contains("--Splittbuchung--")) // german lexware finanzmanager extension
                    {
                        transaction.Notes = fields[account.TagColumn!.Value];
                    }
                }

                transactions.Add(transaction);
                indexForSubtransactions++;
            }

            Actual.AddTransactions(account.Account!.Value, transactions).GetAwaiter().GetResult();
            File.Delete(file); // Done with this file.
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }


    private Guid? ConvertCategoryToGUID(string v)
    {
        Category match;
        string mappedCat;
        if (!customCategoryMapping.TryGetValue(v, out mappedCat))
        {
            mappedCat = customCategoryMapping["default"];
        }
        if (actualCategories.TryGetValue(mappedCat, out match))
        {
            return match.Id;
        }
        return null;
    }
}