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
