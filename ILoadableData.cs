
using UnityEngine.SceneManagement;

namespace TBSGameCore
{
    public interface ILoadableData
    {
        ISavable load(Scene scene, int id, string path);
    }
}