using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager instance;

    public bool ButItem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    void Start()
    {
        ButItem = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
