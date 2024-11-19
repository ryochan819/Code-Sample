using CarterVerseM.combat;
using CarterVerseM.interact;
using CarterVerseM.inventory;
using CarterVerseM.system;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace CarterVerseM.controls
{
    public class controls_PlayerController : MonoBehaviour, system_IDataPersistence
    {
        [Header("PlayerJoystickReferences")]
        [SerializeField] private Vector2 joyStickSize = new Vector2(200, 200);
        [SerializeField] private controls_JoyStick joyStick;
        Vector2 joyStickPosition;

        [Header("PlayerUICanvasReferences")]
        [SerializeField] GameObject uiCanvas;
        GraphicRaycaster uiGraphicRaycaster;
        PointerEventData touchData;
        List<RaycastResult> touchResults;

        [Header("TouchReferences")]
        private Finger movementFinger;
        private Vector2 movementAmount = new Vector2(0, 0);
        public Vector2 movementAmountAccess => movementAmount;
        private Finger cameraFinger;
        private Vector2 cameraAngleFingerLastTouchPosition;
        private Finger zoomFinger;
        private Vector2 zoomFingerLastTouchPosition;
        public float cameraAngleModifier = 0.15f;

        [Header("PlayerReferences")]
        Animator animator;
        public Animator animatiorAccess => animator;
        NavMeshAgent navMeshAgent;
        public NavMeshAgent navMeshAgentAccess => navMeshAgent;
        [SerializeField] private Transform bodyCamera;
        public Transform bodyCam => bodyCamera;
        bool isUI;
        bool isInteracting;
        string playerID;
        public string playerIDAccess => playerID;
        private bool canMove = true;
        public static controls_PlayerController instance { get; private set; }

        [SerializeField] AudioSource soundSource;
        float footstepRate = 1.0f;
        float nextFootStep = 0.0f;
        [SerializeField] List<SoundType> soundTypes = new List<SoundType>();
        int previousStepSoundIndex;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(this); }
            else { instance = this; }

            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
        }

        void Start()
        {
            if (playerID == null)
            {
                playerID = System.Guid.NewGuid().ToString();
            }

            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            uiGraphicRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();
            touchData = new PointerEventData(EventSystem.current);
            touchResults = new List<RaycastResult>();
        }

        void Update()
        {
            HandleTouchMovement();
        }

        private void HandleTouchMovement()
        {
            if (canMove && !combat_PlayerStat.instance.isDeadAccess)
            {
                if (movementAmount.sqrMagnitude != 0)
                {
                    Vector3 directionFromCamera = CalculateDirectionRelativeToCamera();

                    navMeshAgent.Move(directionFromCamera);

                    navMeshAgent.transform.LookAt(navMeshAgent.transform.position + directionFromCamera, Vector3.up);

                    animator.SetFloat("moveX", movementAmount.x);
                    animator.SetFloat("moveZ", movementAmount.y);

                    // check ground play walking sound
                    // PlayFootStep();

                    // reset UI
                    if(interact_PlayerInteractor.instance.interactingClickNPCAccess && interact_PlayerInteractor.instance.chatButtons.activeInHierarchy)
                    {
                        interact_PlayerInteractor.instance.ResetInteractWithPartyMember();
                    }
                }
                else
                {
                    animator.SetFloat("moveX", movementAmount.x);
                    animator.SetFloat("moveZ", movementAmount.y);
                }
            }
        }

        private void PlayFootStep()
        {
            if(nextFootStep >= 100f)
            {
                soundSource.PlayOneShot(RandomFootstepClip());
                nextFootStep = 0;
            }
            nextFootStep += (navMeshAgent.speed * movementAmount.sqrMagnitude);
        }

        private AudioClip RandomFootstepClip()
        {
            soundSource.pitch = Random.Range(0.9f, 1.1f);
            AudioClip[] clips = GetFootstepClipsByScene();
            if (clips.Length > 0) {
                int randomStepIndex = Random.Range(0, clips.Length);
                if (previousStepSoundIndex == randomStepIndex)
                {
                    randomStepIndex = (previousStepSoundIndex + 1) % clips.Length;
                }
                AudioClip selectedClip = clips[randomStepIndex];
                previousStepSoundIndex = randomStepIndex;
                return selectedClip; 
            }
            else
            {
                return null;
            }
        }

        private AudioClip[] GetFootstepClipsByScene()
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "Farm":
                    return soundTypes.Find(groundType => groundType.type == "grass").sounds;
                case "Town":
                    return soundTypes.Find(groundType => groundType.type == "stone").sounds;
                case "Home":
                    return soundTypes.Find(groundType => groundType.type == "wood").sounds;
                case "Forest":
                    return soundTypes.Find(groundType => groundType.type == "grass").sounds;
                default:
                    return soundTypes[0].sounds;
            }
        }

        private Vector3 CalculateDirectionRelativeToCamera()
        {
            // calculate scaled direction from character transform to camera direction
            Vector3 directionFromCamera = bodyCamera.TransformDirection(new Vector3(movementAmount.x, 0, movementAmount.y));
            directionFromCamera.y = 0;
            directionFromCamera = directionFromCamera.normalized * Vector3.Distance(transform.position, transform.position + new Vector3(movementAmount.x, 0, movementAmount.y)) * navMeshAgent.speed * Time.deltaTime;
            return directionFromCamera;
        }

        private void HandleFingerDown(Finger touchedFinger)
        {
            if (!isInteracting)
            {
                CheckIfTouchingUI(touchedFinger);

                if (!isUI)
                {
                    if (movementFinger == null && touchedFinger.screenPosition.x <= Screen.width / 3)
                    {
                        movementFinger = touchedFinger;
                        movementAmount = Vector2.zero;
                        joyStick.gameObject.SetActive(true);
                        joyStick.joyStickRef.sizeDelta = joyStickSize;
                        joyStick.joyStickRef.position = ClampStartPosition(touchedFinger.screenPosition);

                        // get ref of joystick position in screen space
                        joyStickPosition.x = joyStick.joyStickRef.position.x;
                        joyStickPosition.y = joyStick.joyStickRef.position.y;
                    }
                    else if (cameraFinger == null && touchedFinger.screenPosition.x > Screen.width / 3)
                    {
                        cameraFinger = touchedFinger;
                        cameraAngleFingerLastTouchPosition = cameraFinger.screenPosition;
                    }
                    else if (zoomFinger == null && touchedFinger.screenPosition.x > Screen.width / 3)
                    {
                        zoomFinger = touchedFinger;
                        zoomFingerLastTouchPosition = zoomFinger.screenPosition;
                    }
                }
            }
            else
            {
                CheckIfTouchingUI(touchedFinger);
            }
        }

        private void CheckIfTouchingUI(Finger touchedFinger)
        {
            touchData.position = touchedFinger.screenPosition;
            touchResults.Clear();
            isUI = false;

            uiGraphicRaycaster.Raycast(touchData, touchResults);
            foreach (RaycastResult result in touchResults)
            {
                if (result.gameObject.tag == "UI")
                {
                    isUI = true;
                }
            }

            // check if touched pet
            //Ray ray = Camera.main.ScreenPointToRay(touchData.position);
            //if(Physics.Raycast(ray, out RaycastHit hit))
            //{
            //    Debug.Log("Clicked object: " + hit.collider.gameObject.name);
            //}
        }

        private void HandleFingerUp(Finger releasedFinger)
        {
            if (releasedFinger == movementFinger)
            {
                movementFinger = null;
                joyStick.gameObject.SetActive(false);
                joyStick.knobRef.anchoredPosition = Vector2.zero;
                movementAmount = Vector2.zero;
            }
            else if (releasedFinger == cameraFinger)
            {
                cameraFinger = null;
                zoomFinger = null;
            }
            else if (releasedFinger == zoomFinger)
            {
                zoomFinger = null;
            }

            if (!isUI && isInteracting)
            {
                UpdateCurrentGraphicRaycaster();
                GetComponent<interact_PlayerInteractor>().interactingNPC.GetComponent<interact_NPCInteractor>().ExitChat();
                GetComponent<interact_PlayerInteractor>().ChangeBodyCamTarget();
            }
        }

        private void HandleFingerMove(Finger movedFinger)
        {

            if (movedFinger == movementFinger)
            {
                Vector2 knobPosition;
                float maxMovement = joyStickSize.x / 2;
                ETouch.Touch currentTouch = movedFinger.currentTouch;

                if (Vector2.Distance(currentTouch.screenPosition, joyStickPosition) > maxMovement)
                {
                    knobPosition = (currentTouch.screenPosition - joyStickPosition).normalized * maxMovement;
                }
                else
                {
                    knobPosition = currentTouch.screenPosition - joyStickPosition;
                }

                joyStick.knobRef.localPosition = knobPosition / 2;
                movementAmount = knobPosition / maxMovement;
            }
            else if (movedFinger == cameraFinger && zoomFinger == null)
            {
                Vector2 delta = cameraAngleFingerLastTouchPosition - movedFinger.screenPosition;
                cameraAngleFingerLastTouchPosition = movedFinger.screenPosition;

                // Convert the angle to a range of -180 to 180
                float currentAngleX = bodyCamera.eulerAngles.x;
                if (currentAngleX > 180) { currentAngleX -= 360; }

                // rotate X and Y angle, temp delta.y <1 and >1 (original 0) to fix camera occasionally flipping upside down
                if (currentAngleX > -15 && delta.y < 1)
                {
                    bodyCamera.eulerAngles += new Vector3(delta.y, 0) * cameraAngleModifier;
                }
                else if (currentAngleX < 80 && delta.y > 1)
                {
                    bodyCamera.eulerAngles += new Vector3(delta.y, 0) * cameraAngleModifier;
                }

                bodyCamera.eulerAngles -= new Vector3(0, delta.x) * cameraAngleModifier;
            }
            else if (zoomFinger != null && (movedFinger == cameraFinger || movedFinger == zoomFinger))
            {
                float originalZoomFingerDistance = Vector2.Distance(cameraAngleFingerLastTouchPosition, zoomFingerLastTouchPosition);
                float newZoomFingerDistance;
                if (movedFinger == cameraFinger)
                {
                    newZoomFingerDistance = originalZoomFingerDistance - Vector2.Distance(movedFinger.currentTouch.screenPosition, zoomFingerLastTouchPosition);
                }
                else
                {
                    newZoomFingerDistance = originalZoomFingerDistance - Vector2.Distance(movedFinger.currentTouch.screenPosition, cameraAngleFingerLastTouchPosition);
                }
                LensSettings lens = bodyCamera.GetComponent<CinemachineVirtualCamera>().m_Lens;
                if ((lens.FieldOfView < 50 && newZoomFingerDistance > 0) || (lens.FieldOfView > 25 && newZoomFingerDistance < 0))
                {
                    bodyCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView += newZoomFingerDistance * cameraAngleModifier * 0.1f;
                }
            }
        }

        private Vector2 ClampStartPosition(Vector2 startPosition)
        {
            if (startPosition.x < joyStickSize.x)
            {
                startPosition.x = joyStickSize.x;
            }
            if (startPosition.y < joyStickSize.y)
            {
                startPosition.y = joyStickSize.y;
            }
            else if (startPosition.y > Screen.height - joyStickSize.y)
            {
                startPosition.y = Screen.height - joyStickSize.y;
            }

            return startPosition;
        }

        public void PlayerInteractingWithNPC(bool value)
        {
            isInteracting = value;
        }

        public void UpdateCurrentGraphicRaycaster(GameObject canvas = null)
        {
            if (canvas == null) { canvas = uiCanvas; }
            uiGraphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        }

        public IEnumerator TempDisableMovement(float time)
        {
            canMove = false;
            yield return new WaitForSeconds(time);
            canMove = true;
        }

        public void MovementSwitch(bool CanMove)
        {
            canMove = CanMove;
        }

        private void OnEnable()
        {
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleFingerUp;
            ETouch.Touch.onFingerMove += HandleFingerMove;
        }

        private void OnDisable()
        {
            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleFingerUp;
            ETouch.Touch.onFingerMove -= HandleFingerMove;
        }

        public void LoadData(system_SaveLoadGameData data)
        {
            playerID = data.playerID;
        }

        public void SaveData(ref system_SaveLoadGameData data)
        {
            data.playerID = playerID;
        }

        // Animation Event
        public void PlayFootSound()
        {
            soundSource.PlayOneShot(RandomFootstepClip());
        }

        public void weaponSound()
        {
            soundSource.PlayOneShot(RandomWeaponClip());
        }
        private AudioClip RandomWeaponClip()
        {
            soundSource.pitch = Random.Range(0.9f, 1.1f);
            AudioClip[] clips = GetWeaponClipsByType();
            if (clips.Length > 0)
            {
                AudioClip selectedClip = clips[Random.Range(0, clips.Length)];
                return selectedClip;
            }
            else
            {
                return null;
            }
        }

        private AudioClip[] GetWeaponClipsByType()
        {
            if (inventory_InventoryManager.instance.weaponSlotAccess.equipmentData != null)
            {
                switch (inventory_InventoryManager.instance.weaponSlotAccess.equipmentData.weaponType)
                {
                    case data.weaponType.fist:
                        return soundTypes.Find(weaponType => weaponType.type == "fist").sounds;
                    case data.weaponType.sword:
                        return soundTypes.Find(weaponType => weaponType.type == "blade").sounds;
                    case data.weaponType.axe:
                        return soundTypes.Find(weaponType => weaponType.type == "blade").sounds;
                    default:
                        return soundTypes.Find(weaponType => weaponType.type == "fist").sounds;
                }
            }
            else
            {
                return soundTypes.Find(weaponType => weaponType.type == "fist").sounds;
            }
        }

        public void HoeingSound()
        {
            AudioClip[] clips = soundTypes.Find(toolType => toolType.type == "hoe").sounds;
            soundSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }

        public void WateringSound()
        {
            AudioClip[] clips = soundTypes.Find(toolType => toolType.type == "watering").sounds;
            soundSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }

        public void PlantSeedSound()
        {
            AudioClip[] clips = soundTypes.Find(toolType => toolType.type == "plantSeed").sounds;
            soundSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }

    [System.Serializable]
    public class SoundType
    {
        public string type;
        public AudioClip[] sounds;
    }
}