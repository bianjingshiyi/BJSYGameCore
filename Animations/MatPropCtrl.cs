using UnityEngine;

namespace BJSYGameCore.Animations
{
    [ExecuteInEditMode]
    public abstract class MatPropCtrl : MonoBehaviour
    {
        Material[] _originMaterials;
        Material[] _cloneMaterials;
        public abstract Material[] materials { get; set; }
        protected void OnEnable()
        {
            _originMaterials = materials;
            _cloneMaterials = new Material[_originMaterials.Length];
            for (int i = 0; i < _cloneMaterials.Length; i++)
            {
                _cloneMaterials[i] = Instantiate(_originMaterials[i]);
            }
            materials = _cloneMaterials;
        }
        protected abstract void Update();
        protected void OnDisable()
        {
            for (int i = 0; i < _cloneMaterials.Length; i++)
            {
                DestroyImmediate(_cloneMaterials[i]);
            }
            _cloneMaterials = null;
            materials = _originMaterials;
            _originMaterials = null;
        }
        protected virtual void Reset()
        {
            enabled = false;
        }
        [ContextMenu(nameof(showMatPropNames))]
        public void showMatPropNames()
        {
            if (materials != null)
            {
                foreach (Material material in materials)
                {
                    foreach (string name in material.GetTexturePropertyNames())
                    {
                        Debug.Log(name);
                    }
                }
            }
        }
    }
}
