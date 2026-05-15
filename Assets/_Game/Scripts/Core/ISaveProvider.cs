namespace Capybrawlers.Core
{
    public interface ISaveProvider
    {
        PlayerProfile Load();
        void Save(PlayerProfile profile);
    }
}
