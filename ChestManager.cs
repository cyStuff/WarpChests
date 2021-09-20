using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace WarpChests
{
    class ChestManager
    {
        IList<WarpChest> chestList;

        ModEntry mod;

        IMonitor Monitor;


        public ChestManager(ModEntry m)
        {
            mod = m;
            Monitor = m.Monitor;
            chestList = new List<WarpChest>();
        }

        private bool tickProcess = false;
        private HashSet<Color> changeGroups = new HashSet<Color>();
        public void OnGameTick(object sender, UpdateTickedEventArgs e)
        {
            foreach (WarpChest wc in chestList)
            {
                tickProcess |= wc.HasInventoryChanged();
            }
            if (tickProcess)
            {
                foreach (WarpChest wc in chestList)
                {
                    changeGroups.Add(wc.group);
                }
                //DebugSearch();
                foreach (Color group in changeGroups)
                {
                    //Monitor.Log("Linking Group " + group.ToString(), LogLevel.Debug);
                    LinkGroup(group);
                }
                changeGroups.Clear();
                tickProcess = false;
            }
        }

        public void UnlinkGroup(Color group)
        {
            IList<WarpChest> removes = new List<WarpChest>();
            foreach(WarpChest wc in chestList)
            {
                if (!wc.isMain && wc.group == group)
                {
                    wc.ResetSlave();

                    removes.Add(wc);
                }
                else if (wc.group == group)
                {
                    removes.Add(wc);
                }
            }

            foreach(WarpChest wc in removes)
            {
                chestList.Remove(wc);
            }
        }

        public bool LinkGroup(Color group)
        {
            bool didLink = false;
            WarpChest master = null;
            foreach(WarpChest wc in chestList)
            {
                if (wc.isMain && wc.group == group)
                {
                    master = wc;
                    break;
                }
            }
            foreach(WarpChest wc in chestList)
            {
                if (wc.group == group && !wc.isMain)
                {
                    didLink = true;
                    if (wc.SlaveCheckRequirements() && !wc.HasInventoryChanged() && !master.HasInventoryChanged())
                    {
                        wc.container.items.Clear();
                        foreach (Item item in master.container.items)
                        {
                            wc.container.addItem(item);
                        }
                    }
                    else if (master.HasInventoryChanged() && !wc.HasInventoryChanged())
                    {
                        Sync(wc, master);
                    }
                    else if (!master.HasInventoryChanged() && wc.HasInventoryChanged())
                    {
                        Sync(master, wc);
                    }
                    else
                    {
                        DualSync(wc, master);
                    }
                }
            }
            foreach(WarpChest wc in chestList)
            {
                if (wc.group == group)
                {
                    wc.ResetInventoryCheck();
                }
            }
            if (!master.MeetsRequirements())
            {
                didLink = false;
                UnlinkGroup(master.group);
            }
            return didLink;
        }

        private void DualSync(WarpChest warp1, WarpChest warp2)
        {
            //Monitor.Log("Syncing Chests.", LogLevel.Debug);

            IEnumerable<Item> warp1Added = warp1.state.Where(item => !warp1.HasItem(item));
            IEnumerable<Item> warp1Removed = warp1.container.items.Where(item => !warp1.HadItem(item));

            IEnumerable<Item> warp2Added = warp2.state.Where(item => !warp2.HasItem(item));
            IEnumerable<Item> warp2Removed = warp2.container.items.Where(item => !warp2.HadItem(item));

            foreach(Item item in warp1Removed)
            {
                warp2.container.items.Remove(item);
                //Monitor.Log("Removing Item: " + item.DisplayName, LogLevel.Debug);
            }
            foreach(Item item in warp2Removed)
            {
                warp1.container.items.Remove(item);
                //Monitor.Log("Removing Item: " + item.DisplayName, LogLevel.Debug);
            } 

            foreach(Item item in warp1Added)
            {
                warp2.container.addItem(item);
            }
            foreach(Item item in warp2Added)
            {
                warp1.container.addItem(item);
            }
        }

        private void Sync(WarpChest stat, WarpChest change)
        {
            //Monitor.Log("Syncing Chests.", LogLevel.Debug);
            IList<Item> removes = new List<Item>();
            foreach (Item item in stat.container.items)
            {
                if (!change.HasItem(item))
                {
                    removes.Add(item);
                    //Monitor.Log("Removing Item: " + item.DisplayName, LogLevel.Debug);
                }
            }
            foreach (Item item in removes)
            {
                stat.container.items.Remove(item);
            }
            foreach (Item item in change.container.items)
            {
                if (!stat.HasItem(item))
                {
                    stat.container.addItem(item);
                    //Monitor.Log("Adding Item: " + item.DisplayName, LogLevel.Debug);
                }
            }
        }

        public void UnlinkChests()
        {
            Monitor.Log("Unlinking Chests", LogLevel.Debug);
            IList<Color> groups = new List<Color>();
            foreach(WarpChest wc in chestList)
            {
                if (!groups.Contains(wc.group))
                {
                    groups.Add(wc.group);
                }
            }
            foreach(Color group in groups)
            {
                UnlinkGroup(group);
            }
        }

        public void LinkChests()
        {
            Monitor.Log("Linking Chests", LogLevel.Debug);
            chestList.Clear();
            IEnumerable<Chest> chests = GetAllChests();
            foreach(Chest chest in chests)
            {
                WarpChest wc = new WarpChest(chest, chest.playerChoiceColor, mod, true);
                if (wc.MeetsRequirements()) { chestList.Add(wc); }
            }
            //DebugSearch();
            foreach (WarpChest ch in chestList) {
                chestList = chestList.Where(chest => !chest.SameGroup(ch) || chest == ch).ToList();
            }
            IList<WarpChest> slaveList = new List<WarpChest>();
            foreach(Chest chest in chests)
            {
                foreach (WarpChest wc in chestList)
                {
                    WarpChest nc = new WarpChest(chest, chest.playerChoiceColor, mod, true);
                    if (wc.isMain && wc.container != nc.container && nc.group == wc.group && nc.SlaveCheckRequirements())
                    {
                        slaveList.Add(new WarpChest(chest, chest.playerChoiceColor, mod));
                    }
                }
            }
            chestList = chestList.Concat(slaveList).ToList();
            chestList = chestList.Where(wc => wc.SameGroup(chestList.Aggregate((c1,c2) => (c1.SameGroup(wc) && c1!=wc)?c1:c2))).ToList();
            foreach(WarpChest wc in chestList)
            {
                if (wc.isMain)
                {
                    bool attempt = LinkGroup(wc.group);
                    if (attempt)
                    {
                        Monitor.Log("Linking group: " + wc.group, LogLevel.Debug);
                    }
                }
            }
        }
        public void DebugSearch()
        {
            Monitor.Log("Debug Search", LogLevel.Alert);
            foreach (Chest chest in (from WarpChest chest in chestList select chest.container))
            {
                var items = (
                    from Item item in chest.items
                    select new
                    {
                        Name = item.DisplayName,
                        ID = item.parentSheetIndex,
                        Count = item.Stack
                    }
                    );
                Monitor.Log(chest.playerChoiceColor.ToString(), LogLevel.Info);
                foreach (var item in items)
                {
                    Monitor.Log(item.Name + ", " + item.ID + ", " + item.Count, LogLevel.Debug);
                }
            }
        }
       

        private IEnumerable<Chest> GetAllChests()
        {
            foreach (GameLocation location in GetLocations())
            {
                foreach (KeyValuePair<Vector2, SObject> pair in location.Objects.Pairs)
                {
                    Vector2 tile = pair.Key;
                    SObject obj = pair.Value;

                    if (obj is Chest chest && chest.playerChest.Value)
                    {
                        yield return chest;
                    }
                }
            }
        }


        public IEnumerable<GameLocation> GetLocations()
        {
            return Game1.locations
                .Concat(
                    from location in Game1.locations.OfType<BuildableGameLocation>()
                    from building in location.buildings
                    where building.indoors.Value != null
                    select building.indoors.Value
                );
        }
    }
}
