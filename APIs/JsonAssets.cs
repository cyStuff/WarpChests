namespace WarpChests
{
    public interface IJsonAssetsApi //Get The JsonAssets Api functions
    {
        int GetObjectId(string name);
        void LoadAssets(string path);
    }
}
