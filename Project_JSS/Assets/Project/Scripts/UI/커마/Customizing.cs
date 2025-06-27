using System;
using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class Customizing : MonoBehaviour
{
    enum WhatIndex { skin, deco1, deco2 }

    [SerializeField] FurniturePlacementManager furniturePlacementManager;
    [SerializeField] SpriteLibrary[] library;
    [SerializeField] CharacterData[] charaterData;
    [SerializeField] SpriteLibraryAsset[] assets;
    [SerializeField] Image character;
    [SerializeField] CustomizingButton[] customizingButtons;

    int floorIndex;

    int tempIndex;
    WhatIndex whatIndex;

    [SerializeField] GameObject ui;

    [SerializeField] Button[] buttons;
    [SerializeField] Image[] buttonsItemImage;
    [SerializeField] TextMeshProUGUI[] buttonsText;

    [SerializeField] Sprite[] buttonSprite;
    [SerializeField] int[] buttonsIndex;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
        whatIndex = WhatIndex.skin;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetUI( int index)
    {
        floorIndex = index;
        assets = charaterData[floorIndex].assets;
        whatIndex = WhatIndex.skin;
        setButtonImage();
        SetSkin();
        ui.SetActive(true);
    }
    public void SetUI_false()
    {    
        ui.SetActive(false);
    }
    public void SelectPart(int index)
    {
        whatIndex = (WhatIndex)index;
        switch (whatIndex)
        {
            case WhatIndex.skin:
                tempIndex = charaterData[floorIndex].skinIndex;
                break;
            case WhatIndex.deco1:
                tempIndex = charaterData[floorIndex].decorationIndex1;
                break;
            case WhatIndex.deco2:
                tempIndex = charaterData[floorIndex].decorationIndex2;
                break;
            default:
                tempIndex = charaterData[floorIndex].skinIndex;
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
                sprites = charaterData[floorIndex].skins;
                index = charaterData[floorIndex].skinIndex;
                break;
            case WhatIndex.deco1:
                sprites = charaterData[floorIndex].deco1;
                index = charaterData[floorIndex].decorationIndex1;
                break;
            case WhatIndex.deco2:
                sprites = charaterData[floorIndex].deco2;
                index = charaterData[floorIndex].decorationIndex2;
                break;
            default:
                sprites = charaterData[floorIndex].skins;
                index = charaterData[floorIndex].skinIndex;
                break;
        }
        if (tempIndex == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                
                buttonsItemImage[i].sprite = sprites[tempIndex + i];
                buttonsText[i].text = sprites[tempIndex + i].name;
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
                buttonsText[i].text = sprites[tempIndex + i - 2].name;
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
                buttonsText[i].text = sprites[tempIndex + i - 1].name;
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
                    ChangeIndex(ref tempIndex, charaterData[floorIndex].skins.Length,isLeft);
                    break;
            case WhatIndex.deco1:
                    ChangeIndex(ref tempIndex, charaterData[floorIndex].deco1.Length, isLeft);
                    break;
            case WhatIndex.deco2:
                    ChangeIndex(ref tempIndex, charaterData[floorIndex].deco2.Length, isLeft);
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
        if (buttonsItemImage[num].sprite.name == "LOCK ICON")
        {
            return;
        }
        switch (whatIndex)
        {
            case WhatIndex.skin:
                charaterData[floorIndex].skinIndex = buttonsIndex[num];
                break; 
            case WhatIndex.deco1:
                charaterData[floorIndex].decorationIndex1 = buttonsIndex[num];
                break;  
            case WhatIndex.deco2:
                charaterData[floorIndex].decorationIndex2 = buttonsIndex[num];
                break;
            default:
                break;
        }
        setButtonImage();
        SetSkin();
    }
    public void SetSkin() {
        library[floorIndex].spriteLibraryAsset = assets[charaterData[floorIndex].skinIndex * 4 + charaterData[floorIndex].decorationIndex1 * 4 + charaterData[floorIndex].decorationIndex2];
        character.sprite = library[floorIndex].spriteLibraryAsset.GetSprite("idle", "idle_0");
        customizingButtons[floorIndex].Setting();
    }

}
