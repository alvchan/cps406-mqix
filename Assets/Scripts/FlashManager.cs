using UnityEngine;
using TMPro;
public class FlashManager : MonoBehaviour
{
    private TMP_Text m_TextComponent;
   
    private void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();

    }

    // Update is called once per frame
    void Update()
    {
        if (m_TextComponent.alpha > 0)
        {
            m_TextComponent.alpha -= (float)0.25;
        }
    }
}
