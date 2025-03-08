using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using Rocket.API;
using Rocket.Core.Plugins;
using SDG.Unturned;
using UnturnedCases.Services;
using Logger = Rocket.Core.Logging.Logger;

namespace UnturnedCases
{
    public class UnturnedCases : RocketPlugin<UnturnedCasesConfiguration>
    {
        public static UnturnedCases Instance { get; private set; }
        public JSONService JSONService { get; private set; }
        public static Case[] ConfigCases { get; private set; }

        protected override void Load()
        {
            Case[] cases = Configuration.Instance.Cases;
            Case invalidCase = cases.FirstOrDefault(c => c.Items == null || c.Items.Sum(item => item.Odds) != 100);
            if (invalidCase != null)
                throw new Exception($"The odds for case '{invalidCase.Name}' do not sum to 100.");

            ConfigCases = cases.OrderBy(c => c.Name).ToArray();
            Instance = this;

            EffectManager.onEffectButtonClicked += UIService.HandleUI;
            Level.onLevelLoaded += OnLevelLoaded;
            JSONService = gameObject.AddComponent<JSONService>();

            Logger.Log("UnturnedCases has been loaded!");
        }

        private void OnLevelLoaded(int level)
        {
            foreach (CaseItem caseItem in ConfigCases.SelectMany(c => c.Items))
                caseItem.ItemName = (Assets.find(EAssetType.ITEM, caseItem.ItemID) as ItemAsset).itemName;
        }

        protected override void Unload()
        {
            Instance = null;

            EffectManager.onEffectButtonClicked -= UIService.HandleUI;
            Level.onLevelLoaded -= OnLevelLoaded;
            Destroy(JSONService);

            Logger.Log("UnturnedCases has been unloaded!");
        }
    }
}
