using UnityEngine;
using UnityEngine.UI;
namespace BJSYGameCore.UI
{
    [RequireComponent(typeof(Image))]
    public class ImageAspectRatioFitter : AspectRatioFitter
    {
        Image image
        {
            get { return GetComponent<Image>(); }
        }
        protected override void Update()
        {
            if (aspectRatio != calcRatio())
                resetRatio();
            base.Update();
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            resetRatio();
        }
#endif
        private void resetRatio()
        {
            aspectMode = AspectMode.EnvelopeParent;
            aspectRatio = calcRatio();
        }

        private float calcRatio()
        {
            return image.sprite.rect.width / image.sprite.rect.height;
        }
    }
}