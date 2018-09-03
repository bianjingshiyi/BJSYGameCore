using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    public abstract class TriggerExpr : MonoBehaviour
    {
        public abstract object getValue();
    }
}
