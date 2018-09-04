
using UnityEditor;
using UnityEngine;

namespace TBSGameCore.TriggerSystem
{
    class TriggerAddIntDrawer : TriggerExprSubDrawer<TriggerAddInt>
    {
        public TriggerAddIntDrawer(TriggerObjectDrawer parent, Transform transform) : base(parent, transform)
        {
        }
        public override float height
        {
            get { return 16; }
        }
        TriggerTypedExprDrawer[] _paraDrawers = null;
        protected override void draw(Rect position, GUIContent label, TriggerAddInt expr)
        {
            Rect valuePosition;
            if (label != null)
            {
                Rect labelPosition = new Rect(position.x, position.y, 64, 16);
                EditorGUI.LabelField(labelPosition, label);
                valuePosition = new Rect(position.x + labelPosition.width, position.y, position.width - labelPosition.width, position.height);
            }
            else
                valuePosition = position;
            TriggerAddInt addInt = expr as TriggerAddInt;
            if (_paraDrawers == null)
            {
                _paraDrawers = new TriggerTypedExprDrawer[2];
                _paraDrawers[0] = new TriggerTypedExprDrawer(this, addInt.transform, typeof(int), nameof(addInt.left));
                _paraDrawers[1] = new TriggerTypedExprDrawer(this, addInt.transform, typeof(int), nameof(addInt.right));
            }
            Rect leftPosition = new Rect(valuePosition.x, valuePosition.y, (valuePosition.width - 16) / 2, valuePosition.height);
            addInt.left = _paraDrawers[0].draw(leftPosition, null, addInt.left);
            Rect operatorPosition = new Rect(valuePosition.x + leftPosition.width, valuePosition.y, 16, valuePosition.height);
            EditorGUI.LabelField(operatorPosition, new GUIContent("+"));
            Rect rightPosition = new Rect(valuePosition.x + leftPosition.width + operatorPosition.width, valuePosition.y, valuePosition.width - leftPosition.width - operatorPosition.width, valuePosition.height);
            addInt.right = _paraDrawers[1].draw(rightPosition, null, addInt.right);
        }
    }
}