// ��ü���� ������ Ÿ�� ����
using UnityEngine;

public class GeneratedItem : MergeableItem
{
    [Header("Generated Item Settings")]
    [SerializeField] private float lifeTime = -1f; // -1�� ������
    [SerializeField] private int pointValue = 10;

    private float remainingTime;

    //public void Initialize(int newLevel)
    //{
    //    base.Initialize(newLevel);

    //    if (lifeTime > 0)
    //    {
    //        remainingTime = lifeTime;
    //        StartCoroutine(LifeTimeCoroutine());
    //    }
    //}

    //private System.Collections.IEnumerator LifeTimeCoroutine()
    //{
    //    while (remainingTime > 0)
    //    {
    //        remainingTime -= Time.deltaTime;

    //        // �ð��� �� �� ������ �� �ð��� �ǵ��
    //        if (remainingTime < 3f)
    //        {
    //            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
    //            image.color = new Color(1f, 1f, 1f, alpha);
    //        }

    //        yield return null;
    //    }

    //    // ������ ���� ������ ����
    //    OnLifeTimeEnd();
    //}

    //protected virtual void OnLifeTimeEnd()
    //{
    //    Managers.Grid.RemoveItem(gridPosition);
    //    Managers.Game.ReturnItemToPool(gameObject);
    //}

    //protected override void OnItemPlaced()
    //{
    //    base.OnItemPlaced();
    //    GameManager.Instance.AddScore(pointValue * level);
    //}
}