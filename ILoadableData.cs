
using UnityEngine.SceneManagement;

namespace BJSYGameCore
{
    public interface ILoadableData
    {
        ISavable load(SaveManager saveManager, int id, string path);
    }
}