using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsibleUIText : MovableUIText
{
    private ResponsibleUIText senior; //object that this object responses to
    private List<ResponsibleUIText> subjects = new List<ResponsibleUIText>(); //objects that response to this object

    private bool isResponsing;
    private float speed;

    private float score;

    public delegate void OnGetResponseDelegate(ResponsibleUIText senior, ResponsibleUIText subject);
    public OnGetResponseDelegate OnGetResponse;

    public ResponsibleUIText Senior
    {
        get { return senior; }
        set
        {
            senior = value;
            value.subjects.Add(this); 
        }
    }

    public float Score
    {
        get { return score; }
        set
        {
            score = value;
            text.text = value.ToString("N0");
        }
    }

    private void Start()
    {
        score = float.Parse(text.text);
    }

    protected override void Update()
    {
        base.Update();

        if(senior != null)
        {
            if (isResponsing)
            {
                if (transform.position == senior.transform.position)
                {
                    GetResponse(senior);
                    Destroy(this.gameObject);
                }

                if (!isMoving)
                    if (subjects.Count == 0)
                        StartMoveTo(senior.transform.position, speed);
            }
        }
    }

    public void StartResponse(float speed)
    {
        isResponsing = true;
        this.speed = speed;

        if (subjects.Count != 0)
        {
            foreach (ResponsibleUIText subject in subjects)
            {
                if (!subject.isResponsing)
                {
                    subject.StartResponse(speed);
                }
            }   
        }    
        else
        {
            StartMoveTo(senior.transform.position, speed);
        }
    }

    public void GetResponse(ResponsibleUIText senior)
    {
        OnGetResponse(senior, this);
        senior.subjects.Remove(this);
    }
}
