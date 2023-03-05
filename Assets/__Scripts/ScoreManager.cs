using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss,
}

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [SerializeField] private int chain = 0;
    [SerializeField] private int scoreRun = 0;
    [SerializeField] private int score = 0;
    [SerializeField] private int goldCards = 0;

    static public int CHAIN { get { return S.chain; } }

    static public int SCORE_RUN { get { return S.scoreRun; } }
    static public int SCORE { get { return S.score; } }
    static public int GOLD_CARDS { get { return S.goldCards; } }

    private void Awake()
    {
        if (S != null)
        {
            Debug.Log("ScoreManager.S is already set!");
            Destroy(this.gameObject);
        }
        else
        {
            S = this;
        }

        if (PlayerPrefs.HasKey("HIGH_SCORE"))
            HIGH_SCORE = PlayerPrefs.GetInt("HIGH_SCORE");

        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;

        DontDestroyOnLoad(this.gameObject);
    }

    static public void EVENT(ScoreEvent scoreEvent)
    {
        try
        {
            S.Event(scoreEvent);
        }
        catch(System.NullReferenceException exception)
        {
            Debug.LogError("ScoreManager: Event() is called, while S = null\n" + exception);
        }
    }

    private void Event(ScoreEvent scoreEvent)
    {
        switch (scoreEvent)
        {
            case ScoreEvent.draw:
            case ScoreEvent.gameWin:
            case ScoreEvent.gameLoss:
                chain = 0;
                scoreRun *= (int)Mathf.Pow(2, goldCards);
                score += scoreRun;
                goldCards = 0;
                scoreRun = 0;
                break;
            case ScoreEvent.mine:
                chain++;
                scoreRun += chain;
                break;
            case ScoreEvent.mineGold:
                chain++;
                scoreRun += chain;
                goldCards++;
                break;
        }

        switch (scoreEvent) 
        {
            case ScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                Debug.Log("Score for this round: " + SCORE_FROM_PREV_ROUND + " Total score: " + score);
                break;
            case ScoreEvent.gameLoss:
                if (HIGH_SCORE < score)
                {
                    PlayerPrefs.SetInt("HIGH_SCORE", score);
                    Debug.Log("You lost! New high score: " + score);
                }
                else
                {
                    Debug.Log("You lost! Your score is:" + score);
                }
                score = 0;
                break;  
        }
    }
}
