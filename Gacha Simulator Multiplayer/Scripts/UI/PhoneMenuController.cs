using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gacha.ui
{
    public class PhoneMenuController : MonoBehaviour
    {
        [SerializeField] private List<PageMapping> pageMappings;

        private Dictionary<Button, GameObject> buttonToPageMap = new();
        private GameObject lastActivePage;

        void Start()
        {
            foreach (var mapping in pageMappings)
            {
                if (mapping.activeButton != null && mapping.page != null)
                {
                    buttonToPageMap[mapping.activeButton] = mapping.page;
                    mapping.activeButton.onClick.AddListener(() => OnPageButtonClicked(mapping.page));
                }

                if (mapping.closeButton != null && mapping.page != null)
                {
                    mapping.closeButton.onClick.AddListener(() => OnCloseButtonClicked(mapping.page));
                }
            }

            UIEventSystem.onClosePhoneMenu += closePhoneMenu;
        }

        private void OnPageButtonClicked(GameObject targetPage)
        {
            lastActivePage?.SetActive(false);

            targetPage.SetActive(true);
            lastActivePage = targetPage;
        }

        private void OnCloseButtonClicked(GameObject targetPage)
        {
            targetPage.SetActive(false);
        }

        private void closePhoneMenu()
        {
            lastActivePage?.SetActive(false);
            lastActivePage = null;
        }

        void OnDestroy()
        {
            foreach (var mapping in pageMappings)
            {
                mapping.activeButton?.onClick.RemoveAllListeners();
                mapping.closeButton?.onClick.RemoveAllListeners();
            }

            UIEventSystem.onClosePhoneMenu -= closePhoneMenu;
        }

        [System.Serializable]
        public class PageMapping
        {
            public Button activeButton;
            public Button closeButton;
            public GameObject page;
        }
    }
}
