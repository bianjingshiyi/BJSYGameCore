using System.Threading.Tasks;

namespace BJSYGameCore
{
    public interface IResourceManager
    {
        T load<T>(string resPath, string dir);
        Task<T> loadAsync<T>(string resPath, string dir);
    }
    public interface IGameManager
    {
        ILanguageManager langManager { get; }
        IResourceManager resourceManager { get; }
        FileManager fileManager { get; }
    }
    public class GameManager : IGameManager
    {
        public GameManager()
        {
            resourceManager = new ResourceManager(this);
            fileManager = new FileManager();
        }
        public ILanguageManager langManager => throw new System.NotImplementedException();
        public ResourceManager resourceManager { get; }
        IResourceManager IGameManager.resourceManager => resourceManager;
        public FileManager fileManager { get; }
    }
}
