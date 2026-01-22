using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using NmarketTestTask.Config;
using NmarketTestTask.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NmarketTestTask.Parsers
{
    public class ExcelParser : IParser
    {
        public IList<House> GetHouses(string path)
        {
            var houses = new List<House>();

            var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheets.First();

            var numberCells = sheet.Cells()
                            .Where(c => c.GetValue<string>().Contains(ExcelParserConfig.FlatMarker))
                            .ToList();

            houses.AddRange(FillHouses(sheet));

            #region Примеры использования библиотек

            //var cell = sheet.Cell(1, 1);
            //var row = cell.WorksheetRow().RowNumber();
            //var column = cell.WorksheetColumn().ColumnNumber();
            //var value = cell.GetValue<string>();
            //var cells = sheet.Cells().Where(c => !c.GetValue<string>().Equals("1")).ToList();

            #endregion

            return houses;
        }

        private static List<string> GetFlatsNumbers(List<IXLCell> numberCells)
        {
            var numbers = new List<string>();

            foreach (var cell in numberCells)
            {
                string cellValue = cell.GetValue<string>();

                string numberPart = cellValue.Replace(ExcelParserConfig.FlatMarker, "").Trim();

                if (int.TryParse(numberPart, out int number))
                {
                    numbers.Add(number.ToString());
                }
            }

            return numbers;
        }

        private static List<string> GetFlatsPrices(List<IXLCell> numberCells)
        {
            List<string> prices = new List<string>();

            foreach (var numberCell in numberCells)
            {
                var priceCell = numberCell.CellBelow();
                string priceString = priceCell.GetValue<string>();

                prices.Add(priceString);

                priceString = priceString.Replace(" ", "")
                                         .Replace(",", ".");

                if (decimal.TryParse(priceString, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out decimal price))
                {
                    if (price == 0)
                    {
                        prices.Add("0");
                    }
                    else
                    {
                        prices.Add(price.ToString());
                    }
                }
            }

            return prices;
        }

        public IList<House> FillHouses(IXLWorksheet sheet)
        {
            var houses = new List<House>();

            var houseNameCells = sheet.Cells()
                .Where(c => c.GetValue<string>().StartsWith(ExcelParserConfig.HouseMarker))
                .OrderBy(c => c.Address.RowNumber)
                .ToList();

            for (int houseIndex = 0; houseIndex < houseNameCells.Count; houseIndex++)
            {
                var currentHouseCell = houseNameCells[houseIndex];
                int startRow = currentHouseCell.Address.RowNumber;
                int endRow = houseIndex < houseNameCells.Count - 1
                    ? houseNameCells[houseIndex + 1].Address.RowNumber - 1
                    : sheet.LastRowUsed().RowNumber();

                string houseName = currentHouseCell.GetValue<string>().Trim();
                
                var house = new House { 
                    Name = houseName,
                    Flats = new List<Flat>()
                };

                var numberCells = sheet.Range(startRow, 1, endRow, sheet.LastColumnUsed().ColumnNumber())
                    .Cells()
                    .Where(c => c.GetValue<string>().Contains(ExcelParserConfig.FlatMarker))
                    .ToList();

                var numbers = GetFlatsNumbers(numberCells);
                var prices = GetFlatsPrices(numberCells);

                for (int i = 0; i < numbers.Count; i++)
                {
                    house.Flats.Add(new Flat()
                    {
                        Number = numbers[i],
                        Price = prices[i]
                    });
                }

                houses.Add(house);
            }

            return houses;
        }
    }
}