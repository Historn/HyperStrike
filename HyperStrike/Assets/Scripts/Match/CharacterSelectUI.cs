using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if NEW_INPUT_SYSTEM_INSTALLED
using UnityEngine.InputSystem.UI;
#endif

public class CharacterSelectUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI characterSelectTimerText;

    [SerializeField] private List<Button> characterSelectButtons;

    [SerializeField] private int currentSelectedButton;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;
        MatchManager.Instance.LocalCharacterSelected.OnListChanged += OnClientSelectCharacterLocal;
        MatchManager.Instance.VisitantCharacterSelected.OnListChanged += OnClientSelectCharacterVisitant;
        MatchManager.Instance.currentCharacterSelectionTime.OnValueChanged += UpdateCharSelectTimerAsText;
        MatchManager.Instance.currentCharacterSelectionTime.OnValueChanged += SetCharacterSelectActive;
    }

    private void Awake()
    {
        if (!FindAnyObjectByType<EventSystem>())
        {
            var inputType = typeof(StandaloneInputModule);
#if ENABLE_INPUT_SYSTEM && NEW_INPUT_SYSTEM_INSTALLED
                inputType = typeof(InputSystemUIInputModule);                
#endif
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), inputType);
            eventSystem.transform.SetParent(transform);
        }
    }

    private void Start()
    {
        // Automatically assign based on enum order and ensure not hardcoding assumptions
        Characters[] enumValues = (Characters[])System.Enum.GetValues(typeof(Characters));

        for (int i = 0; i < characterSelectButtons.Count && i < enumValues.Length; i++)
        {
            if (characterSelectButtons[i])
            {
                Characters character = enumValues[i];
                characterSelectButtons[i].enabled = true;
                characterSelectButtons[i].onClick.AddListener(() => MatchManager.Instance.SelectCharacter(character));
            }
        }
    }

    private void SetCharacterSelectActive(float previousValue, float newValue)
    {
        if (MatchManager.Instance.GetCurrentCharSelectTime() < 0.0f) gameObject.SetActive(false);
    }

    void UpdateCharSelectTimerAsText(float previous, float current)
    {
        if (characterSelectTimerText == null) return;

        int minutes = Mathf.FloorToInt(MatchManager.Instance.GetCurrentCharSelectTime() / 60f);
        int seconds = Mathf.FloorToInt(MatchManager.Instance.GetCurrentCharSelectTime() % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";

        characterSelectTimerText.text = timeText;
    }

    private void OnClientSelectCharacterLocal(NetworkListEvent<byte> changeEvent)
    {
        if (!MatchManager.Instance.LocalPlayersID.Contains(NetworkManager.Singleton.LocalClient.ClientId)) return;

        switch (changeEvent.PreviousValue)
        {
            case 0:
                characterSelectButtons[0].interactable = true;
                break;
            case 1:
                characterSelectButtons[1].interactable = true;
                break;
            case 2:
                characterSelectButtons[2].interactable = true;
                break;
            default:
                break;
        }

        switch (changeEvent.Value)
        {
            case 0:
                characterSelectButtons[0].interactable = false;
                break;
            case 1:
                characterSelectButtons[1].interactable = false;
                break;
            case 2:
                characterSelectButtons[2].interactable = false;
                break;
            case 3:
                characterSelectButtons[0].interactable = true;
                characterSelectButtons[1].interactable = true;
                characterSelectButtons[2].interactable = true;
                break;
            default:
                break;
        }

        
    }
    
    private void OnClientSelectCharacterVisitant(NetworkListEvent<byte> changeEvent)
    {
        if (!MatchManager.Instance.VisitantPlayersID.Contains(NetworkManager.Singleton.LocalClient.ClientId)) return;

        switch (changeEvent.PreviousValue)
        {
            case 0:
                characterSelectButtons[0].interactable = true;
                break;
            case 1:
                characterSelectButtons[1].interactable = true;
                break;
            case 2:
                characterSelectButtons[2].interactable = true;
                break;
            default:
                break;
        }

        switch (changeEvent.Value)
        {
            case 0:
                characterSelectButtons[0].interactable = false;
                break;
            case 1:
                characterSelectButtons[1].interactable = false;
                break;
            case 2:
                characterSelectButtons[2].interactable = false;
                break;
            case 3:
                characterSelectButtons[0].interactable = true;
                characterSelectButtons[1].interactable = true;
                characterSelectButtons[2].interactable = true;
                break;
            default:
                break;
        }

        
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;
        MatchManager.Instance.LocalCharacterSelected.OnListChanged -= OnClientSelectCharacterLocal;
        MatchManager.Instance.VisitantCharacterSelected.OnListChanged -= OnClientSelectCharacterVisitant;
        MatchManager.Instance.currentCharacterSelectionTime.OnValueChanged -= UpdateCharSelectTimerAsText;
        MatchManager.Instance.currentCharacterSelectionTime.OnValueChanged -= SetCharacterSelectActive;
    }
}
