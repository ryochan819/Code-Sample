using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace PurrLobby
{
    public class LobbyPlayer : MonoBehaviour
    {
        [SerializeField] LobbyManager lobbyManager;
        [SerializeField] MenuLobbyManager menuLobbyManager;
        [SerializeField] Image boarder;
        [SerializeField] RawImage avatarImage;
        [SerializeField] Button playerButton;
        [SerializeField] Button readyButton;
        [SerializeField] Button leaveButton;

        public void SetupLocalPlayer (Texture2D AvatarImage)
        {
            avatarImage.color = Color.white;
            avatarImage.texture = AvatarImage;

            readyButton.gameObject.SetActive(false);
            leaveButton.gameObject.SetActive(false);
        }

        public void SetupPlayer(LobbyUser lobbyUser, bool isOwner, bool isReady, bool isSelf)
        {
            avatarImage.color = Color.white;
            avatarImage.texture = lobbyUser.Avatar;

            playerButton.onClick.RemoveAllListeners();

            readyButton.image.color = isReady ? Color.green : Color.red;
            boarder.color = isReady ? Color.green : Color.white;
        }

        public void SetInvite(string lobbyId)
        {
            readyButton.gameObject.SetActive(false);
            leaveButton.gameObject.SetActive(false);

            playerButton.onClick.RemoveAllListeners();

            string lobbyID = lobbyId;

            avatarImage.texture = null;
            avatarImage.color = Color.grey;

            playerButton.onClick.AddListener(() =>
            {
                if (lobbyID == "-1")
                {
                    playerButton.onClick.RemoveAllListeners();
                    // start server then invite
                    menuLobbyManager.activiateInviteOverlayWhenCreateRoom = true;
                    lobbyManager.CreateRoom();
                    return;
                }
                else
                {
                    Debug.Log($"Inviting player to lobby {lobbyId}");
                    // Use steaminviteOverlay to invite player to the lobby
                    Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(ulong.Parse(lobbyID)));
                }
            });
        }
    }

    public enum LobbyPlayerState
    {
        joined,
        ready,
        invite
    }
}
