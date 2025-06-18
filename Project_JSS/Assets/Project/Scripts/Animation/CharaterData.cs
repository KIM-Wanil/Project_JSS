using UnityEngine;
using UnityEngine.U2D.Animation;
[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string CharaterName;
    public SpriteLibraryAsset[] assets;

    public Sprite[] skins;
    public Sprite[] deco1;
    public Sprite[] deco2;
}

