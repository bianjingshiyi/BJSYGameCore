using UnityEngine;
using UnityEngine.UI;
namespace BJSYGameCore.Animations
{
    [RequireComponent(typeof(Graphic))]
    public abstract class GraphMatPropCtrl : MatPropCtrl
    {
        Graphic _graphic;
        public Graphic graphic
        {
            get
            {
                if (_graphic == null)
                    _graphic = GetComponent<Graphic>();
                return _graphic;
            }
        }
        public override Material[] materials
        {
            get { return new Material[] { graphic.material }; }
            set { graphic.material = value != null && value.Length > 0 ? value[0] : null; }
        }
    }
}
