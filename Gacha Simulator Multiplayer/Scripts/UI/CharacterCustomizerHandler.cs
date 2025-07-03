using CC;
using Gacha.gameplay;
using UnityEngine;

namespace Gacha.ui
{
    public class CharacterCustomizerHandler : MonoBehaviour
    {
        [SerializeField] CC_UI_Util CC_UI_Util;

        public void CreateCharacter()
        {
            _ = GamePlayerSpawner.instance.CreateCharacter(CC_UI_Util.Customizer.StoredCharacterData);
        }
    }
}
