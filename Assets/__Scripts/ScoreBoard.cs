using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    static public ScoreBoard S;
    public Transform canvasTransform;

    [SerializeField]private int score = 0;
    [SerializeField] private string scoreString;

    [SerializeField] private ResponsibleUIText scoreBoard, scoreRun, scoreGoldCards;
    [SerializeField] private GameObject prefabFloatingScore;
    [SerializeField] private Vector2 posScoreRun = new Vector2(0f, 6f), posScoreGoldCards = new Vector2(2f, 6f);
    [SerializeField] private float floatingScoreSpeed = 0.5f;

    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            ScoreString = value.ToString("N0");
        }
    }

    public string ScoreString
    {
        get { return scoreString; }
        set
        {
            scoreString = value;
            GetComponent<Text>().text = value;
        }
    }

    private void Awake()
    {
        if(S == null)
        {
            S = this;
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("ScoreBoard.S is already set!");
        }

        scoreBoard = GetComponent<ResponsibleUIText>();

        DontDestroyOnLoad(this.gameObject);
    }

    public void FloatingScoreHandler(ScoreEvent scoreEvent)
    {
        switch (scoreEvent)
        {
            case ScoreEvent.mine:
                ResponsibleUIText mine = Instantiate<GameObject>(prefabFloatingScore).GetComponent<ResponsibleUIText>();
                mine.transform.position = Input.mousePosition;
                mine.transform.SetParent(canvasTransform, true);
                mine.Score = ScoreManager.CHAIN;
                mine.OnGetResponse = AddScore;
                if (scoreRun == null)
                {
                    scoreRun = mine;
                    scoreRun.Senior = scoreBoard;
                    scoreRun.StartMoveTo(Camera.main.WorldToScreenPoint(posScoreRun), floatingScoreSpeed);
                }
                else
                {
                    mine.Senior = scoreRun;
                    mine.StartResponse(floatingScoreSpeed);
                }
                break;

            case ScoreEvent.mineGold:
                ResponsibleUIText mineGold = Instantiate<GameObject>(prefabFloatingScore).GetComponent<ResponsibleUIText>();
                mineGold.GetComponent<Text>().color = Color.yellow;
                mineGold.transform.position = Input.mousePosition;
                mineGold.transform.SetParent(canvasTransform, true);
                mineGold.Score = 1;
                if (scoreGoldCards == null)
                {
                    scoreGoldCards = mineGold;
                    scoreGoldCards.Senior = scoreRun;
                    scoreGoldCards.OnGetResponse = MultiplyScore;
                    scoreGoldCards.StartMoveTo(Camera.main.WorldToScreenPoint(posScoreGoldCards), floatingScoreSpeed);
                }
                else
                {
                    mineGold.Senior = scoreGoldCards;
                    mineGold.OnGetResponse = AddScore;
                    mineGold.StartResponse(floatingScoreSpeed);
                }
                break;

            case ScoreEvent.draw:
            case ScoreEvent.gameWin:
            case ScoreEvent.gameLoss:
                if (scoreRun != null)
                {
                    scoreRun.StartResponse(floatingScoreSpeed);
                    scoreRun = null;
                    scoreGoldCards = null;
                }
                break;
        }
    }

    private void AddScore(ResponsibleUIText senior, ResponsibleUIText subject)
    {
        senior.Score += subject.Score;
    }

    private void MultiplyScore(ResponsibleUIText senior, ResponsibleUIText subject)
    {
        senior.Score *= (int)Mathf.Pow(2, subject.Score);
    }
}
