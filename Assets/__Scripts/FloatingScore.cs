using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FSstate
{
    idle,
    pre,
    active,
    post,
}

public class FloatingScore : MonoBehaviour
{
    [SerializeField]private FSstate state = FSstate.idle;

    private int score = 0;

    private List<Vector2> pointsBezier;
    public List<float> fontSizes;

    private float timeStart = -1f;
    private float timeDuration = 1f;
    private string easingCurve = Easing.InOut;

    public GameObject reportFinishTo;

    private RectTransform rectTransform;
    private Text text;

    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            GetComponent<Text>().text = value.ToString("N0");
        }
    }

    private void Update()
    {
        if (state == FSstate.idle)
            return;

        float u = (Time.time - timeStart) / timeDuration;
        float uC = Easing.Ease(u, easingCurve);

        if(u < 0)
        {
            state = FSstate.pre;
            text.enabled = false;
        }
        else if(u >= 1)
        {
            uC = 1;
            state = FSstate.post;
            if(reportFinishTo != null)
            {
                reportFinishTo.SendMessage("FSCallBack", this);
                Destroy(this.gameObject);
            }
            else
            {
                state = FSstate.idle;
            }
        }
        else
        {
            state = FSstate.active;
            text.enabled = true;
        }

        rectTransform.anchorMax = rectTransform.anchorMin = Utils.Bezier(uC, pointsBezier);
        if(fontSizes != null && fontSizes.Count > 0)
            GetComponent<Text>().fontSize = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
    }

    public void Init(List<Vector2> points, float timeStart = 0, float timeDuration = 1)
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;

        text = GetComponent<Text>();
        pointsBezier = new List<Vector2>(points);

        if(points.Count == 1)
        {
            transform.position = points[0];
            return;
        }

        if (timeStart == 0)
            timeStart = Time.time;
        this.timeStart = timeStart;
        this.timeDuration = timeDuration;

        state = FSstate.pre;
    }

    public void FSCallBack(FloatingScore fs)
    {
        Score += fs.score;
    }
}
