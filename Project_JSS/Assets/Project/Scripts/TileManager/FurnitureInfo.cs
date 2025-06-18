 using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class Sprites
{
    public bool isUnlocked;
    public Sprite[] sprites;    
    public Sprite lockSprite;    

}
public class FurnitureInfo : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    [SerializeField] FurnitureData data;
    [SerializeField] string furnitureName;
    [SerializeField] bool isFloor;
    [SerializeField] int floorIndex;

    [SerializeField] Vector2Int gridPosition;
    [SerializeField] Vector2Int[] size;

    [SerializeField] Vector2Int[] tartgetPosition;
    [SerializeField] int rotation;
    [SerializeField] int spriteNumber;
    bool flip;
    [SerializeField] Sprites[] sprites;
    bool isUnlocked;

    [Header("Spawn Settings")]
    [SerializeField] private float fallDistance = 3f; // 월드 유닛 기준 떨어지는 거리
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private Ease fallEase = Ease.OutBounce;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Vector3 originalPosition;

    public bool IsFloor { get { return isFloor; } }
    public int FloorIndex { get { return floorIndex; } }
    public Vector2Int Size { get { return size[rotation]; } }
    public Vector2Int GridPosition { get { return gridPosition; } set => gridPosition = value; }
    public Vector2Int TartgetPosition { get { return tartgetPosition[rotation]; ; } }
    public int Rotation { get { return rotation; } }
    public Sprites[] MySprites { get { return sprites; } }
    public int SpriteNumber { get { return spriteNumber; }  }
    public void SettingData(FurnitureData data)
    {
        this.name = data.furnitureName;
        this.data = data;
        this.furnitureName= data.furnitureName; // 가구 이름
        
        size = data.size;
        tartgetPosition = data.tartgetPosition;
        sprites = data.furnitureSprite;

        gridPosition = data.gridPosition; // 가구 위치
        rotation = data.rotation;
        spriteNumber = data.spriteNumber;
        isUnlocked = data.isUnlocked;

        isFloor  = data.isFloor;

        this.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset;
        this.GetComponent<IsoSpriteSorting>().SorterPositionOffset2 = data.SorterPositionOffset2;

        this.GetComponent<BoxCollider2D>().offset = data.colliderOffset;
        this.GetComponent<BoxCollider2D>().size = data.colliderSize;
        if (data.isUnlocked)
        {
            if (rotation == 1 || rotation == 3)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
            spriteRenderer.sprite = sprites[spriteNumber].sprites[rotation / 2];
           // SpawnFurniture();
        }
    }
    public FurnitureData saveInfo()
    {
        FurnitureData data = new FurnitureData();
        data.furnitureName = this.furnitureName;

        data.size = size;
        data.tartgetPosition = tartgetPosition;
        data.furnitureSprite = sprites;

        data.gridPosition = gridPosition; // 가구 위치
        data.rotation = rotation;
        data.spriteNumber = spriteNumber;
        data.isUnlocked = isUnlocked;

        data.isFloor = isFloor;
        data.SorterPositionOffset = this.GetComponent<IsoSpriteSorting>().SorterPositionOffset;
        data.SorterPositionOffset2 = this.GetComponent<IsoSpriteSorting>().SorterPositionOffset2;

        data.colliderOffset = this.GetComponent<BoxCollider2D>().offset;
        data.colliderSize = this.GetComponent<BoxCollider2D>().size;
        return data;
    }





    public void SetSpriterenderColor()
    {
        Color startColor = spriteRenderer.color;
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
    public void SettingSprites(int spriteNumber)
    {
        if (!sprites[spriteNumber].isUnlocked)
        {
            return;
        }
        this.spriteNumber = spriteNumber;
        spriteRenderer.sprite = sprites[spriteNumber].sprites[rotation / 2];
    }
    public void SettingRotate(int rotationNumber)
    {
        rotation = rotationNumber;
        spriteRenderer.sprite = sprites[spriteNumber].sprites[rotation / 2];
    }
    public void RotateSprites()
    {
        if (!isFloor)
            return;
        rotation++;
        if (rotation >= 4)
        {
            rotation = 0;
        }
        if (rotation == 1 || rotation == 3)
        {
            this.gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            this.gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
        spriteRenderer.sprite = sprites[spriteNumber].sprites[rotation / 2];
    }
    public Vector2Int GetTargetPosition(int num)
    {
        return new Vector2Int(gridPosition.x + tartgetPosition[num].x, gridPosition.y + tartgetPosition[num].y);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }
    public void SpawnFurniture()
    {
        // 현재 위치 저장
        originalPosition = transform.position;
        // 투명도 초기화
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        }
        // 페이드 인 애니메이션
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(1f, fadeDuration)
                .SetEase(Ease.Linear);
        }
        if (isFloor)
        {

            // 초기 위치를 위로 올림
            Vector3 startPosition = originalPosition + Vector3.up * fallDistance;
            transform.position = startPosition;



            // 위치 애니메이션
            transform.DOMove(originalPosition, fallDuration)
                .SetEase(fallEase);
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
