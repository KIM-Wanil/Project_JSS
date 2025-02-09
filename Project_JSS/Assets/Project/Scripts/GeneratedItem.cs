// 구체적인 아이템 타입 예시
using UnityEngine;

public class GeneratedItem : MergeableItem
{
    [Header("Generated Item Settings")]
    [SerializeField] private float lifeTime = -1f; // -1은 무제한
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

    //        // 시간이 얼마 안 남았을 때 시각적 피드백
    //        if (remainingTime < 3f)
    //        {
    //            float alpha = Mathf.PingPong(Time.time * 2f, 1f);
    //            image.color = new Color(1f, 1f, 1f, alpha);
    //        }

    //        yield return null;
    //    }

    //    // 수명이 다한 아이템 제거
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