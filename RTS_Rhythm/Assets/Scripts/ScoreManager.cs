using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour   // handles sound effects and combos
{
    public static ScoreManager instance;
    public AudioSource hitSFX;
    public AudioSource missSFX;

    public static TMPro.TextMeshPro scoreText;
    static int comboScore;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        comboScore = 0;
    }

    public static void Hit()
    {
        comboScore += 1;
        instance.hitSFX.Play();

        scoreText.text = comboScore.ToString();
    }

    public static void Miss()
    {
        comboScore = 0;
        instance.missSFX.Play();

        scoreText.text = comboScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
