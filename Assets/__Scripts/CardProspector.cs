using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card
{
    public CardState state = CardState.drawpile;
    public int layoutId;
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    public SlotDefinition slotDef;
    public bool isGold;

    public void MakeGoldCard(Deck deck, string layerName)
    {
        back.SetActive(true);
        SpriteRenderer backRenderer = back.GetComponent<SpriteRenderer>();
        backRenderer.sprite = deck.cardBackGold;
        backRenderer.sortingLayerName = layerName;
        back.SetActive(false);
        PopulateSpriteRenderers();
        spriteRenderers[0].sprite = deck.cardFrontGold;
        isGold = true;
    }

    protected override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);

        base.OnMouseUpAsButton();
    }
}
