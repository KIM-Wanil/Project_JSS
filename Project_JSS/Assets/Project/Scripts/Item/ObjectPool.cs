// ObjectPool.cs - 제네릭 오브젝트 풀 클래스
using System.Collections.Generic;

public class ObjectPool<T>
{
    private readonly System.Func<T> createFunc;
    private readonly System.Action<T> actionOnGet;
    private readonly System.Action<T> actionOnRelease;
    private readonly System.Action<T> actionOnDestroy;
    private readonly int maxSize;

    private readonly Stack<T> pool;
    private readonly bool collectionCheck;

    public ObjectPool(System.Func<T> createFunc,
        System.Action<T> actionOnGet = null,
        System.Action<T> actionOnRelease = null,
        System.Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
    {
        this.createFunc = createFunc;
        this.actionOnGet = actionOnGet;
        this.actionOnRelease = actionOnRelease;
        this.actionOnDestroy = actionOnDestroy;
        this.maxSize = maxSize;
        this.collectionCheck = collectionCheck;

        pool = new Stack<T>(defaultCapacity);
    }

    public T Get()
    {
        T item;
        if (pool.Count == 0)
        {
            item = createFunc();
        }
        else
        {
            item = pool.Pop();
        }

        actionOnGet?.Invoke(item);
        return item;
    }

    public void Release(T item)
    {
        if (collectionCheck && pool.Count > 0 && pool.Contains(item))
        {
            throw new System.InvalidOperationException("Trying to release an item that has already been released to the pool.");
        }

        actionOnRelease?.Invoke(item);

        if (pool.Count < maxSize)
        {
            pool.Push(item);
        }
        else
        {
            actionOnDestroy?.Invoke(item);
        }
    }

    public void Clear()
    {
        if (actionOnDestroy != null)
        {
            foreach (T item in pool)
            {
                actionOnDestroy(item);
            }
        }

        pool.Clear();
    }
}