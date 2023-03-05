using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool startFaceUp = false;

    //������� ������
    public Sprite suitClub;
    public Sprite suitHeart;
    public Sprite suitDiamond;
    public Sprite suitSpade;

    public Sprite[] faceSprites; //������� ������, ���� � ������
    public Sprite[] rankSprites; //������� �����, ������������ ����������� �����

    //������� ��������� ����
    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    public GameObject prefabCard;
    public GameObject prefabSprite;

    [HideInInspector]
    public List<Card> cards;

    private PT_XMLReader xmlr;

    private List<string> cardNames;
    [SerializeField]
    private List<Decorator> decorators;
    [SerializeField]
    private List<CardDefinition> cardDefs;
    private Transform deckAnchor;
    private Dictionary<string, Sprite> dictSuits;

    static public void Shuffle(ref List<Card> cards)
    {
        List<Card> tmpList = new List<Card>();

        while(cards.Count > 0)
        {
            int i = Random.Range(0, cards.Count);
            tmpList.Add(cards[i]);
            cards.RemoveAt(i);
        }

        cards = tmpList;
    }

    public void InitDeck(string deckXMLtext)
    {
        //������� ������������ ������� ������ ��� ���� ����
        if (GameObject.Find("_DECK") == null) 
        {
            GameObject go = new GameObject("_DECK");
            Vector3 pos = new Vector3(-Camera.main.orthographicSize * Camera.main.aspect + 2f, -Camera.main.orthographicSize + 2.5f, 0);
            go.transform.position = pos;
            deckAnchor = go.transform;
        }

        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C", suitClub},
            {"H", suitHeart},
            {"D", suitDiamond},
            {"S", suitSpade}
        };

        ReadDeck(deckXMLtext);

        MakeCards();
    }

    private void ReadDeck(string deckXMLtext)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLtext);

        decorators = new List<Decorator>();

        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        for(int i = 0; i < xDecos.Count; i++)
        {
            Decorator deco = new Decorator(xDecos, i);
            decorators.Add(deco);
        }

        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for(int i = 0; i < xCardDefs.Count; i++)
        {
            CardDefinition cDef = new CardDefinition();

            cDef.rank = int.Parse(xCardDefs[i].att("rank"));

            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if(xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    Decorator deco = new Decorator(xPips, j);
                    cDef.pips.Add(deco);
                }
            }

            if (xCardDefs[i].HasAtt("face"))
                cDef.face = xCardDefs[i].att("face");

            cardDefs.Add(cDef);
        }
    }

    //������� ������� ������� ����
    private void MakeCards()
    {
        //cardNames �������� �������� ����
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };

        foreach (string letter in letters)
            for (int i = 0; i < 13; i++)
                cardNames.Add(letter + (i + 1).ToString());

        cards = new List<Card>();
        for(int i = 0; i < cardNames.Count; i++)
            cards.Add(MakeCard(i));
    }

    private Card MakeCard(int cNum)
    {
        GameObject cardGO = Instantiate<GameObject>(prefabCard);
        cardGO.transform.parent = deckAnchor;
        Card card = cardGO.GetComponent<Card>();

        //��� ������ ���������� ����� � ���������� ���
        //� ��� �������, ����� �������
        cardGO.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        //���������������� ���� ���������� Card 
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if(card.suit == "H" || card.suit == "D")
        {
            card.color = Color.red;
            card.colS = "Red";
        }
        card.def = GetCardDefinitionByRank(card.rank);

        foreach (Decorator deco in decorators) //�������� ������ ����������
            AddDeco(card, deco);
        foreach (Decorator pip in card.def.pips) //�������� ������ ����������� �����
            AddDeco(card, pip);

        AddFace(card);
        AddBack(card);

        return card;
    }

    private CardDefinition GetCardDefinitionByRank(int rank)
    {
        foreach(CardDefinition def in cardDefs)
            if (def.rank == rank)
                return def;
        return null;
    }

    private GameObject tmpGO;
    private SpriteRenderer tmpSpriteRend;

    private void AddDeco(Card card, Decorator deco)
    {
        tmpGO = Instantiate<GameObject>(prefabSprite); //������� ������� ������ ������ �� �����

        tmpSpriteRend = tmpGO.GetComponent<SpriteRenderer>();

        if (deco.type != "letter")
        {
            tmpSpriteRend.sprite = dictSuits[card.suit];
        }
        else
        {
            tmpSpriteRend.sprite = rankSprites[card.rank];
            tmpSpriteRend.color = card.color;
        }
        tmpSpriteRend.sortingOrder = 1; //��������� ������ ������ ��� �������� �����

        tmpGO.transform.parent = card.transform;
        tmpGO.transform.localPosition = deco.location;
        if (deco.flip)
            tmpGO.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
        if (deco.scale != 1)
            tmpGO.transform.localScale = Vector3.one * deco.scale;
        tmpGO.name = deco.type;

        if (deco.type == "pip")
            card.pipGOs.Add(tmpGO);
        else
            card.decoratorGOs.Add(tmpGO);
    }

    private void AddFace(Card card)
    {
        if (card.def.face == null)
            return;

        tmpGO = Instantiate<GameObject>(prefabSprite);
        tmpGO.transform.parent = card.transform;
        tmpGO.transform.localPosition = Vector3.zero;
        tmpGO.name = "face";

        tmpSpriteRend = tmpGO.GetComponent<SpriteRenderer>();
        tmpSpriteRend.sprite = GetFace(card.def.face + card.suit);
        tmpSpriteRend.sortingOrder = 1;
    }

    private Sprite GetFace(string name)
    {
        foreach (Sprite sprite in faceSprites)
            if (sprite.name == name)
                return sprite;
        return null;
    }

    private void AddBack(Card card)
    {
        tmpGO = Instantiate<GameObject>(prefabSprite);
        tmpGO.transform.parent = card.transform;
        tmpGO.transform.localPosition = Vector3.zero;
        tmpGO.name = "back";

        tmpSpriteRend = tmpGO.GetComponent<SpriteRenderer>();
        tmpSpriteRend.sprite = cardBack;
        tmpSpriteRend.sortingOrder = 2;

        card.back = tmpGO;
        card.faceUp = startFaceUp;
    }
}
