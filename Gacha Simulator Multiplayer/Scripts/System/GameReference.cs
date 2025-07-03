using System.Collections.Generic;
using Gacha.gameplay;
using UnityEngine;

namespace Gacha.system
{
    public class GameReference : MonoBehaviour
    {
        public static GameReference Instance { get; private set; }

        [Header ("Gacha")]
        [SerializeField] CapsuleToySetData[] localCapsuleToySets;
        public CapsuleToySetData[] LocalCapsuleToySets
        {
            get { return localCapsuleToySets; }
            set { localCapsuleToySets = value; }
        }

        [SerializeField] GameObject capsuleSize48mm;
        [SerializeField] GameObject capsuleSize65mm;
        [SerializeField] GameObject capsuleToyParent;
        public GameObject CapsuleToyParent => capsuleToyParent;

        [Header ("Build")]
        [SerializeField] private Material validPlacementMaterial;
        public Material ValidPlacementMaterial => validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        public Material InvalidPlacementMaterial => invalidPlacementMaterial;
        [SerializeField] BuildScriptable[] buildScriptables;
        public BuildScriptable[] BuildScriptables => buildScriptables;

        // For Testing Purposes
        public List<ModScriptable> modScriptables;
        public List<GameObject> GameObjects;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public GameObject GetCapsuleBySize(CapsuleSize capsuleSize)
        {
            switch (capsuleSize)
            {
                case CapsuleSize.Size48mm:
                    return capsuleSize48mm;
                case CapsuleSize.Size65mm:
                    return capsuleSize65mm;
                default:
                    return capsuleSize48mm;
            }
        }
    }
}
