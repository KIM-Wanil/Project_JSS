using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAssetReferencesDataBase", menuName = "ScriptableObjects/AssetReferencesSO", order = 1)]
public class AssetReferencesSO : ScriptableObject
{
    [System.Serializable]
    public class AssetReference2
    {
        public string key;
        public UnityEngine.AddressableAssets.AssetReference assetReference;
    }

    public List<AssetReference2> guestSprites;
    public List<AssetReference2> BGMSounds;
    public List<AssetReference2> UISounds;
    public List<AssetReference2> effectSounds;

    public UnityEngine.AddressableAssets.AssetReference GetBackgroundAssetReference(string key)
    {
        return guestSprites.Find(img => img.key == key)?.assetReference;
    }
    public UnityEngine.AddressableAssets.AssetReference GetSoundAssetReference(string key, SoundType type)
    {
        switch(type)
        {
            case SoundType.BGM:
                return BGMSounds.Find(sfx => sfx.key == key)?.assetReference;
            case SoundType.UI:
                return UISounds.Find(sfx => sfx.key == key)?.assetReference;
            case SoundType.Effect:
                return effectSounds.Find(sfx => sfx.key == key)?.assetReference;
            default:
                Debug.Log("type error");
                return null;
        }
    }
    public UnityEngine.AddressableAssets.AssetReference GetBGMSoundAssetReference(string key)
    {
        return BGMSounds.Find(sfx => sfx.key == key)?.assetReference;
    }
    public UnityEngine.AddressableAssets.AssetReference GetAmbientSoundAssetReference(string key)
    {
        return UISounds.Find(sfx => sfx.key == key)?.assetReference;
    }
    public UnityEngine.AddressableAssets.AssetReference GetEffectSoundAssetReference(string key)
    {
        return effectSounds.Find(sfx => sfx.key == key)?.assetReference;
    }
}