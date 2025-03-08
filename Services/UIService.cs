using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using fr34kyn01535.Uconomy;
using Logger = Rocket.Core.Logging.Logger;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Collections;

namespace UnturnedCases.Services
{
    public class UIService
    {
        private static UnturnedCases pluginInstance => UnturnedCases.Instance;
        private static Case[] StoreCases => UnturnedCases.ConfigCases;
        private static Dictionary<CSteamID, ViewingPages> playerPages = new Dictionary<CSteamID, ViewingPages>();
        private static string ActivityHistory = "\n";
        private static int CasesOpened = 0;
        private static int CasesBought = 0;

        public static void HandleUI(Player player, string buttonName)
        {
            PlayerData playerData = pluginInstance.JSONService.GetOrCreatePlayer(player.channel.owner.playerID.steamID);
            CSteamID cSteamID = playerData.SteamID;

            Match previewCaseMatch = Regex.Match(buttonName, @"sPreview(\d+)");
            Match buyButtonMatch = Regex.Match(buttonName, @"sButton(\d+)");
            Match useButtonMatch = Regex.Match(buttonName, @"^Button(\d+)$");

            if (buttonName == "CloseButton")
            {
                EffectManager.askEffectClearByID(28280, player.channel.owner.transportConnection);
                player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);

                playerPages.Remove(cSteamID);
            }
            else if (buttonName == "HomeButton")
            {
                DrawHome(player, playerData);
            }
            else if (buttonName == "InventoryButton")
            {
                playerPages[cSteamID].IventoryPage = 0;
                HandleInventoryPagination(player, playerData, 0);
            }
            else if (buttonName == "StoreButton")
            {
                playerPages[cSteamID].StorePage = 0;
                HandleStorePagination(player, playerData, 0);
            }
            else if (buttonName == "iNext")
            {
                int totalPages = (int)Math.Ceiling(playerData.Cases.Count / 12.0);
                if (playerPages[cSteamID].IventoryPage >= totalPages - 1)
                    return;

                playerPages[cSteamID].IventoryPage++;
                HandleInventoryPagination(player, playerData, playerPages[cSteamID].IventoryPage);
            }
            else if (buttonName == "iPrev")
            {
                if (playerPages[cSteamID].IventoryPage == 0)
                    return;

                playerPages[cSteamID].IventoryPage--;
                HandleInventoryPagination(player, playerData, playerPages[cSteamID].IventoryPage);
            }
            else if (buttonName == "sNext")
            {
                int totalPages = (int)Math.Ceiling(StoreCases.Length / 10.0);
                if (playerPages[cSteamID].StorePage >= totalPages - 1)
                    return;

                playerPages[cSteamID].StorePage++;
                HandleStorePagination(player, playerData, playerPages[cSteamID].StorePage);
            }
            else if (buttonName == "sPrev")
            {
                if (playerPages[cSteamID].StorePage == 0)
                    return;

                playerPages[cSteamID].StorePage--;
                HandleStorePagination(player, playerData, playerPages[cSteamID].StorePage);
            }
            else if (buttonName == "uNext")
            {
                int totalPages = (int)Math.Ceiling(playerData.CurrentlyUnboxing.Items.Length / 10.0);
                if (playerPages[cSteamID].UnboxPage >= totalPages - 1)
                    return;

                playerPages[cSteamID].UnboxPage++;
                HandleUnboxPagination(player, playerData, playerPages[cSteamID].UnboxPage);
            }
            else if (buttonName == "uPrev")
            {
                if (playerPages[cSteamID].UnboxPage == 0)
                    return;

                playerPages[cSteamID].UnboxPage--;
                HandleUnboxPagination(player, playerData, playerPages[cSteamID].UnboxPage);
            }
            else if (previewCaseMatch.Success)
            {
                int index = int.Parse(previewCaseMatch.Groups[1].Value) - 1;
                int page = playerPages[cSteamID].StorePage;
                playerData.CurrentlyUnboxing = StoreCases[page * 10 + index];

                playerPages[cSteamID].UnboxPage = 0;
                HandleUnboxPagination(player, playerData, 0);
                DrawRoulette(player, playerData, false);
            }
            else if (buyButtonMatch.Success)
            {
                int index = int.Parse(buyButtonMatch.Groups[1].Value) - 1;
                int page = playerPages[cSteamID].StorePage;

                Case buyCase = StoreCases[page * 10 + index];
                if(buyCase.Price > Uconomy.Instance.Database.GetBalance(cSteamID.ToString()))
                {
                    ChatManager.serverSendMessage("You don't have enough money to buy this case!", Color.red, null, 
                        player.channel.owner, EChatMode.SAY, "https://i.ibb.co/sJFWwnmY/360fx360f.png", true);
                    return;
                }

                Uconomy.Instance.Database.IncreaseBalance(cSteamID.ToString(), -buyCase.Price);
                playerData.AddCase(buyCase);
                CasesBought++;
            }
            else if (useButtonMatch.Success)
            {
                int index = int.Parse(useButtonMatch.Groups[1].Value) - 1;
                int page = playerPages[cSteamID].IventoryPage;
                playerData.CurrentlyUnboxing = playerData.Cases[page * 12 + index];

                HandleUnboxPagination(player, playerData, 0);
                DrawRoulette(player, playerData, true);
            }
            else if(buttonName == "uOpen")
            {
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "uOpen", false);
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "CloseButton", false);
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "HomeButton", false);
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "InventoryButton", false);
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "StoreButton", false);

                pluginInstance.StartCoroutine(UnboxAfterDelay(player, playerData));
            }
        }

        public static void OpenUI(Player player)
        {
            CSteamID cSteamID = player.channel.owner.playerID.steamID;
            playerPages.Add(cSteamID, new ViewingPages());

            EffectManager.sendUIEffect(28280, 2828, player.channel.owner.transportConnection, true);
            player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);

            PlayerData playerData = pluginInstance.JSONService.GetOrCreatePlayer(cSteamID);
            DrawHome(player, playerData);
        }

        public static void DrawHome(Player player, PlayerData playerData)
        {
            EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, "OpenedNum", CasesOpened.ToString());
            EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, "BoughtNum", CasesBought.ToString());
            EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, "ActivityTxt", ActivityHistory);
        }

        public static void HandleInventoryPagination(Player player, PlayerData playerData, int page)
        {
            Case[] cases = playerData.Cases.Skip(page * 12).Take(12).ToArray();

            int i = 0;

            while (i < cases.Length)
            {
                string name = cases[i].Name;
                int price = cases[i].Price;

                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, $"Item{i + 1}", true);
                EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, $"ItemName{i + 1}", name);
                EffectManager.sendUIEffectImageURL(2828, player.channel.owner.transportConnection, true, $"Icon{i + 1}",
                    "https://i.ibb.co/sJFWwnmY/360fx360f.png");
                 
                i++;
            }

            for (; i < 12; i++)
            {
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, $"Item{i + 1}", false);
            }
        }

        public static void HandleStorePagination(Player player, PlayerData playerData, int page)
        {
            Case[] cases = StoreCases.Skip(page * 10).Take(10).ToArray();

            int i;

            for (i = 0; i < cases.Length; i++)
            {
                string name = cases[i].Name;
                int price = cases[i].Price;

                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, $"StoreItem{i + 1}", true);
                EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, $"sItemPrice{i + 1}", "$" + price.ToString());
                EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, $"sItemName{i + 1}", name);
                EffectManager.sendUIEffectImageURL(2828, player.channel.owner.transportConnection, true, $"sIcon{i + 1}",
                    "https://i.ibb.co/sJFWwnmY/360fx360f.png");
            }

            for (; i < 10; i++)
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, $"StoreItem{i + 1}", false);
        }

        public static void HandleUnboxPagination(Player player, PlayerData playerData, int page)
        {
            EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, "UnboxingName", $"UNBOXING: {playerData.CurrentlyUnboxing.Name}");

            CaseItem[] items = playerData.CurrentlyUnboxing.Items.Skip(page * 10).Take(10).ToArray();
            
            int i;

            for (i = 0; i < items.Length; i++)
            {
                string name = items[i].ItemName;
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, $"uPItem{i + 1}", true);
                EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, $"uPItemName{i + 1}", name);
                EffectManager.sendUIEffectImageURL(2828, player.channel.owner.transportConnection, true, $"uPIcon{i + 1}",
                    $"https://raw.githubusercontent.com/unturnedserverimages/gravity-servers/refs/heads/master/i{items[i].ItemID}.png");
            }

            for (; i < 10; i++)
                EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, $"uPItem{i + 1}", false);
        }

        public static void DrawRoulette(Player player, PlayerData playerData, bool openCaseButtonVisible)
        {
            Case currentlyUnboxing = playerData.CurrentlyUnboxing;
            CaseItem winningItem = currentlyUnboxing.GenerateWinningItem();
            playerData.WinningItem = winningItem;

            // Set the open button visibility
            EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "uOpen", openCaseButtonVisible);

            // Set the winning item
            EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, "uItemNameW", winningItem.ItemName);
            EffectManager.sendUIEffectImageURL(2828, player.channel.owner.transportConnection, true, "uIconW",
                $"https://raw.githubusercontent.com/unturnedserverimages/gravity-servers/refs/heads/master/i{winningItem.ItemID}.png");

            // Randomize the other items
            CaseItem[] randomizedItems = currentlyUnboxing.Items.OrderBy(x => Random.value).ToArray();

            for (int i = 0; i < 30; i++)
            {
                if (i == 27)
                    continue;

                string itemName = randomizedItems[i % randomizedItems.Length].ItemName;
                ushort itemID = randomizedItems[i % randomizedItems.Length].ItemID;

                EffectManager.sendUIEffectText(2828, player.channel.owner.transportConnection, true, $"uItemName{i + 1}", itemName);
                EffectManager.sendUIEffectImageURL(2828, player.channel.owner.transportConnection, true, $"uIcon{i + 1}",
                    $"https://raw.githubusercontent.com/unturnedserverimages/gravity-servers/refs/heads/master/i{itemID}.png");
            }
        }

        private static IEnumerator UnboxAfterDelay(Player player, PlayerData playerData)
        {
            yield return new WaitForSeconds(5.5f);

            if(player == null || player.channel.owner == null)
            {
                Logger.LogError("Player disconnected during unbox.");
                yield break;
            }

            EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "CloseButton", true);
            EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "HomeButton", true);
            EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "InventoryButton", true);
            EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "StoreButton", true);
            EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "Unboxing", false);
            EffectManager.sendUIEffectVisibility(2828, player.channel.owner.transportConnection, true, "Inventory", true);

            Case currentlyUnboxing = playerData.CurrentlyUnboxing;
            CaseItem winningItem = playerData.WinningItem;

            playerData.RemoveCase(currentlyUnboxing);

            player.inventory.tryAddItem(new Item(winningItem.ItemID, true), false);

            playerPages[playerData.SteamID].IventoryPage = 0;
            HandleInventoryPagination(player, playerData, 0);

            ActivityHistory += $"{UnturnedPlayer.FromPlayer(player).DisplayName} unboxed a {winningItem.ItemName} from a {currentlyUnboxing.Name}\n";
            ChatManager.serverSendMessage($"You unboxed a {winningItem.ItemName} from a {currentlyUnboxing.Name}!",
                Color.green, null, player.channel.owner, EChatMode.SAY, "https://i.ibb.co/sJFWwnmY/360fx360f.png", true);

            CasesOpened++;
        }
    }

    public class ViewingPages
    {
        public byte IventoryPage { get; set; }
        public byte StorePage { get; set; }
        public byte UnboxPage { get; set; }

        public ViewingPages()
        {
            IventoryPage = 0;
            StorePage = 0;
            UnboxPage = 0;
        }
    }
}
