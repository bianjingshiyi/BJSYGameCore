using System.Threading.Tasks;

namespace BJSYGameCore
{
    public interface IResourceManager
    {
#if !ADDRESSABLE_ASSETS
        T load<T>(string resPath, string dir);
        Task<T> loadAsync<T>(string resPath, string dir);
#endif
    }
    public interface IAppManager
    {
        ILanguageManager langManager { get; }
        IResourceManager resourceManager { get; }
        FileManager fileManager { get; }
    }
}
