using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpChests
{
    public class Config
    {
        public string[] MasterChestRequiredItems { get; set; }

        public string[] SubChestRequiredItems { get; set; }

        public Config()
        {
            MasterChestRequiredItems = new string[] { "Star Shards" };
            SubChestRequiredItems = new string[] { "Battery Pack" };
        }
    }
}
