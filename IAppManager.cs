using System.Threading.Tasks;

namespace BJSYGameCore
{
    public interface IResourceManager
    {
        T load<T>(string resPath, string dir);
        Task<T> loadAsync<T>(string resPath, string dir);
    }
    public interface IAppManager
    {
        ILanguageManager langManager { get; }
        IResourceManager resourceManager { get; }
        FileManager fileManager { get; }
    }
}
