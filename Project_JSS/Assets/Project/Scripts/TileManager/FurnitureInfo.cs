 using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class Sprites
{
    public Sprite[] sprites;
}
public class FurnitureInfo : MonoBehaviour
{
    FurniturePlacementManager placementManager;
    SpriteRenderer spriteRenderer;

    [SerializeField] string furnitureName;
    [SerializeField] bool isFloor;

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
    public Vector2Int Size { get { return size[rotation]; } }
    public Vector2Int GridPosition { get { return gridPosition; } set => gridPosition = value; }
    public Vector2Int TartgetPosition { get { return tartgetPosition[rotation]; ; } }
    public int Rotation { get { return rotation; } }
    public void SettingData(FurnitureData data)
    {

       this.furnitureName= data.furnitureName; // 가구 이름
        gridPosition = data.gridPosition; // 가구 위치
        size = data.size;
        tartgetPosition = data.tartgetPosition;
        rotation = data.rotation;
        sprites = data.furnitureSprite;
        spriteNumber = data.spriteNumber;
        isUnlocked = data.isUnlocked;

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
            placementManager.Placement(this.gameObject);
            SpawnFurniture();
        }
    }
    public void SetSpriterenderColor()
    {
        Color startColor = spriteRenderer.color;
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
    public void SettingSprites(int spriteNumber)
    {
        this.spriteNumber = spriteNumber;
        spriteRenderer.sprite = sprites[rotation].sprites[spriteNumber];
    }
    public void SettingRotate(int rotationNumber)
    {
        rotation = rotationNumber;
        spriteRenderer.sprite = sprites[spriteNumber].sprites[rotation / 2];
    }
    public void RotateSprites()
    {
        rotation++;
        if (rotation >= 4)
        {
            rotation = 0;
        }
        if (rotation == 1 || rotation == 3)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
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

        // 초기 위치를 위로 올림
        Vector3 startPosition = originalPosition + Vector3.up * fallDistance;
        transform.position = startPosition;

        // 투명도 초기화
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        }

        // 위치 애니메이션
        transform.DOMove(originalPosition, fallDuration)
            .SetEase(fallEase);

        // 페이드 인 애니메이션
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(1f, fadeDuration)
                .SetEase(Ease.Linear);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
