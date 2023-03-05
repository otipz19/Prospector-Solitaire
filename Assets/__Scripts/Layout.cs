using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Layout : MonoBehaviour
{
    private PT_XMLReader xmlr;
    private PT_XMLHashtable xml;

    public Vector2 multiplier;

    public List<SlotDefinition> slotDefs = new List<SlotDefinition>(); //Все экземпляры SlotDefinition для слоев 0-3
    public SlotDefinition drawPile; //Слот свободных карт
    public SlotDefinition discardPile; //Слот сброса

    static public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Draw", "Discard" };

    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("x"));

        SlotDefinition tmpSlotDef;
        PT_XMLHashList xmlSlots = xml["slot"];

        for(int i = 0; i < xmlSlots.Count; i++)
        {
            tmpSlotDef = new SlotDefinition(xmlSlots, i);

            switch (tmpSlotDef.type)
            {
                case "slot":
                    slotDefs.Add(tmpSlotDef);
                    break;
                case "drawpile":
                    drawPile = tmpSlotDef;
                    break;
                case "discardpile":
                    discardPile = tmpSlotDef;
                    break;
            }
        }
    }
}

[System.Serializable]
public class SlotDefinition
{
    public float x;
    public float y;
    public bool faceUp;
    public string layerName = "Debug";
    public int layerId = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type;
    public Vector2 stagger;

    public SlotDefinition(PT_XMLHashList list, int i)
    {
        if (list[i].HasAtt("type"))
            type = list[i].att("type");
        else
            type = "slot";

        x = float.Parse(list[i].att("x"));
        y = float.Parse(list[i].att("y"));

        layerId = int.Parse(list[i].att("layer"));
        layerName = Layout.sortingLayerNames[layerId];
        faceUp = list[i].att("faceup") == "1";

        switch (type)
        {
            case "slot":
                id = int.Parse(list[i].att("id"));
                if (list[i].HasAtt("hiddenby"))
                {
                    string[] hiddendBy = list[i].att("hiddenby").Split(",");
                    foreach (string str in hiddendBy)
                        hiddenBy.Add(int.Parse(str));
                }
                break;

            case "drawpile":
                stagger.x = float.Parse(list[i].att("xstagger"));
                break;
        }
    }
}