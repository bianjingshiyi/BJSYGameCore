using UnityEngine;
namespace BJSYGameCore.Animations
{
    [RequireComponent(typeof(Renderer))]
    public abstract class RendererMatPropCtrl : MatPropCtrl
    {
        public new Renderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = GetComponent<Renderer>();
                return _renderer;
            }
        }
        public override Material[] materials
        {
            get { return renderer.materials; }
            set { renderer.materials = value; }
        }
        Renderer _renderer;
    }
}
