using TMPro;
using UnityEngine;

namespace CarterApp._UI
{
    public class FigmaToCanvasConverter : MonoBehaviour
    {
        // ***** IMPORTANT *****
        // Remember to change RectTransform to Top Left Anchor (0.5, 0.5) in Unity Editor
        // The calculation is based on Figma's Top Left Anchor (0, 1) setting
        // ********************** 

        public float figmaWidth = 375;
        public float figmaHeight = 812;

        [SerializeField] protected bool isDebug = true;

        public void SetupTarget(FigmaElementTarget[] elementTarget, bool isUnityCenterAnchor = false)
        {
            float screenWidthRatio = Screen.width / figmaWidth;
            float screenHeightRatio = Screen.height / figmaHeight;

            foreach (FigmaElementTarget target in elementTarget)
            {
                Vector2 targetCanvasSize = CalculateTargetCanvasSize(target, screenWidthRatio, screenHeightRatio);

                float targetCanvasPosX, targetCanvasPosY;

                if (isUnityCenterAnchor)
                {
                    targetCanvasPosX = target.figmaPosX * screenWidthRatio;
                    targetCanvasPosY = target.figmaPosY * screenHeightRatio;
                }
                else
                {
                    // Convert Figma's anchor (0, 1) to center anchor (0.5, 0.5)
                    float figmaPosXConvertToCenterAnchor = target.figmaWidth / 2 + target.figmaPosX;
                    float figmaPosYConvertToCenterAnchor = target.figmaHeight / 2 + target.figmaPosY;

                    targetCanvasPosX = figmaPosXConvertToCenterAnchor * screenWidthRatio;
                    targetCanvasPosY = figmaPosYConvertToCenterAnchor * screenHeightRatio;
                }

                if (target.rect != null)
                {
                    target.rect.sizeDelta = targetCanvasSize;
                    target.rect.anchoredPosition = new Vector2(targetCanvasPosX, -targetCanvasPosY);
                }
            }
        }

        public void SetupTargetDeltaSizeOnly(FigmaElementTarget_DeltaSizeOnly[] elementTarget)
        {
            float screenWidthRatio = Screen.width / figmaWidth;
            float screenHeightRatio = Screen.height / figmaHeight;

            foreach (FigmaElementTarget_DeltaSizeOnly target in elementTarget)
            {
                Vector2 targetCanvasSize = CalculateTargetCanvasSize(target, screenWidthRatio, screenHeightRatio);

                if (target.rect != null)
                {
                    target.rect.sizeDelta = targetCanvasSize;
                }
            }
        }

        public Vector2 CalculateTargetCanvasSize(FigmaElementTarget target, float screenWidthRatio, float screenHeightRatio)
        {
            float targetCanvasWidth;
            float targetCanvasHeight;

            if (target.keepNativeRatio)
            {
                if (target.nativeRatioScaleByWidth)
                {
                    targetCanvasWidth = target.figmaWidth * screenWidthRatio;
                    targetCanvasHeight = target.figmaHeight * (targetCanvasWidth / target.figmaWidth);
                }
                else
                {
                    targetCanvasHeight = target.figmaHeight * screenHeightRatio;
                    targetCanvasWidth = target.figmaWidth * (targetCanvasHeight / target.figmaHeight);
                }
            }
            else
            {
                targetCanvasWidth = target.figmaWidth * screenWidthRatio;
                targetCanvasHeight = target.figmaHeight * screenHeightRatio;
            }

            return new Vector2(targetCanvasWidth, targetCanvasHeight);
        }

        private Vector2 CalculateTargetCanvasSize(FigmaElementTarget_DeltaSizeOnly target, float screenWidthRatio, float screenHeightRatio)
        {
            float targetCanvasWidth;
            float targetCanvasHeight;

            //Debug.Log("ScreenWidthRatio: " + screenWidthRatio + " TargetWidth: " + target.figmaWidth + " ScreenWidth: " + Screen.width);

            if (target.keepNativeRatio)
            {
                if (target.nativeRatioScaleByWidth)
                {
                    targetCanvasWidth = target.figmaWidth * screenWidthRatio;
                    targetCanvasHeight = target.figmaHeight * (targetCanvasWidth / target.figmaWidth);
                }
                else
                {
                    targetCanvasHeight = target.figmaHeight * screenHeightRatio;
                    targetCanvasWidth = target.figmaWidth * (targetCanvasHeight / target.figmaHeight);
                }
            }
            else
            {
                targetCanvasWidth = target.figmaWidth * screenWidthRatio;
                targetCanvasHeight = target.figmaHeight * screenHeightRatio;
            }

            return new Vector2(targetCanvasWidth, targetCanvasHeight);
        }

        public void ScaleTextSize(TextElementTarget[] textElementTargets)
        {
            foreach (TextElementTarget textElement in textElementTargets)
            {
                if (!textElement.useCustomScaleSize && textElement.text != null)
                {
                    textElement.text.fontSize = (int)(textElement.figmaFontSize * Screen.width / figmaWidth);
                }
                else if (textElement.text != null)
                {
                    textElement.text.fontSize = UIUtility.ResolveResponsiveValue(textElement.size640, textElement.size750, textElement.size828, textElement.size1080, textElement.size1440);
                }

                if (textElement.characterSpacing != 0)
                {
                    // Looks like double the characterSpacing can get the cloestest result as Figma
                    textElement.text.characterSpacing = textElement.characterSpacing * Screen.width / figmaWidth;
                    textElement.text.wordSpacing = textElement.characterSpacing * Screen.width / figmaWidth;
                }
            }
        }

        public void SetupFreeProceduralCorner(FreeProceduralImageElementTarget[] freeProceduralElementTargets)
        {
            foreach (FreeProceduralImageElementTarget target in freeProceduralElementTargets)
            {
                float upperLeftRadious = target.figmaUpperLeftRadious * Screen.width / figmaWidth;
                float upperRightRadious = target.figmaUpperRightRadious * Screen.width / figmaWidth;
                float lowerLeftRadious = target.figmaLowerLeftRadious * Screen.width / figmaWidth;
                float lowerRightRadious = target.figmaLowerRightRadious * Screen.width / figmaWidth;

                if (target.proceduralImageModifier != null)
                    target.proceduralImageModifier.Radius = new Vector4(upperLeftRadious, upperRightRadious, lowerLeftRadious, lowerRightRadious);
            }
        }

        [System.Serializable]
        public class FigmaElementTarget
        {
            public float figmaWidth;
            public float figmaHeight;
            public float figmaPosX;
            public float figmaPosY;
            public RectTransform rect;
            public bool keepNativeRatio;
            public bool nativeRatioScaleByWidth;
        }

        [System.Serializable]
        public class FigmaElementTarget_DeltaSizeOnly
        {
            public float figmaWidth;
            public float figmaHeight;
            public RectTransform rect;
            public bool keepNativeRatio;
            public bool nativeRatioScaleByWidth;
        }

        [System.Serializable]
        public class TextElementTarget
        {
            public float figmaFontSize;
            public TextMeshProUGUI text;
            public float characterSpacing;
            public bool useCustomScaleSize;
            public float size640;
            public float size750;
            public float size828;
            public float size1080;
            public float size1440;
        }


        [System.Serializable]
        public class FreeProceduralImageElementTarget
        {
            public FreeModifier proceduralImageModifier;
            public float figmaUpperLeftRadious;
            public float figmaUpperRightRadious;
            public float figmaLowerLeftRadious;
            public float figmaLowerRightRadious;
        }
    }
}
