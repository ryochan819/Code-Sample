using System.Collections;
using System.Collections.Generic;
using BezierSolution;
using Sushi.control;
using Sushi.tutorial;
using UnityEngine;

namespace Sushi.gameplay
{
    public class BeltController : MonoBehaviour, ISushiBeltInteractable
    {
        [SerializeField] BezierSpline bezierSpline; // Spline path the sushi plates will follow
        Outline outline;
        InteractableType interactableType = InteractableType.SushiBelt;
        List<GameObject> sushiPlates = new List<GameObject>(); // List of sushi plates on the belt
        public List<GameObject> SushiPlates { get { return sushiPlates; } }

        private void Start()
        {
            outline = GetComponent<Outline>();
        }

        public void EnableOutline(bool enable)
        {
            if (outline != null)
            outline.enabled = enable;
        }

        public InteractableType GetInteractableType()
        {
            return interactableType;
        }

        public void Interact(PlayerState playerState)
        {
            // Reserved for interaction logic with the belt (e.g., player clicks or triggers)
        }

        public bool PlaceSushiPlate(GameObject sushiPlate, Vector3 hitPoint)
        {
            BezierWalkerWithSpeed bezierWalker = sushiPlate.GetComponent<BezierWalkerWithSpeed>();
            if (bezierWalker != null)
            {           
                // Progress tutorial stage 
                TutorialEventSystem.SwitchTutorialStage(TutorialStage.Task2);

                // Smoothly move plate to hit point before activating belt movement
                LeanTween.move(sushiPlate, hitPoint, 0.3f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
                {
                    SetBezierWalker(sushiPlate, hitPoint, bezierWalker);
                    sushiPlate.GetComponent<SushiPlate>().SushiPlatePlacedSound();
                });
                
                return true;
            }
            return false;
        }

        private void SetBezierWalker(GameObject sushiPlate, Vector3 hitPoint, BezierWalkerWithSpeed bezierWalker)
        {
            bezierWalker.spline = bezierSpline;

            // Find the closest point on the spline to start the plate's movement
            float normalizedT;
            bezierSpline.FindNearestPointTo(hitPoint, out normalizedT);
            bezierWalker.NormalizedT = normalizedT;

             // Set up plate for conveyor movement
            sushiPlate.layer = LayerMask.NameToLayer("SushiPlate");
            sushiPlates.Add(sushiPlate);
            sushiPlate.transform.parent = GameReferenceManager.instance.PlateHolder.transform;
            sushiPlate.GetComponent<Collider>().isTrigger = true;
            sushiPlate.GetComponent<SushiPlate>().SetSushiPlateBelt(gameObject);

            bezierWalker.enabled = true;
            
            sushiPlate.GetComponent<SushiPlate>().SetSushiPlateState(SushiPlateState.PlacedOnBelt);
        }

        public void PlaceSushiPlateOnLoad(GameObject sushiPlate, float normalizedT)
        {
            // Used when loading a saved game — restore plate position
            BezierWalkerWithSpeed bezierWalker = sushiPlate.GetComponent<BezierWalkerWithSpeed>();
            if (bezierWalker != null)
            {            
                SetBezierWalkerLoad(sushiPlate, bezierWalker, normalizedT);
            }
        }

        private void SetBezierWalkerLoad(GameObject sushiPlate, BezierWalkerWithSpeed bezierWalker, float normalizedT)
        {
            bezierWalker.spline = bezierSpline;
            bezierWalker.NormalizedT = normalizedT;

            sushiPlate.layer = LayerMask.NameToLayer("SushiPlate");
            sushiPlates.Add(sushiPlate);
            sushiPlate.transform.parent = GameReferenceManager.instance.PlateHolder.transform;
            sushiPlate.GetComponent<Collider>().isTrigger = true;
            sushiPlate.GetComponent<SushiPlate>().SetSushiPlateBelt(gameObject);

            // Disabled and let Sushi Plate finish the setup
            bezierWalker.enabled = false;

            sushiPlate.GetComponent<SushiPlate>().SetSushiPlateState(SushiPlateState.PlacedOnBelt);
        }

        public bool SushiSpaceAvaliableOnBelt()
        {
            // Determine if there’s enough space on the belt for another plate - Use for Chef AI decision
            Debug.Log("line bezierSpline.length: " + bezierSpline.length + ", sushiPlates.Count: " + sushiPlates.Count);
            return sushiPlates.Count * 0.2f < bezierSpline.length;
        }

        public void HandleSushiPlateList(GameObject gameObject, bool reigster)
        {
            if (reigster && sushiPlates.Contains(gameObject))
            {
                sushiPlates.Add(gameObject);
            }
            else if (!reigster && sushiPlates.Contains(gameObject))
            {
                sushiPlates.Remove(gameObject);
            }
        }
    }
}