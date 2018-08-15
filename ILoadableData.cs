
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    public interface ILoadableData
    {
        ISavable load(SaveManager saveManager, int id, string path);
    }
}