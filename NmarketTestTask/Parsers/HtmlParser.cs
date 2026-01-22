using HtmlAgilityPack;
using NmarketTestTask.Config;
using NmarketTestTask.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace NmarketTestTask.Parsers
{
    public class HtmlParser : IParser
    {
        public IList<House> GetHouses(string path)
        {
            var houses = new List<House>();

            var doc = new HtmlDocument();
            doc.Load(path);

            var headerNode = doc.DocumentNode.SelectSingleNode(HtmlParserConfig.TheadNode);
            var headerCells = doc.DocumentNode.SelectNodes(HtmlParserConfig.ThNode);

            var rows = doc.DocumentNode.SelectNodes(HtmlParserConfig.TbodyNode) ??
                      doc.DocumentNode.SelectNodes(HtmlParserConfig.TrNode);

            if (rows == null)
            {
                return houses;
            }

            var dataRows = rows.Where(r =>
            {
                var thCells = r.SelectNodes(HtmlParserConfig.ThNode);
                return thCells == null || thCells.Count == 0;
            }).ToList();

            var housesDictionary = new Dictionary<string, House>();

            foreach (var row in dataRows)
            {
                var cells = row.SelectNodes(".//td");

                if (cells == null || cells.Count < 3)
                {
                    continue;
                }

                string houseNumber = GetDigitsOnly(cells[0].InnerText);
                string flatNumber = GetDigitsOnly(cells[1].InnerText);
                string priceText = cells[cells.Count - 1].InnerText;

                if (string.IsNullOrEmpty(houseNumber) || string.IsNullOrEmpty(flatNumber))
                {
                    continue;
                }

                string parsedPrice = ParsePrice(priceText);

                string houseKey = houseNumber;
                if (!housesDictionary.ContainsKey(houseKey))
                {
                    housesDictionary[houseKey] = new House
                    {
                        Name = $"Дом {houseKey}",
                        Flats = new List<Flat>()
                    };
                }

                housesDictionary[houseKey].Flats.Add(new Flat
                {
                    Number = flatNumber,
                    Price = parsedPrice
                });
            }

            houses = housesDictionary
                .OrderBy(kvp => int.Parse(kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();

            return houses;
        }

        private string GetDigitsOnly(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            string result = "";
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    result += c;
                }
            }
            return result;
        }

        private string ParsePrice(string priceText)
        {
            if (string.IsNullOrEmpty(priceText))
            {
                return "0";
            }

            priceText = priceText.Replace(HtmlParserConfig.Currency, "")
                                 .Replace(" ", "")
                                 .Replace(" ", "");

            string clean = "";

            foreach (char c in priceText)
            {
                if (char.IsDigit(c) || c == ',' || c == '.')
                {
                    clean += c;
                }
            }

            if (string.IsNullOrEmpty(clean))
            {
                return HtmlParserConfig.DefaultPrice;
            }

            clean = clean.Replace(",", ".");

            if (decimal.TryParse(clean, NumberStyles.Any,
                CultureInfo.InvariantCulture, out decimal price))
            {
                return price.ToString(CultureInfo.InvariantCulture);
            }

            return HtmlParserConfig.DefaultPrice;
        }
    }
}