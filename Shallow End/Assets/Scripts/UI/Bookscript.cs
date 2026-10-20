
using UnityEngine;
using UnityEngine.UI;

public class Bookscript : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private Image leftPage;
    [SerializeField] private Image rightPage;

    [Header("Page Sprites")]
    [SerializeField] private Sprite page1;
    [SerializeField] private Sprite page2;
    [SerializeField] private Sprite page3;
    [SerializeField] private Sprite page4;
    [SerializeField] private Sprite page5;
    [SerializeField] private Sprite page6;
    [SerializeField] private Sprite page7;
    [SerializeField] private Sprite page8;
    [SerializeField] private Sprite page9;
    [SerializeField] private Sprite page10;
    [SerializeField] private Sprite page11;
    [SerializeField] private Sprite page12;
    [SerializeField] private Sprite page13;
    [SerializeField] private Sprite page14;
    [SerializeField] private Sprite page15;

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    private int currentPage = 1;

    private void Start()
    {
        currentPage = 1;
        UpdatePages();
    }

    public void NextPage()
    {
        if (currentPage < 15)
        {
            currentPage += 2;

            if (currentPage > 15)
                currentPage = 15;

            UpdatePages();
        }
    }

    public void BackPage()
    {
        if (currentPage > 1)
        {
            currentPage -= 2;

            if (currentPage < 1)
                currentPage = 1;

            UpdatePages();
        }
    }

    private void UpdatePages()
    {
        // Update the left page.
        switch (currentPage)
        {
            case 1: leftPage.sprite = page1; break;
            case 3: leftPage.sprite = page3; break;
            case 5: leftPage.sprite = page5; break;
            case 7: leftPage.sprite = page7; break;
            case 9: leftPage.sprite = page9; break;
            case 11: leftPage.sprite = page11; break;
            case 13: leftPage.sprite = page13; break;
            case 15: leftPage.sprite = page15; break;
        }

        // Update the right page.
        switch (currentPage)
        {
            case 1:
                rightPage.sprite = page2;
                rightPage.gameObject.SetActive(true);
                break;

            case 3:
                rightPage.sprite = page4;
                rightPage.gameObject.SetActive(true);
                break;

            case 5:
                rightPage.sprite = page6;
                rightPage.gameObject.SetActive(true);
                break;

            case 7:
                rightPage.sprite = page8;
                rightPage.gameObject.SetActive(true);
                break;

            case 9:
                rightPage.sprite = page10;
                rightPage.gameObject.SetActive(true);
                break;

            case 11:
                rightPage.sprite = page12;
                rightPage.gameObject.SetActive(true);
                break;

            case 13:
                rightPage.sprite = page14;
                rightPage.gameObject.SetActive(true);
                break;

            case 15:
                rightPage.gameObject.SetActive(false);
                break;
        }

        // Update navigation buttons.
        if (backButton != null)
            backButton.interactable = currentPage > 1;

        if (nextButton != null)
            nextButton.interactable = currentPage < 15;
    }
}
