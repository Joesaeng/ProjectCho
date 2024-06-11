using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SpellIcon : UI_Base
{
    private Image _spellImage;
    private Image _spellEdge;
    public override void Init()
    {
        _spellImage = GetComponent<Image>();
        _spellEdge = Util.FindChild<Image>(gameObject, "Image_SpellEdge");
    }

    public void SetImages(Sprite spellImage, Sprite edgeImage)
    {
        _spellImage.sprite = spellImage;
        _spellEdge.sprite = edgeImage;
    }

    public void SetEdge(Sprite edgeImage)
    {
        _spellEdge.sprite = edgeImage;
    }
}
