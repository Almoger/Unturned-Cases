using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnturnedCases.Services;

namespace UnturnedCases
{
    public class UnturnedCasesConfiguration : IRocketPluginConfiguration
    {
        [XmlArrayItem(ElementName = "Cases")]
        public Case[] Cases { get; set; }

        public void LoadDefaults()
        {
            Cases = new Case[]
            {
                new Case { Name = "Money Printers Case", Price = 30000, Items = new CaseItem[] {
                    new CaseItem { ItemID = 16800, Odds = 20 },
                    new CaseItem { ItemID = 16801, Odds = 20 },
                    new CaseItem { ItemID = 16802, Odds = 20 },
                    new CaseItem { ItemID = 16803, Odds = 20 },
                    new CaseItem { ItemID = 16810, Odds = 4 },
                    new CaseItem { ItemID = 16811, Odds = 4 },
                    new CaseItem { ItemID = 16812, Odds = 4 },
                    new CaseItem { ItemID = 16813, Odds = 4 },
                    new CaseItem { ItemID = 16814, Odds = 4 } }
                },
                new Case { Name = "Weapons Case #1", Price = 50000, Items = new CaseItem[] {
                    new CaseItem { ItemID = 4, Odds = 9 },
                    new CaseItem { ItemID = 18, Odds = 9 },
                    new CaseItem { ItemID = 97, Odds = 9 },
                    new CaseItem { ItemID = 99, Odds = 9 },
                    new CaseItem { ItemID = 101, Odds = 9 },
                    new CaseItem { ItemID = 107, Odds = 9 },
                    new CaseItem { ItemID = 109, Odds = 9 },
                    new CaseItem { ItemID = 112, Odds = 9 },
                    new CaseItem { ItemID = 116, Odds = 9 },
                    new CaseItem { ItemID = 132, Odds = 9 },
                    new CaseItem { ItemID = 300, Odds = 8 },
                    new CaseItem { ItemID = 1441, Odds = 2 } }
                },
                new Case { Name = "Weapons Case #2", Price = 30000, Items = new CaseItem[] {
                    new CaseItem { ItemID = 1379, Odds = 20 },
                    new CaseItem { ItemID = 1377, Odds = 20 },
                    new CaseItem { ItemID = 1382, Odds = 20 },
                    new CaseItem { ItemID = 1362, Odds = 20 },
                    new CaseItem { ItemID = 1382, Odds = 20 }} 
                }
            };
        }
    }
}