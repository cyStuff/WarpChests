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

        ChestManager Manager;
        public override void Entry(IModHelper helper)
        {

            Manager = new ChestManager(Monitor);

            helper.Events.GameLoop.DayStarted += OnNewDay;
            helper.Events.GameLoop.DayEnding += OnDayEnd;
            helper.Events.GameLoop.UpdateTicked += Manager.OnGameTick;

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
