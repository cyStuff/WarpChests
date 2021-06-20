using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Linq;

namespace WarpChests
{
    class WarpChest
    {
        internal readonly IMonitor Monitor;

        internal readonly ModEntry mod;

        internal readonly Color group;

        internal readonly Chest container;

        internal IList<Item> state;

        internal readonly bool isMain;

        //internal readonly Item iridium = null;

        //internal int iridiumSize = 0;

        public WarpChest(Chest chest, Color color, ModEntry m, bool main = false)
        {
            group = color;
            container = chest;
            mod = m;
            Monitor = m.Monitor;
            isMain = main;

            state = new List<Item>();
            foreach(Item i in container.items)
            {
                state.Add(i);
            }

            /*if (HasIridium())
            {
                iridium = GetIridium();
                iridiumSize = iridium.Stack;
            }*/
        }

        public bool SameGroup(WarpChest other)
        {
            return this.group == other.group;
        }

        public bool HasInventoryChanged()
        {
            if (state.Count() != container.items.Count()) { return true; }
            //if (isMain && iridiumSize != iridium.Stack) { return true;  }
            return !CompareState();
            //bool changed = !state.SequenceEqual(container.items);
        }

        public void ResetInventoryCheck()
        {
            if (HasInventoryChanged())
            {
                state.Clear();
                foreach (Item i in container.items)
                {
                    state.Add(i);
                }
                //if (isMain) { iridiumSize = iridium.Stack; }
            }
        }

        /*public bool HasIridium()
        {
            return (from Item item in container.items
                    where item.ParentSheetIndex == 337
                    select item).Count() > 0;
        }*/

        public bool HasRequiredItems()
        {
            //DebugState();
            foreach (string s in mod.Config.MasterChestRequiredItems)
            {
                if (!HasItemString(s)) { return false; }
            }
            return true;
        }

        public bool SlaveCheckRequirements()
        {
            if (container.items.Count != mod.Config.SubChestRequiredItems.Length) { return false; }
            foreach (string s in mod.Config.SubChestRequiredItems)
            {
                if (!HasItemString(s)) { return false; }
            }
            return true;
        }

        public void ResetSlave()
        {
            if (isMain) { return; }
            container.items.Clear();
            foreach (string s in mod.Config.SubChestRequiredItems)
            {
                AddItemString(s);
            }
        }

        public bool MeetsRequirements()
        {
            //return (iridium != null && iridium.Stack >= 10) || !isMain;
            return HasRequiredItems() || !isMain;
        }

        /*public Item GetIridium()
        {
            Item i = (from Item item in container.items
                    where item.ParentSheetIndex == 337
                    select item).ToList().First();
            Monitor.Log("Item: " + i.DisplayName, LogLevel.Debug);
            return i;
        }*/

        internal string DebugState()
        {
            string output = "Chest Current: ";
            foreach (Item i in this.container.items)
            {
                output += i.DisplayName + "(" + i.ParentSheetIndex + "):" + i.Stack + ", ";
            }
            output += "\nChest State: ";
            foreach (Item i in this.state)
            {
                output += i.DisplayName + "(" + i.ParentSheetIndex + "):" + i.Stack + ", ";
            }
            return output;
        }

        private bool CompareState()
        {
            foreach (var i in state.Zip(container.items, (a, b) => new { a, b }))
            {
                if (!ItemsEqual(i.a,i.b))
                {
                    return false;
                }
            }
            return true;
        }


        public bool HasItem(Item item)
        {
            foreach (Item i in container.items)
            {
                if (i == item)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HadItem(Item item)
        {
            foreach (Item i in state)
            {
                if (i == item)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool HasItemString(string s)
        {
            //DebugState();
            int i = -1;
            if (mod.ja != null)
            {
                i = mod.ja.GetObjectId(s);
            }
            bool hasItem = false;
            if (i != -1)
            {
                hasItem = (from Item item in container.items
                            where mod.ja.GetObjectId(item.ToString()) == i
                            select item).Count() > 0;
            }
            else
            {
                i = GetGameIdFromString(s);
                hasItem = (from Item item in container.items
                            where item.parentSheetIndex == i
                            select item).Count() > 0;
            }
            //Monitor.Log($"Finding item {s} with id {i} is {hasItem}.", LogLevel.Warn);
            return hasItem;
        }

        internal void AddItemString(string s)
        {
            int i = -1;
            if (mod.ja != null)
            {
                mod.ja.GetObjectId(s);
            }
            if (i == -1)
            {
                i = GetGameIdFromString(s);
            }
            //Monitor.Log($"Adding item {s} with id {i}", LogLevel.Warn);
            Item toAdd = (Item)new StardewValley.Object(i, 1);
            container.addItem(toAdd);


        }

        static internal int GetGameIdFromString(string s)
        {
            return Game1.objectInformation.FirstOrDefault(x => x.Value.Split('/')[0] == s).Key;
        }

        internal static bool ItemsEqual(Item i1, Item i2)
        {
            if (i1 is MeleeWeapon w1 && i2 is MeleeWeapon w2)
            {
                if (w1.InitialParentTileIndex == w2.InitialParentTileIndex)
                {
                    return true;
                }
            }
            else if (i1 is Tool t1 && i2 is Tool t2)
            {
                if (t1.initialParentTileIndex == t2.initialParentTileIndex && t1.upgradeLevel == t2.upgradeLevel)
                {
                    return true;
                }
            }
            else if (i1.ParentSheetIndex == i2.parentSheetIndex && i1.Stack == i2.Stack)
            {
                return true;
            }
            return false;
        }
    }
}
