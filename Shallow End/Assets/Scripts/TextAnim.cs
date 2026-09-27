using UnityEngine;
using TMPro;
using System;
using System.Collections;
using Unity.VisualScripting;

public class TextAnim : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TextMeshPro;
    [SerializeField] float timeBtwnChar;
    [SerializeField] float timeBtwnWords;
    public string[] stringarray;

    int i = 0;

    void Start()
    {
        
    }

    void Endcheck()
    {
        if (i <= stringarray.Length - 1)
        {
            m_TextMeshPro.text = stringarray[i];
            StartCoroutine(TextVisible());
        }
    }

   private IEnumerator TextVisible()
    {
        m_TextMeshPro.ForceMeshUpdate();
        int totalVisibleCharacter = m_TextMeshPro.textInfo.characterCount;
        int counter = 0;
        
        while (true)
        {
            int VisibleCount = counter % (totalVisibleCharacter + 1);
            m_TextMeshPro.maxVisibleCharacters = VisibleCount; ;

            if (VisibleCount >= totalVisibleCharacter)
            {
                i += 1;
                Invoke("EndCheck", timeBtwnWords);
                break;
            }

            counter += 1;

            yield return new WaitForSeconds(timeBtwnChar);
        }
    }

  
}
