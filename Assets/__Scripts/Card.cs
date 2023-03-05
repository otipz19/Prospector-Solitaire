using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Card : MonoBehaviour
{
    public string suit; //Масть
    public int rank; //Достоинство
    public Color color = Color.black; //Цвет значков
    public string colS = "Black";

    public List<GameObject> decoratorGOs = new List<GameObject>();
    public List<GameObject> pipGOs = new List<GameObject>();
    public GameObject back; //Рубашка карты

    public CardDefinition def;

    [SerializeField]protected SpriteRenderer[] spriteRenderers;

    private bool isMoving;
    private Vector3 destinationMove;
    private float timeStartMove;
    private float timeDurationMove;

    private bool isRotating;
    private bool toFaceUpRotate;
    private Quaternion startRotate;
    private Quaternion destinationRotate;
    private float timeStartRotate;
    private float timeDurationRotate;

    public bool faceUp
    {
        get { return !back.activeSelf; }
        set
        {
            back.SetActive(!value);
            //backRenderer.sortingOrder = spriteRenderers[0].sortingOrder;
        }
    }

    public void StartMove(Vector3 destination, float timeDuration = 1.5f)
    {
        if (!isMoving)
        {
            isMoving = true;
            destinationMove = destination;
            timeStartMove = Time.time;
            timeDurationMove = timeDuration;
        }
    }

    public void StartRotate(float timeDuration = 0.75f, bool toFaceUp = true)
    {
        if (!isRotating)
        {
            toFaceUpRotate = toFaceUp;
            isRotating = true;
            timeStartRotate = Time.time;
            timeDurationRotate = timeDuration;

            startRotate = transform.rotation;
            destinationRotate = Quaternion.Euler(startRotate.eulerAngles + new Vector3(0, 180, 0));

            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                if (renderer.gameObject.name != "back")
                    renderer.flipX = true;
                if (renderer.gameObject.name == "letter" || renderer.gameObject.name == "suit")
                {
                    Vector3 pos = renderer.transform.localPosition;
                    pos.x *= -1;
                    renderer.transform.localPosition = pos;
                }
            }
        }
    }

    private void Update()
    {
        if (isMoving)
        {
            float u = (Time.time - timeStartMove) / timeDurationMove;
            if (u > 1)
            {
                u = 1;
                isMoving = false;
            }   
            transform.position = Vector3.Lerp(transform.position, destinationMove, u);
        }

        if (isRotating)
        {
            float u = Easing.Ease((Time.time - timeStartRotate) / timeDurationRotate, Easing.SinIn);
            //float u = (Time.time - timeStartRotate) / timeDurationRotate;
            if (u > 0.5)
            {
                if (toFaceUpRotate)
                    faceUp = true;
                else
                    faceUp = false;
            }
            if (u > 1)
            {
                u = 1;
                isRotating = false;
            }

            transform.rotation = Quaternion.Lerp(startRotate, destinationRotate, u);
        }
    }

    private void Start()
    {
        SetSortingOrder(0);
    }

    protected void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetSortingLayerName(string name)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer renderer in spriteRenderers)
            renderer.sortingLayerName = name;

        if (!back.activeSelf)
        {
            back.SetActive(true);
            back.GetComponent<SpriteRenderer>().sortingLayerName = name;
            back.SetActive(false);
        }
    }

    public void SetSortingOrder(int sortingOrder)
    {
        PopulateSpriteRenderers();

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer.gameObject == this.gameObject)
                renderer.sortingOrder = sortingOrder;
            else
                switch (renderer.gameObject.name)
                {
                    case "back":
                        renderer.sortingOrder = sortingOrder + 2;
                        break;
                    case "face":
                    default:
                        renderer.sortingOrder = sortingOrder + 1;
                        break;
                }
        }

        if (!back.activeSelf)
        {
            back.SetActive(true);
            back.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 2;
            back.SetActive(false);
        }
    }

    virtual protected void OnMouseUpAsButton()
    {
        //Debug.Log(gameObject.name);
    }
}

[System.Serializable]
public class Decorator //Хранит информацию о каждом значке на карте
{
    public string type;
    public Vector3 location = Vector3.zero;
    public bool flip = false; //Признак отражения спрайта по вертикали
    public float scale = 1f;

    public Decorator(PT_XMLHashList list, int i)
    {
        if (list[i].HasAtt("type"))
            type = list[i].att("type");
        else
            type = "pip";
        flip = list[i].att("flip") == "1";
        location.x = float.Parse(list[i].att("x"));
        location.y = float.Parse(list[i].att("y"));
        location.z = float.Parse(list[i].att("z"));
        if (list[i].HasAtt("scale"))
            scale = float.Parse(list[i].att("scale"));
    }
}

[System.Serializable]
public class CardDefinition
{
    public string face; //Название спрайта, изображающего лицевую сторону карты
    public int rank; //Достоинство карты
    public List<Decorator> pips = new List<Decorator>();
}