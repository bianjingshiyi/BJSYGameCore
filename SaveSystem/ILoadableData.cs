
using UnityEngine.SceneManagement;

namespace BJSYGameCore.SaveSystem
{
    public interface ILoadableData
    {
        void load(SaveManager saveManager, int id, string path);
    }
}