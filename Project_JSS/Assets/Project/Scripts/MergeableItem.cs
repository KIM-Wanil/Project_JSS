using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MergeableItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] protected int level = 1;
    [SerializeField] protected string itemId;
    [SerializeField] protected Image image;
    [SerializeField] protected Sprite[] levelSprites; // 레벨별 스프라이트

    [Header("Effects")]
    [SerializeField] protected ParticleSystem mergeEffect;
    [SerializeField] protected ParticleSystem spawnEffect;

    [Header("Events")]
    public UnityEvent<int> onLevelChanged;
    public UnityEvent onMerged;
    public UnityEvent onSpawned;

    protected Vector2Int gridPosition;
    protected bool isInitialized = false;

    public int Level => level;
    public string ItemId => itemId;
    public Vector2Int GridPosition => gridPosition;

    private void Awake()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    public virtual void Initialize(int newLevel)
    {
        level = Mathf.Clamp(newLevel, 1, levelSprites.Length);
        UpdateVisuals();
        isInitialized = true;

        if (spawnEffect != null)
        {
            spawnEffect.Play();
        }

        onSpawned?.Invoke();
    }

    public virtual void Initialize(SaveData.ItemData saveData)
    {
        itemId = saveData.itemId;
        level = saveData.level;
        gridPosition = saveData.position;
        UpdateVisuals();
        isInitialized = true;
    }

    protected virtual void UpdateVisuals()
    {
        if (image != null && levelSprites != null && levelSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(level - 1, 0, levelSprites.Length - 1);
            image.sprite = levelSprites[spriteIndex];
        }
    }

    public virtual bool CanMergeWith(MergeableItem other)
    {
        return other != null &&
               other != this &&
               other.itemId == itemId &&
               other.level == level &&
               level < levelSprites.Length; // 최대 레벨 체크
    }

    public virtual void OnMerged()
    {
        if (mergeEffect != null)
        {
            // 이펙트를 부모에서 분리하고 자동 제거되도록 설정
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
            Destroy(mergeEffect.gameObject, mergeEffect.main.duration);
        }

        onMerged?.Invoke();
        Managers.Grid.RemoveItem(gridPosition);
    }

    public void SetGridPosition(Vector2Int pos)
    {
        gridPosition = pos;
    }

    public virtual void LevelUp()
    {
        if (level < levelSprites.Length)
        {
            level++;
            UpdateVisuals();
            onLevelChanged?.Invoke(level);
        }
    }

    // 아이템 타입별 특수 효과를 위한 가상 메서드들
    protected virtual void OnItemPlaced() { }
    protected virtual void OnItemRemoved() { }
    protected virtual void OnItemMoved() { }

    // 애니메이션 관련 메서드
    public virtual void PlayMergeAnimation(Vector3 targetPosition, System.Action onComplete = null)
    {
        // 기본 구현: 단순 이동
        StartCoroutine(MergeAnimationCoroutine(targetPosition, onComplete));
    }

    private System.Collections.IEnumerator MergeAnimationCoroutine(Vector3 targetPosition, System.Action onComplete)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 이징 함수 적용
            t = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        onComplete?.Invoke();
    }

    // 드래그 관련 메서드
    public virtual void OnBeginDrag()
    {
        //image.sortingOrder = 10; // 드래그 중인 아이템을 최상위로
    }

    public virtual void OnEndDrag()
    {
        //image.sortingOrder = 0; // 원래 정렬 순서로 복구
    }

    // 저장/로드를 위한 데이터 직렬화
    public virtual SaveData.ItemData GetSaveData()
    {
        return new SaveData.ItemData
        {
            itemId = itemId,
            level = level,
            position = gridPosition
        };
    }
}

