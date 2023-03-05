using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private ResponsibleUIText text1, text3, text7;

    private void Start()
    {
        text1 = GameObject.Find("1").GetComponent<ResponsibleUIText>();
        text3 = GameObject.Find("3").GetComponent<ResponsibleUIText>();
        text7 = GameObject.Find("7").GetComponent<ResponsibleUIText>();

        text1.Senior = text3;
        text3.Senior = text7;
        text3.OnGetResponse = AddScore;
        text7.OnGetResponse = AddScore;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            text3.StartResponse(3f);
        }
    }

    public void AddScore(ResponsibleUIText senior, ResponsibleUIText subject)
    {
        senior.Score += subject.Score;
    }
}
