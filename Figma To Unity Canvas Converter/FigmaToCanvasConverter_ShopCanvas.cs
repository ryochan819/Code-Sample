using System;
using CarterApp._InAppPurchase;
using UnityEngine;

namespace CarterApp._UI
{
    public class FigmaToCanvasConverter_ShopCanvas : FigmaToCanvasConverter
    {
        [SerializeField] private FigmaElementTarget[] subscribeElementTargets;
        [SerializeField] private TextElementTarget[] textElementTargets;
        [SerializeField] private FreeProceduralImageElementTarget[] freeProceduralImageElementTargets;

        void Start()
        {
            SetupTarget(subscribeElementTargets);
            ScaleTextSize(textElementTargets);
            SetupFreeProceduralCorner(freeProceduralImageElementTargets);
        }
    }
}