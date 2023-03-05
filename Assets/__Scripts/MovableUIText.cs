using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovableUIText : MonoBehaviour
{
    protected Text text;

    protected bool isMoving;

    protected Vector2 startPos;
    protected Vector2 destinationPos;
    protected float timeStart;
    protected float timeDuration;

    public void StartMoveTo(Vector2 destination, float duration)
    {
        isMoving = true;
        startPos = transform.position;
        destinationPos = destination;
        timeStart = Time.time;
        timeDuration = duration;
    }

    protected void Awake()
    {
        text = GetComponent<Text>();
    }

    protected virtual void Update()
    {
        if (isMoving)
        {
            float u = (Time.time - timeStart) / timeDuration;
            u = Easing.Ease(u, Easing.In);
            if (u > 1)
            {
                u = 1;
                isMoving = false;
            }
            transform.position = Vector2.Lerp(startPos, destinationPos, u);
        }
    }
}
