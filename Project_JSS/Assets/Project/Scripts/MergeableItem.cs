using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MergeableItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] protected int level = 1;
    [SerializeField] protected string itemId;
    [SerializeField] protected Image image;
    [SerializeField] protected Sprite[] levelSprites; // ������ ��������Ʈ

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
               level < levelSprites.Length; // �ִ� ���� üũ
    }

    public virtual void OnMerged()
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

    public virtual void LevelUp()
    {
        if (level < levelSprites.Length)
        {
            level++;
            UpdateVisuals();
            onLevelChanged?.Invoke(level);
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
    public virtual void OnBeginDrag()
    {
        //image.sortingOrder = 10; // �巡�� ���� �������� �ֻ�����
    }

    public virtual void OnEndDrag()
    {
        //image.sortingOrder = 0; // ���� ���� ������ ����
    }

    // ����/�ε带 ���� ������ ����ȭ
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

