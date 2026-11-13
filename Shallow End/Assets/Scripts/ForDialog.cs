using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Thomas")]
    [SerializeField] private GameObject ThomasMontgomerey;
    [SerializeField] private GameObject thomasVisual;
    [SerializeField] private Animator thomasAnimator;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera conversationCamera;

    [Header("Interaction")]
    [SerializeField] private InputActionReference interactionAction;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject interactionImage;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject backgroundImage1;
    [SerializeField] private GameObject backgroundImage2;
    [SerializeField] private GameObject thomasPortrait;
    [SerializeField] private GameObject playerPortrait;
    [SerializeField] private InputActionReference advanceDialogueAction;
    [SerializeField] private float timeBetweenCharacters = 0.03f;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject healthbar;
    [SerializeField] private GameObject overheadText;
    [SerializeField] private GameObject imageOverlap;
    [SerializeField] private GameObject centrePointer;
    [SerializeField] private GameObject[] gameplayUIToHide;
    [SerializeField] private GameObject board;

    [Header("Task Manager")]
    [SerializeField] private TaskManager taskManager;

    [Header("Gameplay Scripts")]
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private DashScript dashScript;
    [SerializeField] private Headbop headbop;
    [SerializeField] private TimeController timeController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FoodScript foodScript;
    [SerializeField] private PauseScript pauseScript;

    [Header("Random Dialogue")]
    [SerializeField] private string randomLine1 = "What you leave behind... the crabs shall find";
    [SerializeField] private string randomLine2 = "Do not believe what your eyes see in the dark";
    [SerializeField] private string randomLine3 = "The light only scares it away but the darkness will always remain";
    [SerializeField] private string randomLine4 = "The sea is a hunting ground, it is not a place of freedom";
    [SerializeField] private string randomLine5 = "The Island is beyond cruelty";

    private DialogueLine[] currentDialogue;
    private int currentLineIndex;

    private bool playerInRange;
    private bool dialogueActive;
    private bool dialogueIsPlaying;
    private bool isTyping;
    private bool conversationStarted;

    private Coroutine typingCoroutine;
    private Action dialogueFinishedCallback;

    private readonly List<Behaviour> pausedBehaviours =
        new List<Behaviour>();

    private readonly List<bool> previousBehaviourStates =
        new List<bool>();

    private readonly List<GameObject> hiddenGameplayUI =
        new List<GameObject>();

    private readonly List<bool> previousGameplayUIStates =
        new List<bool>();

    private Monster[] monsters;
    private bool[] monsterStates;

    private bool conversationCameraWasActive;
    private bool conversationCameraWasEnabled;
    private bool boardWasActive;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisibility;

    private void OnEnable()
    {
        if (interactionAction != null)
            interactionAction.action.Enable();

        if (advanceDialogueAction != null)
            advanceDialogueAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactionAction != null)
            interactionAction.action.Disable();

        if (advanceDialogueAction != null)
            advanceDialogueAction.action.Disable();
    }

    private void Start()
    {
        if (timeController == null)
            timeController = TimeController.instance;

        if (taskManager == null)
            taskManager = FindFirstObjectByType<TaskManager>();

        if (conversationCamera != null)
        {
            conversationCameraWasActive =
                conversationCamera.gameObject.activeSelf;

            conversationCameraWasEnabled =
                conversationCamera.enabled;

            conversationCamera.gameObject.SetActive(false);
        }

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

        HideInteractionUI();

        if (board != null)
            boardWasActive = board.activeSelf;
    }

    private void Update()
    {
        if (dialogueActive)
        {
            UpdateDialogue();
            return;
        }

        UpdateNightObject();

        if (!playerInRange || dialogueIsPlaying)
        {
            HideInteractionUI();
            return;
        }

        UpdateInteractionUI();

        if (!CanInteractWithCurrentTask())
            return;

        if (interactionAction == null)
            return;

        if (!interactionAction.action.WasPressedThisFrame())
            return;

        InteractWithThomas();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        UpdateInteractionUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        HideInteractionUI();
    }

    private bool CanInteractWithCurrentTask()
    {
        if (taskManager == null)
            return false;

        return
            taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasFirst) ||
            taskManager.IsCurrentTask(TaskManager.TaskID.SearchWoodenBox) ||
            taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasResources) ||
            taskManager.IsCurrentTask(TaskManager.TaskID.OpenCraftingBench) ||
            taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasNight) ||
            taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasEnding);
    }

    private void InteractWithThomas()
    {
        if (taskManager == null || dialogueIsPlaying)
            return;

        if (!CanInteractAtCurrentTime())
            return;

        if (taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasFirst))
        {
            StartFirstDialogue();
            return;
        }

        if (taskManager.IsCurrentTask(TaskManager.TaskID.SearchWoodenBox))
        {
            ShowRandomDialogue();
            return;
        }

        if (taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasResources))
        {
            StartResourcesDialogue();
            return;
        }

        if (taskManager.IsCurrentTask(TaskManager.TaskID.OpenCraftingBench))
        {
            ShowRandomDialogue();
            return;
        }

        if (taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasNight))
        {
            StartNightDialogue();
            return;
        }

        if (taskManager.IsCurrentTask(TaskManager.TaskID.SpeakToThomasEnding))
        {
            StartEndingDialogue();
        }
    }

    private void StartFirstDialogue()
    {
        DialogueLine[] dialogue =
        {
            new DialogueLine { speakerName = "Thomas", dialogueText = "Hello stranger", isThomas = true },
            new DialogueLine { speakerName = "Player", dialogueText = "Who are you?", isThomas = false },
            new DialogueLine { speakerName = "Thomas", dialogueText = "My name is Thomas Montgomery. Pleased to meet you", isThomas = true },
            new DialogueLine { speakerName = "Player", dialogueText = "What is this place? All I remember was my boat sinking and then i woke up here on the shore.", isThomas = false },
            new DialogueLine { speakerName = "Thomas", dialogueText = "Ahh I suffered a similar fate.But you will find that this island has all you need to live on the rest of your life.", isThomas = true },
            new DialogueLine { speakerName = "Player", dialogueText = "What....I can't stay here. I have to get home...", isThomas = false },
            new DialogueLine { speakerName = "Thomas", dialogueText = "Oh it's not so bad here. Sure the island has it's downsides but its quite peacful...during the day atleast.", isThomas = true },
            new DialogueLine { speakerName = "Player", dialogueText = "How long have you been here?", isThomas = false },
            new DialogueLine { speakerName = "Thomas", dialogueText = "Hmm, what year is it?.", isThomas = true },
            new DialogueLine { speakerName = "Player", dialogueText = "It's 2024", isThomas = false },
            new DialogueLine { speakerName = "Thomas", dialogueText = "I see, about 20 years..give or take.", isThomas = true },
            new DialogueLine { speakerName = "Player", dialogueText = "WHAT! I cant stay here your 20 years, there must be a way off this island", isThomas = false },
            new DialogueLine { speakerName = "Thomas", dialogueText = "Dont be so worried, ill teach you how to make your way around here", isThomas = true },
            new DialogueLine { speakerName = "Thomas", dialogueText = "Use your WASD keys to move around and head to that chest behind me, it's got my journal in it and an axe you will need to cut down trees to make a tent.", isThomas = true },
            new DialogueLine { speakerName = "Thomas", dialogueText = "Once you have the axe just 'Left Click' on a tree to cut it down. 'Left Click' is also how you interact and use other items and food.", isThomas = true }
        };

        StartThomasConversation(dialogue, FirstDialogueFinished);
    }

    private void FirstDialogueFinished()
    {
        taskManager.CompleteTask(TaskManager.TaskID.SpeakToThomasFirst);
    }

    private void StartResourcesDialogue()
    {
        DialogueLine[] dialogue =
        {
            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "I collected all the resources. Now how do I use these?",
                isThomas = false
            },

            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Good Job. Head to the crafting table to create a hammer that you can use to break open the coconuts good for your thirst",
                isThomas = true
            },
             new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "or maybe create a spear to hunt down those annoying little crabs",
                isThomas = true
            }
        };

        StartThomasConversation(dialogue, ResourcesDialogueFinished);
    }

    private void ResourcesDialogueFinished()
    {
        taskManager.CompleteTask(TaskManager.TaskID.SpeakToThomasResources);
    }

    private void StartNightDialogue()
    {
        DialogueLine[] dialogue =
        {
            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "A long day full of hard work eh? It only gets worse from here",
                isThomas = true
            },

            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "What do you mean?",
                isThomas = false
            },

            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "At nightfall there are creatures that roam this island. Deadly monsters that you have to run away from,",
                isThomas = true
            },

            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Beware they may not appear to you the first night but make no mistake they are coming, where there is no light you shall find them waiting in the shadows.",
                isThomas = true
            },

            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "How do I protect myself from them",
                isThomas = false
            },

            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Use the dash (shift) to get away if they’re too close.",
                isThomas = true
            },

            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "Is that it? Just run away?",
                isThomas = false
            },

            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "You could craft a knife to fight and kill them",
                isThomas = true
            },

            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "Now that’s more like it!",
                isThomas = false
            }
        };

        StartThomasConversation(dialogue, NightDialogueFinished);
    }

    private void NightDialogueFinished()
    {
        taskManager.CompleteTask(TaskManager.TaskID.SpeakToThomasNight);
    }

    private void StartEndingDialogue()
    {
        DialogueLine[] dialogue =
        {
            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "Thank you thomas, I wouldn’t have survived this long without you",
                isThomas = false
            },

            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "No need to thank, just helping a fellow soul. Safe travels",
                isThomas = true
            },

            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "You’re not coming with me.",
                isThomas = false
            },

            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "I have been here long enough that this place is my home now",
                isThomas = true
            },

            new DialogueLine
            {
                speakerName = "Player",
                dialogueText = "I wish you luck, friend.",
                isThomas = false
            }
        };

        StartThomasConversation(dialogue, EndingDialogueFinished);
    }

    private void EndingDialogueFinished()
    {
        taskManager.RegisterThomasEndingFinished();
    }

    private void ShowRandomDialogue()
    {
        string[] lines =
        {
            randomLine1,
            randomLine2,
            randomLine3,
            randomLine4,
            randomLine5
        };

        int randomIndex = UnityEngine.Random.Range(0, lines.Length);

        DialogueLine[] dialogue =
        {
            new DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = lines[randomIndex],
                isThomas = true
            }
        };

        StartThomasConversation(dialogue);
    }

    private void StartThomasConversation(
        DialogueLine[] dialogue,
        Action onComplete = null)
    {
        if (conversationStarted || dialogue == null || dialogue.Length == 0)
            return;

        conversationStarted = true;
        dialogueIsPlaying = true;

        SaveCursorState();
        SaveBoardState();
        PauseGameplay();
        HideGameplayUI();
        HideInteractionUI();

        if (board != null)
            board.SetActive(false);

        if (thomasAnimator != null)
            thomasAnimator.SetTrigger("TalkingThomas");

        if (conversationCamera != null)
        {
            conversationCamera.gameObject.SetActive(true);
            conversationCamera.enabled = true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartDialogue(dialogue, onComplete);
    }

    private void PauseGameplay()
    {
        pausedBehaviours.Clear();
        previousBehaviourStates.Clear();

        SaveAndDisable(healthScript);
        SaveAndDisable(dashScript);
        SaveAndDisable(headbop);
        SaveAndDisable(timeController);
        SaveAndDisable(playerController);
        SaveAndDisable(foodScript);
        SaveAndDisable(pauseScript);

        monsters = FindObjectsOfType<Monster>();
        monsterStates = new bool[monsters.Length];

        for (int i = 0; i < monsters.Length; i++)
        {
            if (monsters[i] == null)
                continue;

            monsterStates[i] = monsters[i].enabled;
            monsters[i].enabled = false;
        }

        Monster.SetDialoguePaused(true);
        MonsterAttack.SetDialoguePaused(true);
    }

    private void SaveAndDisable(Behaviour behaviour)
    {
        if (behaviour == null)
            return;

        if (pausedBehaviours.Contains(behaviour))
            return;

        pausedBehaviours.Add(behaviour);
        previousBehaviourStates.Add(behaviour.enabled);

        behaviour.enabled = false;
    }

    private void HideGameplayUI()
    {
        SaveUIState(healthbar);
        SaveUIState(overheadText);
        SaveUIState(imageOverlap);
        SaveUIState(centrePointer);

        if (gameplayUIToHide == null)
            return;

        foreach (GameObject uiObject in gameplayUIToHide)
            SaveUIState(uiObject);
    }

    private void SaveUIState(GameObject uiObject)
    {
        if (uiObject == null || hiddenGameplayUI.Contains(uiObject))
            return;

        hiddenGameplayUI.Add(uiObject);
        previousGameplayUIStates.Add(uiObject.activeSelf);

        uiObject.SetActive(false);
    }

    private void SaveCursorState()
    {
        previousCursorLock = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;
    }

    private void SaveBoardState()
    {
        if (board != null)
            boardWasActive = board.activeSelf;
    }

    public void StartDialogue(DialogueLine[] dialogue, Action onComplete = null)
    {
        if (dialogue == null || dialogue.Length == 0 || dialogueActive)
            return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueFinishedCallback = onComplete;
        dialogueActive = true;

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

        if (currentLineIndex >= currentDialogue.Length)
        {
            EndDialogue();
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

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

    private void UpdateDialogue()
    {
        bool advancePressed =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        if (!advancePressed)
            return;

        if (isTyping)
            FinishTyping();
        else
            ShowNextLine();
    }

    private void FinishTyping()
    {
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

        Action callback = dialogueFinishedCallback;
        dialogueFinishedCallback = null;

        if (thomasAnimator != null)
            thomasAnimator.SetTrigger("StopTalkingThomas");

        RestoreGameplay();

        conversationStarted = false;
        dialogueIsPlaying = false;

        callback?.Invoke();

        if (playerInRange)
            UpdateInteractionUI();
    }

    private void RestoreGameplay()
    {
        if (conversationCamera != null)
        {
            conversationCamera.enabled = conversationCameraWasEnabled;
            conversationCamera.gameObject.SetActive(conversationCameraWasActive);
        }

        for (int i = 0; i < pausedBehaviours.Count; i++)
        {
            if (pausedBehaviours[i] != null)
                pausedBehaviours[i].enabled = previousBehaviourStates[i];
        }

        if (monsters != null)
        {
            for (int i = 0; i < monsters.Length; i++)
            {
                if (monsters[i] != null)
                    monsters[i].enabled = monsterStates[i];
            }
        }

        Monster.SetDialoguePaused(false);
        MonsterAttack.SetDialoguePaused(false);

        for (int i = 0; i < hiddenGameplayUI.Count; i++)
        {
            if (hiddenGameplayUI[i] != null)
                hiddenGameplayUI[i].SetActive(previousGameplayUIStates[i]);
        }

        hiddenGameplayUI.Clear();
        previousGameplayUIStates.Clear();
        pausedBehaviours.Clear();
        previousBehaviourStates.Clear();

        if (board != null)
            board.SetActive(boardWasActive);

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisibility;
    }

    private float GetCurrentHour()
    {
        if (timeController == null)
            timeController = TimeController.instance;

        if (timeController == null)
            return 12f;

        return (float)timeController.CurrentTime.TimeOfDay.TotalHours;
    }

    private bool CanInteractAtCurrentTime()
    {
        float currentHour = GetCurrentHour();

        return currentHour >= 6f && currentHour < 21f;
    }

    private void UpdateNightObject()
    {
        if (thomasVisual == null)
            return;

        bool isDay = CanInteractAtCurrentTime();

        if (thomasVisual.activeSelf != isDay)
            thomasVisual.SetActive(isDay);
    }

    private void UpdateInteractionUI()
    {
        if (!playerInRange ||
            dialogueIsPlaying ||
            !CanInteractAtCurrentTime() ||
            !CanInteractWithCurrentTask())
        {
            HideInteractionUI();
            return;
        }

        if (interactionText != null)
        {
            interactionText.text = "Interact with Thomas";
            interactionText.gameObject.SetActive(true);
        }

        if (interactionImage != null)
            interactionImage.SetActive(true);
    }

    private void HideInteractionUI()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        if (interactionImage != null)
            interactionImage.SetActive(false);
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
}