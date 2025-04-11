using UnityEngine;
using UnityEngine.UI;

public class HeartManager : MonoBehaviour
{
    public Sprite lostHeart;
    private Image img;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loseHeart() {

        img.sprite = lostHeart;
    }
}
