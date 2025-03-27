 using UnityEngine;

[System.Serializable]
public class Sprites
{
    public Sprite[] sprites;
}
public class FurnitureInfo : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    [SerializeField] int floor;
    [SerializeField] Vector2Int[] size;
    [SerializeField] Vector2Int gridPosition;

    [SerializeField] Vector2Int[] tartgetPosition;
    [SerializeField] int rotation;
    [SerializeField] int spriteNumber;
    bool flip;
    [SerializeField] Sprites[] sprites;
    public int Floor { get { return floor; } }
    public Vector2Int Size { get { return size[rotation]; } }
    public Vector2Int GridPosition { get { return gridPosition; } set => gridPosition = value; }
    public Vector2Int TartgetPosition { get { return tartgetPosition[rotation]; ; } }
    public int Rotation { get { return rotation; } }

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
