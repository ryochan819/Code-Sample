using Gacha.gameplay;
using Gacha.system;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gacha.ui
{
    public class BuildButton : MonoBehaviour
    {
        [SerializeField] BuildScriptable buildScriptable;
        [SerializeField] Button button;
        [SerializeField] Image buttonImage;
        [SerializeField] TextMeshProUGUI buttonText;

        void Start()
        {
            buttonImage.sprite = buildScriptable.buildImage;
            buttonText.text = buildScriptable.itemName;

            button.onClick.AddListener(() => BuildGameObject());
        }

        private void BuildGameObject()
        {
            if (GameSceneDataManager.instance.shopLevel >= buildScriptable.unlockLevel && GameSceneDataManager.instance.money.value >= buildScriptable.cost)
            {
                Debug.Log("Requirement met");
                // Requirement met
                BuildManager.Instance.StartBuildMode(buildScriptable);
                UIEventSystem.ClosePhoneMenu();
            }
            else
            {
                // show warning
                Debug.Log($"Cannot build: RequiredLevel={buildScriptable.unlockLevel}, PlayerLevel={GameSceneDataManager.instance.shopLevel}, RequiredCost={buildScriptable.cost}, PlayerMoney={GameSceneDataManager.instance.money.value}");
            }
        }

        void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
