using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MergeableItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] protected int lv = 1;
    //[SerializeField] protected string itemId;
    [SerializeField] protected Image itemImage;
    public ItemData itemData;


    [Header("Effects")]
    [SerializeField] protected ParticleSystem mergeEffect;
    [SerializeField] protected ParticleSystem spawnEffect;

    [Header("Events")]
    public UnityEvent<int> onLevelChanged;
    public UnityEvent onMerged;
    public UnityEvent onSpawned;

    protected Vector2Int gridPosition;
    protected bool isInitialized = false;

    public int Lv => lv;
    public Vector2Int GridPosition => gridPosition;
    public Image ItemImage => itemImage;
    
    private void Awake()
    {
        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }
    }

    public void Initialize(int newLevel)
    {
        lv = Mathf.Clamp(newLevel, 1, itemData.items.Length);
        UpdateVisuals();
        isInitialized = true;

        if (spawnEffect != null)
        {
            spawnEffect.Play();
        }

        onSpawned?.Invoke();
    }

    public void Initialize(SaveData.ItemData saveData)
    {
        //itemId = saveData.itemId;
        lv = saveData.level;
        gridPosition = saveData.position;
        UpdateVisuals();
        isInitialized = true;
    }

    protected void UpdateVisuals()
    {
        if (itemImage != null  && itemData.items.Length > 0)
        {
            int levelIndex = Mathf.Clamp(lv - 1, 0, itemData.items.Length - 1);
            itemImage.sprite = itemData.items[levelIndex].itemSprite;
        }
    }

    public bool CanMergeWith(MergeableItem other)
    {
        return other != null &&
               other != this &&
               other.itemData.id == itemData.id &&
               other.lv == lv &&
               lv < itemData.items.Length; // 최대 레벨 체크
    }

    public void OnMerged()
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

    public void LevelUp()
    {
        if (lv < itemData.items.Length)
        {
            lv++;
            UpdateVisuals();
            onLevelChanged?.Invoke(lv);
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
    public void OnBeginDrag()
    {
        //image.sortingOrder = 10; // 드래그 중인 아이템을 최상위로
    }

    public void OnEndDrag()
    {
        //image.sortingOrder = 0; // 원래 정렬 순서로 복구
    }

    // 저장/로드를 위한 데이터 직렬화
    public SaveData.ItemData GetSaveData()
    {
        return new SaveData.ItemData
        {
            //itemId = itemId,
            level = lv,
            position = gridPosition
        };
    }
}

