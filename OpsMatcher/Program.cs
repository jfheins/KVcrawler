using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OpsMatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IReadOnlyList<Mapping> records = Array.Empty<Mapping>();
            using (var reader = new StreamReader("mapping.csv"))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { DetectDelimiter = true, HasHeaderRecord = false }))
            {
                records = csv.GetRecords<Mapping>().ToList();
            }

            Console.WriteLine("Bitte Pfad zum Ops Katalog eingeben");
            var excelPath = @"E:\Temp\OPS-Kataloge 2008-2021.xlsx"; //Console.ReadLine();

            Console.WriteLine("Datei lädt ... bitte warten ...");
            using (var excel = new XLWorkbook(excelPath, XLEventTracking.Disabled))
            {
                Console.WriteLine("Bitte Blattnamen eingeben");
                var sheetName = Console.ReadLine();
                Console.WriteLine("Bitte Codespalte eingeben. (i)");
                var codeColumn = Console.ReadLine();
                Console.WriteLine("Bitte Zielspalte eingeben");
                var destColumn = Console.ReadLine();
                foreach (var row in excel.Worksheet(sheetName).RowsUsed(XLCellsUsedOptions.Contents))
                {
                    var code = row.Cell(codeColumn).GetValue<string>();
                    var matchingLg = records.Where(it => it.Matches(code)).Select(it => it.LgId);
                    row.Cell(destColumn).SetValue(string.Join(", ", matchingLg.Distinct().OrderBy(x => x)));
                }
                Console.WriteLine("wird gespeichert ...");
                excel.Save();
            }

            Console.WriteLine("done :-)");
        }
    }

    public record Mapping(string OpsMatchExpression, string LgId)
    {
        private Regex _pattern = MakePattern(OpsMatchExpression);

        public bool Matches(string ops) => _pattern.IsMatch(ops);

        private static Regex MakePattern(string s)
        {
            var regex = s.Replace(".", "\\.").Replace("?", "\\?").Replace("%", ".*");
            return new Regex(regex, RegexOptions.CultureInvariant);
        }
    }
}