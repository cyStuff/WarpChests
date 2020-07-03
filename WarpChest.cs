using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace WarpChests
{
    class WarpChest
    {
        internal readonly IMonitor Monitor;

        internal readonly Color group;

        internal readonly Chest container;

        internal IList<Item> state;

        internal readonly bool isMain;

        //internal readonly Item iridium = null;

        //internal int iridiumSize = 0;

        public static readonly int[] masterRequiredItems = { 578 };

        public static readonly int[] slaveRequiredItems = { 787 };

        public WarpChest(Chest chest, Color color, IMonitor mon, bool main = false)
        {
            group = color;
            container = chest;
            Monitor = mon;
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
            return !compareState();
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
            foreach (int i in masterRequiredItems)
            {
                bool hasItem = (from Item item in container.items
                        where item.ParentSheetIndex == i
                        select item).Count() > 0;
                if (!hasItem) { return false; }
            }
            return true;
        }

        static public bool SlaveCheckRequirements(Chest chest)
        {
            if (chest.items.Count != slaveRequiredItems.Length) { return false; }
            foreach (int i in slaveRequiredItems)
            {
                bool hasItem = (from Item item in chest.items
                                where item.ParentSheetIndex == i
                                select item).Count() > 0;
                if (!hasItem) { return false; }
            }
            return true;
        }

        public bool SlaveCheckRequirements()
        {
            if (container.items.Count != slaveRequiredItems.Length) { return false; }
            foreach (int i in slaveRequiredItems)
            {
                bool hasItem = (from Item item in container.items
                                where item.ParentSheetIndex == i
                                select item).Count() > 0;
                if (!hasItem) { return false; }
            }
            return true;
        }

        public void ResetSlave()
        {
            if (isMain) { return; }
            container.items.Clear();
            foreach (int i in slaveRequiredItems)
            {
                Item toAdd = (Item)new StardewValley.Object(i, 1);
                container.addItem(toAdd);
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

        private bool compareState()
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
