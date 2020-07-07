using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpChests
{
    public class ModEntry : Mod
    {
        internal JsonAssetsAPI ja = null;

        ChestManager Manager;

        public Config Config;
        public override void Entry(IModHelper helper)
        {

            Config = Helper.ReadConfig<Config>();

            Manager = new ChestManager(this);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnNewDay;
            helper.Events.GameLoop.DayEnding += OnDayEnd;
            helper.Events.GameLoop.UpdateTicked += Manager.OnGameTick;

        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            if (api != null)
            {
                ja = api;
                Monitor.Log("Found Json Assets API", LogLevel.Info);
            }
            else
            {
                Monitor.Log("No Json Assets API", LogLevel.Info);
            }
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
