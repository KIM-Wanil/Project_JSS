using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizingButton : MonoBehaviour
{
    public CharacterData characterData;
    [SerializeField] FloorData floorData;
    [SerializeField] Image characterImage;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] TextMeshProUGUI characterDescription;
    [SerializeField] GameObject lockObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterData.skinIndex = 0;
        characterData.decorationIndex1 = 0;
        characterData.decorationIndex2 = 0;
        Setting();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setting()
    {
        characterImage.sprite = characterData.assets[characterData.skinIndex * 4 + characterData.decorationIndex1 * 4 + characterData.decorationIndex2].GetSprite("idle", "idle_0");
        characterName.text = characterData.CharaterName;
        characterDescription.text = characterData.CharacterDescription;

        if (floorData.isUnlock && characterData.isUnlock)
        {
            lockObject.SetActive(false);
          
        }
        else
        {
            lockObject.SetActive(true);
        }
    }
    public void Unlock()
    {
        lockObject.SetActive(false);
    }
}
