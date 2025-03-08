using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Rocket.API;
using Steamworks;
using UnityEngine;
using Newtonsoft.Json;
using Logger = Rocket.Core.Logging.Logger;

namespace UnturnedCases.Services
{
    public class JSONService : MonoBehaviour
    {
        private UnturnedCases pluginInstance => UnturnedCases.Instance;
        private UnturnedCasesConfiguration configInstance => UnturnedCases.Instance.Configuration.Instance;
        public string PathToJsonFile { get; private set; }
        public List<PlayerData> PlayersData { get; private set; }

        void Awake()
        {
            PathToJsonFile = Path.Combine(pluginInstance.Directory, "cases.json");

            if (!File.Exists(PathToJsonFile))
            {
                File.Create(PathToJsonFile).Dispose();
                PlayersData = new List<PlayerData>();
                Save();
            }
            else
            {
                string dataText = File.ReadAllText(PathToJsonFile);
                PlayersData = JsonConvert.DeserializeObject<List<PlayerData>>(dataText);
            }
        }

        void OnDestroy()
        {
            Save();
        }

        public void Save()
        {
            string objData = JsonConvert.SerializeObject(PlayersData);
            File.WriteAllText(PathToJsonFile, objData);

            Logger.Log("Saving data to JSON file...");
        }

        public PlayerData GetOrCreatePlayer(CSteamID steamID)
        {
            PlayerData player = PlayersData.FirstOrDefault(x => x.SteamID == steamID);

            if (player == null)
            {
                player = new PlayerData(steamID);
                PlayersData.Add(player);
            }

            return player;
        }
    }

    public class PlayerData
    {
        public CSteamID SteamID { get; set; }
        public List<Case> Cases { get; set; }
        public Case CurrentlyUnboxing { get; set; }
        public CaseItem WinningItem { get; set; }

        public PlayerData(CSteamID steamID)
        {
            SteamID = steamID;
            Cases = new List<Case>();
            CurrentlyUnboxing = null;
            WinningItem = null;
        }

        public void AddCase(Case c)
        {
            Cases.Add(c);
            SortCases();
        }

        public void RemoveCase(Case c)
        {
            Cases.Remove(c);
        }

        public void SortCases()
        {
            Cases = Cases.OrderBy(c => c.Name).ToList();
        }
    }
    
    public class Case
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public CaseItem[] Items { get; set; }

        public CaseItem GenerateWinningItem()
        {
            int rnd = Random.Range(1, 101);
            int sum = 0;

            for (int i = 0; i < Items.Length; i++)
            {
                sum += Items[i].Odds;
                if (rnd <= sum)
                    return Items[i];
            }

            return null;
        }
    }

    public class CaseItem
    {
        public ushort ItemID { get; set; }
        public byte Odds { get; set; }

        [XmlIgnore]
        public string ItemName { get; set; }
    }
}