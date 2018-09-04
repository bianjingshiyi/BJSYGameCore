using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerExpr : MonoBehaviour
    {
        public abstract string desc
        {
            get;
        }
        public abstract object getValue(UnityEngine.Object targetObject);
    }
}