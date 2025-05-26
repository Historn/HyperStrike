using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI characterSelectTimerText;

    [SerializeField] private List<Button> characterSelectButtons;

    private void OnEnable()
    {
        MatchManager.Instance.characterSelectionTime.OnValueChanged += (previous, current) => UpdateCharSelectTimerAsText();
        MatchManager.Instance.characterSelectionTime.OnValueChanged += (previous, current) => { if (MatchManager.Instance.GetCurrentCharSelectTime() < 0.0f) gameObject.SetActive(false); };
        if (IsClient)
        {
            MatchManager.Instance.CharacterSelected.OnListChanged += (_) => OnClientSelectCharacter();
        }
    }

    private void Start()
    {
        for (int i = 0; i < characterSelectButtons.Count; i++)
        {
            if (characterSelectButtons[i])
            {
                characterSelectButtons[i].onClick.AddListener(() => MatchManager.Instance.SelectCharacter(Characters.SPEED));
                characterSelectButtons[i].onClick.AddListener(() => { Debug.Log($"Client pressed button"); });
            }
        }
    }

    void UpdateCharSelectTimerAsText()
    {
        if (characterSelectTimerText == null) return;

        int minutes = Mathf.FloorToInt(MatchManager.Instance.GetCurrentCharSelectTime() / 60f);
        int seconds = Mathf.FloorToInt(MatchManager.Instance.GetCurrentCharSelectTime() % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";

        characterSelectTimerText.text = timeText;
    }

    public void OnClientSelectCharacter() // NO VA DE MOMENTO
    {
        if (IsClient)
        {
            for (int i = 0; i < MatchManager.Instance.CharacterSelected.Count; i++)
            {
                var players = NetworkManager.Singleton.ConnectedClientsList;
                var byteCharacter = MatchManager.Instance.CharacterSelected[i];
                if (byteCharacter != (byte)Characters.NONE)
                {
                    //if ((MatchManager.Instance.LocalPlayersID.Contains(players[i].ClientId) && MatchManager.Instance.LocalPlayersID.Contains(NetworkManager.Singleton.LocalClientId))
                    //    || (MatchManager.Instance.VisitantPlayersID.Contains(players[i].ClientId) && MatchManager.Instance.VisitantPlayersID.Contains(NetworkManager.Singleton.LocalClientId)))
                    //{
                    //    characterSelectButtons[byteCharacter].enabled = false;
                    //}
                    characterSelectButtons[byteCharacter].enabled = false;
                }
            }
        }
    }
}
