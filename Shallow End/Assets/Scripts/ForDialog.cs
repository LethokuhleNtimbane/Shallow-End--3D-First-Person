
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ForDialog : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string dialogueText;
        public bool isThomas;
    }

    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private GameObject backgroundImage1;
    [SerializeField] private GameObject backgroundImage2;

    [SerializeField] private GameObject thomasPortrait;
    [SerializeField] private GameObject playerPortrait;

    [SerializeField] private GameObject healthbar;
    [SerializeField] private GameObject overheadText;
    [SerializeField] private GameObject imageOverlap;
    [SerializeField] private GameObject centrePointer;

    [SerializeField] private InputActionReference advanceDialogueAction;

    [SerializeField] private float timeBetweenCharacters = 0.03f;

    [SerializeField] private TimeController timeController;
    [SerializeField] private GameObject ThomasMontgomerey;

    private DialogueLine[] currentDialogue;
    private int currentLineIndex;

    private bool dialogueActive;
    private bool isTyping;

    private Coroutine typingCoroutine;
    private Action dialogueFinishedCallback;

    private bool healthbarWasActive;
    private bool overheadTextWasActive;
    private bool imageOverlapWasActive;
    private bool centrePointerWasActive;

    private void OnEnable()
    {
        if (advanceDialogueAction != null)
            advanceDialogueAction.action.Enable();
    }

    private void OnDisable()
    {
        if (advanceDialogueAction != null)
            advanceDialogueAction.action.Disable();
    }

    private void Start()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (backgroundImage1 != null)
            backgroundImage1.SetActive(false);

        if (backgroundImage2 != null)
            backgroundImage2.SetActive(false);

        if (thomasPortrait != null)
            thomasPortrait.SetActive(false);

        if (playerPortrait != null)
            playerPortrait.SetActive(false);

        if (speakerText != null)
            speakerText.text = "";

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        if (timeController == null)
            timeController = TimeController.instance;

        UpdateNightObject();
    }

    private void Update()
    {
        UpdateNightObject();

        if (!dialogueActive || advanceDialogueAction == null)
            return;

        if (!advanceDialogueAction.action.WasPressedThisFrame())
            return;

        if (isTyping)
            FinishTyping();
        else
            ShowNextLine();
    }

    private void UpdateNightObject()
    {
        if (ThomasMontgomerey == null)
            return;

        if (timeController == null)
            timeController = TimeController.instance;

        if (timeController == null)
            return;

        float currentHour =
            (float)timeController.CurrentTime.TimeOfDay.TotalHours;

        bool isNight = currentHour >= 21f || currentHour < 6f;

        ThomasMontgomerey.SetActive(!isNight);
    }

    public void StartDialogue(
        DialogueLine[] dialogue,
        Action onComplete = null)
    {
        if (dialogue == null || dialogue.Length == 0 || dialogueActive)
            return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueFinishedCallback = onComplete;
        dialogueActive = true;

        SaveObjectStates();
        HideGameplayObjects();

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        if (backgroundImage1 != null)
            backgroundImage1.SetActive(true);

        if (backgroundImage2 != null)
            backgroundImage2.SetActive(true);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (!dialogueActive || currentDialogue == null)
            return;

        if (currentLineIndex < 0 || currentLineIndex >= currentDialogue.Length)
        {
            EndDialogue();
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        DialogueLine line = currentDialogue[currentLineIndex];

        if (speakerText != null)
            speakerText.text = line.speakerName;

        UpdatePortrait(line.isThomas);

        if (dialogueText != null)
        {
            dialogueText.text = line.dialogueText ?? "";
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();
        }

        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    private IEnumerator TypeDialogue()
    {
        isTyping = true;

        if (dialogueText == null)
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        dialogueText.ForceMeshUpdate();

        int characterCount = dialogueText.textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            if (!dialogueActive)
                yield break;

            dialogueText.maxVisibleCharacters = i + 1;

            if (timeBetweenCharacters > 0f)
                yield return new WaitForSeconds(timeBetweenCharacters);
            else
                yield return null;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;

        isTyping = false;
        typingCoroutine = null;
    }

    private void FinishTyping()
    {
        if (!isTyping)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
            dialogueText.maxVisibleCharacters = int.MaxValue;

        isTyping = false;
    }

    private void ShowNextLine()
    {
        if (!dialogueActive || currentDialogue == null)
            return;

        currentLineIndex++;
        ShowCurrentLine();
    }

    private void UpdatePortrait(bool isThomas)
    {
        if (thomasPortrait != null)
            thomasPortrait.SetActive(isThomas);

        if (playerPortrait != null)
            playerPortrait.SetActive(!isThomas);
    }

    private void SaveObjectStates()
    {
        if (healthbar != null)
            healthbarWasActive = healthbar.activeSelf;

        if (overheadText != null)
            overheadTextWasActive = overheadText.activeSelf;

        if (imageOverlap != null)
            imageOverlapWasActive = imageOverlap.activeSelf;

        if (centrePointer != null)
            centrePointerWasActive = centrePointer.activeSelf;
    }

    private void HideGameplayObjects()
    {
        if (healthbar != null)
            healthbar.SetActive(false);

        if (overheadText != null)
            overheadText.SetActive(false);

        if (imageOverlap != null)
            imageOverlap.SetActive(false);

        if (centrePointer != null)
            centrePointer.SetActive(false);
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        dialogueActive = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (backgroundImage1 != null)
            backgroundImage1.SetActive(false);

        if (backgroundImage2 != null)
            backgroundImage2.SetActive(false);

        if (thomasPortrait != null)
            thomasPortrait.SetActive(false);

        if (playerPortrait != null)
            playerPortrait.SetActive(false);

        if (speakerText != null)
            speakerText.text = "";

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }

        RestoreGameplayObjects();

        Action callback = dialogueFinishedCallback;
        dialogueFinishedCallback = null;

        callback?.Invoke();
    }

    private void RestoreGameplayObjects()
    {
        if (healthbar != null)
            healthbar.SetActive(healthbarWasActive);

        if (overheadText != null)
            overheadText.SetActive(overheadTextWasActive);

        if (imageOverlap != null)
            imageOverlap.SetActive(imageOverlapWasActive);

        if (centrePointer != null)
            centrePointer.SetActive(centrePointerWasActive);
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
}
