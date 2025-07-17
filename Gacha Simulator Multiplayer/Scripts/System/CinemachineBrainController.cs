using System;
using Gacha.gameplay;
using Unity.Cinemachine;
using UnityEngine;

namespace Gacha.system
{
    public class CinemachineBrainController : MonoBehaviour
    {
        [SerializeField] CinemachineBrain cinemachineBrain;

        private void HandleCameraBlendUpdate(BlendMode mode, float duration)
        {
            switch (mode)
            {
                case BlendMode.Cut:
                    cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, duration);
                    break;
                case BlendMode.EaseInOut:
                    cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, duration);
                    break;
                default:
                    break;
            }
        }

        void OnEnable()
        {
            GameEventSystem.onCameraBlendUpdate += HandleCameraBlendUpdate;
        }

        void OnDisable()
        {
            GameEventSystem.onCameraBlendUpdate -= HandleCameraBlendUpdate;
        }
    }

    public enum BlendMode
    {
        Cut,
        EaseInOut
    }
}