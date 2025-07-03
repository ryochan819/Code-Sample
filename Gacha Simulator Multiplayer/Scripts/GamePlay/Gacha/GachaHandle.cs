using PrimeTween;
using UnityEngine;

namespace Gacha.gameplay
{
    public class GachaHandle : MonoBehaviour
    {
        [SerializeField] GachaMachine gachaMachine;

        [Header("Handle Rotation")]
        private float totalRotation = 0f;
        private float lastZRotation = 0f;
        private const float rotationThreshold = 360f;
        float progress = 0f;

        [Header("Audio")]
        [SerializeField] AudioSource audioSource;
        AudioClip gachaRotateSound;
        private float lastPlayedProgress = -1f; // Track the last played segment
        private double scheduledEndTime = 0; // Track when the current segment is scheduled to stop

        public bool GetIsEnoughCoins()
        {
            return gachaMachine.EnoughCoins();
        }

        public bool HandleRotated()
        {
            float currentZRotation = transform.eulerAngles.z;
            float deltaRotation = Mathf.DeltaAngle(lastZRotation, currentZRotation);

            if (Mathf.Abs(deltaRotation) < 180f)
            {
                totalRotation += deltaRotation;
            }

            lastZRotation = currentZRotation;

            progress = Mathf.Abs(totalRotation) % rotationThreshold / rotationThreshold;

            if (progress > 0f)
            {
                PlaySoundAtProgress(progress);
            }

            if (Mathf.Abs(totalRotation) >= rotationThreshold)
            {
                // Gacha Complete
                gachaMachine.SpawnCapsule();
                gachaMachine.Reset();
                TriggerEvent();
                totalRotation = 0f;
                return true;
            }
            return false;
        }

        private void PlaySoundAtProgress(float progress)
        {
            if (audioSource == null) return;

            if (gachaRotateSound == null)
            {
                gachaRotateSound = SoundManager.Instance.GetClip(SoundType.GachaRotate);
            }

            if (audioSource.clip != gachaRotateSound)
            {
                audioSource.clip = gachaRotateSound;
            }

            double currentTime = AudioSettings.dspTime;

            // Snap progress to the nearest 1% step (0.00, 0.01, 0.02, ..., 1.00)
            float snappedProgress = Mathf.Floor(progress * 100) / 100.0f;

            // Calculate segment start time and duration
            float segmentStartTime = snappedProgress * gachaRotateSound.length;
            float segmentDuration = gachaRotateSound.length * 0.01f; // 1% of total length

            // If progress reaches a new 10% step, start playing from that step
            if (snappedProgress > lastPlayedProgress)
            {
                lastPlayedProgress = snappedProgress; // Update last played segment
                audioSource.Stop();
                audioSource.time = segmentStartTime;
                audioSource.Play();
                scheduledEndTime = currentTime + segmentDuration; // Set the new scheduled end time
            }
            // If progress surpasses the scheduled end time, extend it
            else if (currentTime > scheduledEndTime)
            {
                scheduledEndTime = currentTime + segmentDuration;
            }

            // Update the scheduled stop time dynamically
            audioSource.SetScheduledEndTime(scheduledEndTime);

            Debug.Log($"Playing segment: {snappedProgress * 100}% | Start: {segmentStartTime}s | Scheduled End: {scheduledEndTime}s");
        }

        private void TriggerEvent()
        {
            Debug.Log("Two full rotations detected! Performing an action...");
            lastPlayedProgress= -1;
            scheduledEndTime = 0; 
        }

        public void ResetHandle(bool playSound = true)
        {
            Debug.Log("Resetting handle position");
            Vector3 currentEuler = transform.localEulerAngles;
            Quaternion targetRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 0f);
            Tween.LocalRotation(transform, targetRotation, 0.3f, Ease.OutCubic);

            if (!playSound) return;

            AudioClip gachaFailSound = SoundManager.Instance.GetClip(SoundType.GachaFail);
            audioSource.clip = gachaFailSound;
            audioSource.Play();
        }
    }
}