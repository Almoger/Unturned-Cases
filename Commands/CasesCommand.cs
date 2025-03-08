using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedCases.Services;

namespace UnturnedCases.Commands
{
    internal class CasesCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "cases";
        public string Help => "Open a case!";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer player, string[] command)
        {
            UnturnedPlayer unturnedPlayer = player as UnturnedPlayer;
            UIService.OpenUI(unturnedPlayer.Player);
        }
    }
}
