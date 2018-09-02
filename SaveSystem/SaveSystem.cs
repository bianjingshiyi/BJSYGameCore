
using UnityEngine;

namespace TBSGameCore.SaveSystem
{
    public class SaveSystem : MonoBehaviour
    {

    }
    public interface ISavable
    {
        ILoadable save();
    }
    public interface ILoadable
    {
        ISavable load();
    }
}
