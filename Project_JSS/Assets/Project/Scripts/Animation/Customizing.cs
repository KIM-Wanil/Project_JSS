using System;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class Customizing : MonoBehaviour
{
    enum WhatIndex { skin, deco1, deco2 }

    [SerializeField] SpriteLibrary library;
    [SerializeField] SpriteLibraryAsset[] assets;
    [SerializeField] Image character;
    [SerializeField] CharacterData charaterData;

    int skinIndex;
    int decorationIndex1;
    int decorationIndex2;

    int tempIndex;
    WhatIndex whatIndex;


    [SerializeField] Button[] buttons;
    [SerializeField] Image[] buttonsItemImage;

    [SerializeField] Sprite[] buttonSprite;
    [SerializeField] int[] buttonsIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        assets = charaterData.assets;
        whatIndex = WhatIndex.skin;
        skinIndex = 0;
        decorationIndex1 = 0;
        decorationIndex2 = 0;
        setButtonImage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SelectPart(int index)
    {
        whatIndex = (WhatIndex)index;
        switch (whatIndex)
        {
            case WhatIndex.skin:
                tempIndex = skinIndex;
                break;
            case WhatIndex.deco1:
                tempIndex = decorationIndex1;
                break;
            case WhatIndex.deco2:
                tempIndex = decorationIndex2;
                break;
            default:
                tempIndex = skinIndex;
                break;
        }
        setButtonImage();
    }
    void setButtonImage()
    {
        Sprite[] sprites;
        int index = 0;
        switch (whatIndex)
        {
            case WhatIndex.skin:
                sprites = charaterData.skins;
                index = skinIndex;
                break;
            case WhatIndex.deco1:
                sprites = charaterData.deco1;
                index = decorationIndex1;
                break;
            case WhatIndex.deco2:
                sprites = charaterData.deco2;
                index = decorationIndex2;
                break;
            default:
                sprites = charaterData.skins;
                index = skinIndex;
                break;
        }
        if (tempIndex == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                buttonsItemImage[i].sprite = sprites[tempIndex + i];
                buttonsIndex[i] = tempIndex + i;

                if (tempIndex + i == index)
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[0];
                else
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[1];
            }
        }
        else if (tempIndex == sprites.Length-1)
        {
            for (int i = 0; i < 3; i++)
            {
                buttonsItemImage[i].sprite = sprites[tempIndex + i -2];
                buttonsIndex[i] = tempIndex + i;
                if (tempIndex + i - 2 == index)
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[0];
                else
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[1];
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                buttonsItemImage[i].sprite = sprites[tempIndex + i - 1];
                buttonsIndex[i] = tempIndex + i - 1;
                if (tempIndex + i - 1 == index)
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[0];
                else
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[1];
            }
        }
    }

    public void MoveButton(bool isLeft)
    {
        switch (whatIndex)
        {
            case WhatIndex.skin:
                    ChangeIndex(ref tempIndex, charaterData.skins.Length,isLeft);
                    break;
            case WhatIndex.deco1:
                    ChangeIndex(ref tempIndex, charaterData.deco1.Length, isLeft);
                    break;
            case WhatIndex.deco2:
                    ChangeIndex(ref tempIndex, charaterData.deco2.Length, isLeft);
                    break;
            default:
                    break;
        }
        setButtonImage();
    }
    void ChangeIndex(ref int index ,int maxValue, bool isLeft)
    {
        if (isLeft)
        {
            index--;
            if (index <0)
            {
                index = 0;
            }
        }
        else
        {
            index++;
            if (index >= maxValue)
            {
                index = maxValue - 1;
            }
        }
        
    }
    public void SetIndex(int num)
    {
        switch (whatIndex)
        {
            case WhatIndex.skin:
                skinIndex = buttonsIndex[num];
                break; 
            case WhatIndex.deco1:
                decorationIndex1 = buttonsIndex[num];
                break;  
            case WhatIndex.deco2:
                decorationIndex2 = buttonsIndex[num];
                break;
            default:
                break;
        }
        SetSkin();
    }
    public void SetSkin() {
        library.spriteLibraryAsset = assets[skinIndex * 4 + decorationIndex1* 4 + decorationIndex2];
        character.sprite = library.spriteLibraryAsset.GetSprite("아이들", "앞연금술사idle_0");
    }

}
