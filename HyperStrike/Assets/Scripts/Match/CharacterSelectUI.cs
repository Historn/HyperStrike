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
        MatchManager.Instance.CharacterSelected.OnListChanged += OnClientSelectCharacter;
        MatchManager.Instance.characterSelectionTime.OnValueChanged += UpdateCharSelectTimerAsText;
        MatchManager.Instance.characterSelectionTime.OnValueChanged += (previous, current) => { if (MatchManager.Instance.GetCurrentCharSelectTime() < 0.0f) gameObject.SetActive(false); };
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

                characterSelectButtons[i].onClick.AddListener(() => MatchManager.Instance.SelectCharacter(character));
            }
        }
    }

    void UpdateCharSelectTimerAsText(float previous, float current)
    {
        if (characterSelectTimerText == null) return;

        int minutes = Mathf.FloorToInt(MatchManager.Instance.GetCurrentCharSelectTime() / 60f);
        int seconds = Mathf.FloorToInt(MatchManager.Instance.GetCurrentCharSelectTime() % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";

        characterSelectTimerText.text = timeText;
    }

    private void OnClientSelectCharacter(NetworkListEvent<byte> changeEvent)
    {
        for (int i = 0; i < MatchManager.Instance.CharacterSelected.Count; i++)
        {
            var byteCharacter = MatchManager.Instance.CharacterSelected[i];
            if ((Characters)byteCharacter == Characters.NONE)
            {
                //characterSelectButtons[currentSelectedButton].interactable = true;
            }
        }
    }

    private void OnDisable()
    {
        if (!IsOwner) return;
        MatchManager.Instance.CharacterSelected.OnListChanged -= OnClientSelectCharacter;
    }
}
