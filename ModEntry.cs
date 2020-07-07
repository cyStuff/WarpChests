using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpChests
{
    public class ModEntry : Mod
    {
        internal JsonAssetsAPI ja;

        ChestManager Manager;
        public override void Entry(IModHelper helper)
        {

            Manager = new ChestManager(this);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnNewDay;
            helper.Events.GameLoop.DayEnding += OnDayEnd;
            helper.Events.GameLoop.UpdateTicked += Manager.OnGameTick;

        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            if (api == null)
            {
                Monitor.Log("No Json Assets API???", LogLevel.Error);
                return;
            }
            ja = api;
        }

        public void OnNewDay(object sender, DayStartedEventArgs e)
        {
            Manager.LinkChests();
        }

        public void OnDayEnd(object sender, DayEndingEventArgs e)
        {
            Manager.UnlinkChests();
        }
    }
}
