using TMPro;
using UnityEngine;

namespace Gacha.ui
{
    public class SaveButtonUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI saveName;
        [SerializeField] TextMeshProUGUI saveTime;
        [SerializeField] TextMeshProUGUI shopLevel;
        [SerializeField] TextMeshProUGUI shopMoney;

        public void SetSaveUI(string name, string time, string level, string money)
        {
            saveName.text = name;
            saveTime.text = time;
            shopLevel.text = level;
            shopMoney.text = money;
        }

        public void SetSaveEmpty(string name)
        {
            saveName.text = name;
            saveTime.text = "Empty";
            shopLevel.text = "";
            shopMoney.text = "";
        }
    }
}
