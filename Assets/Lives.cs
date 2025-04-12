using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Lives : MonoBehaviour
{
    private int lives = 3;
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void decrementHeart()
    {
        lives -= 1;
    }

    public int getLives()
    {
        return lives;
    }
}
