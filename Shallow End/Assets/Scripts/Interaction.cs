using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Interaction : MonoBehaviour
{
    [SerializeField] private InputActionReference interactAction;

    [SerializeField] private GameObject thomas;

    [SerializeField] private GameObject board;

    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject interactionImage;

    [SerializeField] private ForDialog dialogueManager;

    [SerializeField] private TaskManager taskManager;

    [SerializeField] private TimeController timeController;
    [SerializeField] private Monster monster;
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private DashScript dash;
    [SerializeField] private PlayerController playerController;

    [SerializeField]
    private string randomLine1 =
        "What you leave behind... the crabs shall find";

    [SerializeField]
    private string randomLine2 =
        "Do not believe what your eyes see in the dark";

    [SerializeField]
    private string randomLine3 =
        "The light only scares it away but the darkness will always remain";

    [SerializeField]
    private string randomLine4 =
        "The sea is a hunting ground, it is not a place of freedom";

    [SerializeField]
    private string randomLine5 =
        "The Island is beyond cruelty";

    private bool playerInRange = false;
    private bool dialogueIsPlaying = false;

 

    private void Start()
    {
        HideInteractionUI();
    }

    private void Update()
    {
        if (playerInRange && !dialogueIsPlaying)
        {
            UpdateInteractionUI();
        }

        if (!playerInRange)
            return;

        if (dialogueIsPlaying)
            return;

        if (interactAction == null)
            return;

        if (!interactAction.action.WasPressedThisFrame())
            return;

        if (dialogueManager != null &&
            dialogueManager.IsDialogueActive())
            return;

        InteractWithThomas();
    }


    private void InteractWithThomas()
    {
        if (taskManager == null || dialogueManager == null)
            return;

        if (!CanInteractAtCurrentTime())
            return;

        if (taskManager.IsCurrentTask(
            TaskManager.TaskID.SpeakToThomasFirst))
        {
            StartFirstDialogue();
            return;
        }

        if (taskManager.IsCurrentTask(
            TaskManager.TaskID.SpeakToThomasSecond))
        {
            if (HasCompletedRequiredTasks())
            {
                StartSecondDialogue();
            }
            else
            {
                ShowRandomDialogue();
            }

            return;
        }

        ShowRandomDialogue();
    }

    private bool HasCompletedRequiredTasks()
    {
        return taskManager.IsTaskCompleted(
                   TaskManager.TaskID.SurviveNight)
            && taskManager.IsTaskCompleted(
                   TaskManager.TaskID.CraftTent)
            && taskManager.IsTaskCompleted(
                   TaskManager.TaskID.BuildRaft);
    }


    private void DisableGameplay()
    {
      
        Monster.SetDialoguePaused(true);
        MonsterAttack.SetDialoguePaused(true);

        if (timeController != null)
            timeController.enabled = false;

        if (healthScript != null)
            healthScript.enabled = false;

        if (dash != null)
            dash.enabled = false;

        if (playerController != null)
            playerController.enabled = false;
    }

    private void EnableGameplay()
    {
   
        Monster.SetDialoguePaused(false);
        MonsterAttack.SetDialoguePaused(false);

        if (timeController != null)
            timeController.enabled = true;

        if (healthScript != null)
            healthScript.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (playerController != null)
            playerController.enabled = true;
    }

    private void HideBoard()
    {
        if (board != null)
            board.SetActive(false);
    }

    private void ShowBoard()
    {
        if (board != null)
            board.SetActive(true);
    }

    private void StartFirstDialogue()
    {
        dialogueIsPlaying = true;

        DisableGameplay();
        HideInteractionUI();
        HideBoard();

        ForDialog.DialogueLine[] dialogue =
        {
            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Hello stranger",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "Who are you?",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "My name is Thomas Montgomery. Pleased to meet you",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "What is this place? All I remember was my boat sinking and then i woke up here on the shore.",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Ahh I suffered a similar fate.But you will find that this island has all you need to live on the rest of your life.",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "What....I can't stay here. I have to get home...",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Oh it's not so bad here. Sure the island has it's downsides but its quite peacful...during the day atleast.",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "How long have you been here?",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Hmm, what year is it?.",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "It's 2024",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "I see, about 20 years..give or take.",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "WHAT! I cant stay here your 20 years, there must be a way off this island",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Dont be so worried, ill teach you how to make your way around here",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Use your WASD keys to move around and head to that chest behind me, it's got my journal in it and an axe you will need to cut down trees to make  tent.",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Once you have the axe just 'Left Click' on a tree to cut it down. 'Left Click' is also how you interact and use other items and food.",
                isThomas = true
            },
        };

        dialogueManager.StartDialogue(
            dialogue,
            FirstDialogueFinished
        );
    }

    private void FirstDialogueFinished()
    {
        dialogueIsPlaying = false;

        ShowBoard();
        EnableGameplay();

        taskManager.CompleteTask(
            TaskManager.TaskID.SpeakToThomasFirst
        );

        UpdateInteractionUI();
    }

    private void StartSecondDialogue()
    {
        dialogueIsPlaying = true;

        DisableGameplay();
        HideInteractionUI();
        HideBoard();

        ForDialog.DialogueLine[] dialogue =
        {
            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "I see you have built yourself a shelter",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "I don't plan to stay here for long though",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "I used to have the same thoughts",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "What changed?",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Life is simple here.",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "There are monsters here!",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "When you have lived with them for so long, you don't see them that way",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "You're crazy!",
                isThomas = false
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = "Please. Let this place become your home. We could all be friends forever",
                isThomas = true
            },

            new ForDialog.DialogueLine
            {
                speakerName = "Player",
                dialogueText = "You're insane. I'm getting off this island.",
                isThomas = false
            }
        };

        dialogueManager.StartDialogue(
            dialogue,
            SecondDialogueFinished
        );
    }

    private void SecondDialogueFinished()
    {
        dialogueIsPlaying = false;

        ShowBoard();
        EnableGameplay();

        taskManager.CompleteTask(
            TaskManager.TaskID.SpeakToThomasSecond
        );

        UpdateInteractionUI();
    }

    private void ShowRandomDialogue()
    {
        dialogueIsPlaying = true;

        DisableGameplay();
        HideInteractionUI();
        HideBoard();

        string[] dialogueLines =
        {
            randomLine1,
            randomLine2,
            randomLine3,
            randomLine4,
            randomLine5
        };

        int randomIndex =
            Random.Range(0, dialogueLines.Length);

        ForDialog.DialogueLine[] dialogue =
        {
            new ForDialog.DialogueLine
            {
                speakerName = "Thomas",
                dialogueText = dialogueLines[randomIndex],
                isThomas = true
            }
        };

        dialogueManager.StartDialogue(
            dialogue,
            RandomDialogueFinished
        );
    }

    private void RandomDialogueFinished()
    {
        dialogueIsPlaying = false;

        ShowBoard();
        EnableGameplay();

        UpdateInteractionUI();
    }

    private float GetCurrentHour()
    {
        if (timeController == null)
            return 12f;

        return (float)
            timeController.CurrentTime.TimeOfDay.TotalHours;
    }

    private bool CanInteractAtCurrentTime()
    {
        float currentHour = GetCurrentHour();

        if (currentHour >= 21f || currentHour < 6f)
            return false;

        return true;
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

    private void UpdateInteractionUI()
    {
        if (!playerInRange || dialogueIsPlaying)
        {
            HideInteractionUI();
            return;
        }

        if (!CanInteractAtCurrentTime())
        {
            HideInteractionUI();
            return;
        }

        ShowInteractionUI();
    }

    private void ShowInteractionUI()
    {
        if (interactionText != null)
        {
            interactionText.text = "interact with Thomas";
            interactionText.gameObject.SetActive(true);
        }

        if (interactionImage != null)
        {
            interactionImage.SetActive(true);
        }
    }

    private void HideInteractionUI()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (interactionImage != null)
        {
            interactionImage.SetActive(false);
        }
    }
}