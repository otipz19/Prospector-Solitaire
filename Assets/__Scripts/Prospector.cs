using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public float goldCardChance = 0.2f;
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3f;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter = Vector3.zero;
    public Text gameOverText, roundResultText, highScoreText;

    private Deck deck;
    private Layout layout;
    private Transform layoutAnchor;
    private List<CardProspector> drawPile;
    private int drawPileMax;
    private List<CardProspector> discardPile = new List<CardProspector>();
    private List<CardProspector> tableau = new List<CardProspector>();
    private CardProspector target;

    private void Awake()
    {
        if (S != null)
            Debug.Log("Prospector.S is already set!");
        else
            S = this;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);

        Deck.Shuffle(ref deck.cards);
        Debug.Log("Amount of cards: " + deck.cards.Count);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardToListCardProspector(deck.cards);
        LayoutGame();

        ScoreBoard.S.Score = ScoreManager.SCORE;
        highScoreText.text = "High score: " + Utils.AddCommasToNumber(ScoreManager.HIGH_SCORE);

        //ScoreBoard.S.FloatingScoreHandler(ScoreEvent.mine);
    }

    private List<CardProspector> ConvertListCardToListCardProspector(List<Card> cards)
    {
        List<CardProspector> cardsProspector = new List<CardProspector>();
        foreach (Card card in cards)
            cardsProspector.Add(card as CardProspector);
        return cardsProspector;
    }

    private CardProspector Draw()
    {
        CardProspector card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }

    private void LayoutGame()
    {
        if(layoutAnchor == null)
        {
            GameObject tmpGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tmpGO.transform;
            layoutAnchor.position = layoutCenter;
        }

        CardProspector card;
        foreach(SlotDefinition slotDefinition in layout.slotDefs)
        {
            card = Draw();

            if (Random.value <= goldCardChance)
            {
                card.MakeGoldCard(deck, slotDefinition.layerName);
            }

            card.faceUp = slotDefinition.faceUp;
            card.transform.parent = layoutAnchor;
            card.transform.position = new Vector3(layout.multiplier.x * slotDefinition.x,
                                                  layout.multiplier.y * slotDefinition.y,
                                                  -slotDefinition.layerId);

            card.layoutId = slotDefinition.id;
            card.slotDef = slotDefinition;
            card.state = CardState.tableau;
            card.SetSortingLayerName(slotDefinition.layerName);
            tableau.Add(card);
        }

        foreach (CardProspector tmpCard in tableau)
        {
            foreach (int id in tmpCard.slotDef.hiddenBy)
            {
                card = FindCardByLayoutId(id);
                tmpCard.hiddenBy.Add(card);            
            }
        }
           
        MoveToTarget(Draw());
        drawPileMax = drawPile.Count;
        UpdateDrawPile();
    }

    private CardProspector FindCardByLayoutId(int id)
    {
        foreach (CardProspector card in tableau)
            if (card.layoutId == id)
                return card;
        return null;
    }

    public void CardClicked(CardProspector card)
    {
        Vector3 targetPos = target.transform.position;

        switch (card.state)
        {
            case CardState.drawpile:
                card.StartRotate(0.5f);
                MoveToTarget(card);
                UpdateDrawPile();
                ScoreManager.EVENT(ScoreEvent.draw);
                ScoreBoard.S.FloatingScoreHandler(ScoreEvent.draw);
                break;

            case CardState.tableau:
                if (!card.faceUp)
                    break;
                if (!AdjacentRank(card, target))
                    break;
                MoveToTarget(card);
                SetTableauFaceUp();
                if (card.isGold)
                {
                    ScoreManager.EVENT(ScoreEvent.mineGold);
                    ScoreBoard.S.FloatingScoreHandler(ScoreEvent.mine);
                    ScoreBoard.S.FloatingScoreHandler(ScoreEvent.mineGold);
                }
                else
                {
                    ScoreManager.EVENT(ScoreEvent.mine);
                    ScoreBoard.S.FloatingScoreHandler(ScoreEvent.mine);
                } 
                break;
        }

        CheckForGameOver();
    }

    private bool AdjacentRank(Card c0, Card c1)
    {
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
            return true;
        if ((c0.rank == 1 && c1.rank == 13) || (c1.rank == 1 && c0.rank == 13)) // ороль и туз считаютс€ "соседними" по достоинству картами
            return true;
        return false;
    }

    private void SetTableauFaceUp()
    {
        foreach(CardProspector card in tableau)
        {
            bool faceUp = true;
            foreach (CardProspector cover in card.hiddenBy)
                if (cover.state == CardState.tableau)
                    faceUp = false;
            if (faceUp)
                if(!card.faceUp)
                    card.StartRotate();
        }  
    }

    private void MoveToDiscard(CardProspector card)
    {
        card.StartRotate(0.25f, false);
        card.state = CardState.discard;
        discardPile.Add(card);
        card.transform.parent = layoutAnchor;

        card.transform.position = new Vector3(layout.multiplier.x * layout.discardPile.x,
                                              layout.multiplier.y * layout.discardPile.y,
                                              -layout.discardPile.layerId + 0.5f);

        
        card.SetSortingOrder(-200 + discardPile.Count * 4);
    }

    private void MoveToTarget(CardProspector card)
    {
        if (target != null)
        {
            MoveToDiscard(target);
            card.StartMove(target.transform.position);
        }
        else
        {
            card.faceUp = true;
            card.transform.position = new Vector3(layout.multiplier.x * layout.discardPile.x,
                                              layout.multiplier.y * layout.discardPile.y,
                                              -layout.discardPile.layerId);
        }

        drawPile.Remove(card);
        tableau.Remove(card);
        target = card;
        card.state = CardState.target;
        card.transform.parent = layoutAnchor;
        card.SetSortingLayerName(layout.discardPile.layerName);
        card.SetSortingOrder(0);
    }

    private void UpdateDrawPile()
    {
        CardProspector card;
        for(int i = 0; i < drawPile.Count; i++)
        {
            card = drawPile[i];
            card.state = CardState.drawpile;
            card.transform.parent = layoutAnchor;

            card.transform.position = new Vector3(layout.drawPile.x + (i + drawPileMax - drawPile.Count) * layout.drawPile.stagger.x,
                                                  layout.drawPile.y + i * layout.drawPile.stagger.y,
                                                  -layout.drawPile.layerId + 0.1f * i); 

            card.faceUp = false;
            card.SetSortingLayerName(layout.drawPile.layerName);
            card.SetSortingOrder(-1 * i);
        }
    }

    private void CheckForGameOver()
    {
        if (tableau.Count == 0) //≈сли карты в раскладке закончились - победа
        {

            StartCoroutine(GameOver(true));
            return;
        }

        if (drawPile.Count > 0) //≈сли еще есть карты в стопке свободных карт, продолжаем игру
            return;

        foreach (CardProspector card in tableau) //≈сли еще есть возможные ходы, продолжаем игру
            if (card.faceUp)
                if (AdjacentRank(card, target))
                    return;

        StartCoroutine(GameOver(false)); //«аканчиваем игру поражением
    }

    private IEnumerator GameOver(bool win)
    {
        float score = ScoreManager.SCORE;
        float highScore = ScoreManager.HIGH_SCORE;

        if (win)
        {
            ScoreManager.EVENT(ScoreEvent.gameWin);
            ScoreBoard.S.FloatingScoreHandler(ScoreEvent.gameWin);
        }
        else
        {
            ScoreManager.EVENT(ScoreEvent.gameLoss);
            ScoreBoard.S.FloatingScoreHandler(ScoreEvent.gameLoss);
        }

        ShowResultsUI(win, score, highScore);

        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowResultsUI(bool win, float score, float highScore)
    {
        gameOverText.gameObject.SetActive(true);
        roundResultText.gameObject.SetActive(true);

        if (win)
        {
            gameOverText.text = "Round over";
            roundResultText.text = "You won this round!\nRound score: " + score;
        }
        else
        {
            gameOverText.text = "Game over";
            if (score > highScore)
                roundResultText.text = "You got the high score!\nHigh score: " + score;
            else
                roundResultText.text = "Your total score was: " + score;
        }
    }
}