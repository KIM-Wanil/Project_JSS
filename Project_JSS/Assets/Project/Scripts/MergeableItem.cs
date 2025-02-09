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
               lv < itemData.items.Length; // �ִ� ���� üũ
    }

    public void OnMerged()
    {
        if (mergeEffect != null)
        {
            // ����Ʈ�� �θ𿡼� �и��ϰ� �ڵ� ���ŵǵ��� ����
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

    // ������ Ÿ�Ժ� Ư�� ȿ���� ���� ���� �޼����
    protected virtual void OnItemPlaced() { }
    protected virtual void OnItemRemoved() { }
    protected virtual void OnItemMoved() { }

    // �ִϸ��̼� ���� �޼���
    public virtual void PlayMergeAnimation(Vector3 targetPosition, System.Action onComplete = null)
    {
        // �⺻ ����: �ܼ� �̵�
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

            // ��¡ �Լ� ����
            t = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        onComplete?.Invoke();
    }

    // �巡�� ���� �޼���
    public void OnBeginDrag()
    {
        //image.sortingOrder = 10; // �巡�� ���� �������� �ֻ�����
    }

    public void OnEndDrag()
    {
        //image.sortingOrder = 0; // ���� ���� ������ ����
    }

    // ����/�ε带 ���� ������ ����ȭ
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

