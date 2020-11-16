using System.Linq;
using UnityEngine;
namespace BJSYGameCore.UI
{
    public class UIController : MonoBehaviour
    {
        Animator _animator;
        Animator animator
        {
            get
            {
                if (_animator == null)
                    _animator = GetComponent<Animator>();
                return _animator;
            }
        }
        [HideInInspector]
        [SerializeField]
        string[] _stateNames;
        string[] stateNames
        {
            get { return _stateNames; }
        }
        [HideInInspector]
        [SerializeField]
        string[] _initStates;
        string[] initStates
        {
            get { return _initStates; }
        }
        public string this[string name]
        {
            get
            {
                return stateNames
                    .Where(sn => sn.Contains(name))
                    .Select(sn => sn.Replace(name + "/", null))
                    .FirstOrDefault(s => animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex(name + "Controller")).IsName(s));
            }
            set
            {
                animator.Play(value, animator.GetLayerIndex(name + "Controller"));
            }
        }
        protected void Awake()
        {
            foreach (string initState in initStates)
            {
                string[] sArray = initState.Split('/');
                string controllerName = sArray[0];
                string stateName = sArray[1];
                this[controllerName] = stateName;
            }
        }
    }
}