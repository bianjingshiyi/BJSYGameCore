using UnityEngine;

namespace BJSYGameCore
{
    public static class AnimationCurveHelper
    {
        public static AnimationCurve getCopy(this AnimationCurve curve, float timeScale = 1, float valueScale = 1)
        {
            AnimationCurve newCurve = new AnimationCurve(curve.keys);
            for (int i = 0; i < newCurve.keys.Length; i++)
            {
                newCurve.keys[i].time *= timeScale;
                newCurve.keys[i].value *= valueScale;
            }
            return newCurve;
        }
    }
}