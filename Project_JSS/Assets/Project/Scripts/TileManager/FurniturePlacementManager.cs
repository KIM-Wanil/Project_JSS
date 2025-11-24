using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Linq;
using UnityEngine.TextCore.Text;
using TMPro;
using Unity.VisualScripting;

public class FurniturePlacementManager : MonoBehaviour
{
    public FloorData[] floorData;
    public FloorData[] basefloorData;
    [SerializeField] FloorManager[] floorManagers;
    FloorManager floorManager;
    [SerializeField] Collection collection;
    [Header("Settings")]
    public float longPressDuration = 0.5f;
    public float gridSize = 1.0f;
    public LayerMask floorLayerMask;
    public float tileOffsetX = 32f; // ????ùº X ?ò§?îÑ?Öã (?îΩ???)
    public float tileOffsetY = 16f; // ????ùº Y ?ò§?îÑ?Öã (?îΩ???)
    // ?Ç¥Î∂? ?ÉÅ?Éú Î≥??àò
    [SerializeField] IsoGird[] isometricGrids;
    IsoGird currentGrid;
    int gridNumbers;
    int floor;
    FurnitureInfo currentInfo;

    private GameObject selectedFurniture;
    private Vector3? originalPosition;
    private Vector2Int gridPosition;
    private int originalRotation;

    private float pressTime = 0f;
    private bool isLongPress = false;
    private int currentFloor = 0;

    public GameObject furniturePrefab;
    public List<GameObject> availableFurnitureObject;
    private Dictionary<int, List<GameObject>> furnitureByFloor = new Dictionary<int, List<GameObject>>();
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 lastValidPosition;

    private Vector3 dragOffset;

    [Header("UI")]
    [SerializeField] GameObject uiObject;
    [SerializeField] Button rotateButton;            // ?öå?†Ñ Î≤ÑÌäº
    [SerializeField] Button confirmButton;           // ?ôï?†ï Î≤ÑÌäº
    [SerializeField] Button cancelButton;            // Ï∑®ÏÜå Î≤ÑÌäº


    [SerializeField] GameObject skinObject;
    int spriteIndex;
    int tempIndex;
    [SerializeField] Button[] buttons;
    [SerializeField] Image[] buttonsItemImage;
    [SerializeField] TextMeshProUGUI[] buttonsTexs;

    [SerializeField] Sprite[] buttonSprite;
    [SerializeField] int[] buttonsIndex;

    [SerializeField] GameObject[] upDownButtons;
    [SerializeField] bool isTestMode = false;

    bool isSettingTime = false;

    [SerializeField] SpriteRenderer bg_Sprite;
    [SerializeField] SpriteRenderer bg_tile_Sprite;
    private void Awake()
    {
        mainCamera = Camera.main;
        // Ï∏µÎ≥Ñ Í∞?Íµ? Î™©Î°ù Ï¥àÍ∏∞?ôî
        for (int i = 0; i < basefloorData.Length; i++) // ÏµúÎ?? 5Ï∏µÏúºÎ°? Í∞??†ï
        {
            furnitureByFloor[i] = new List<GameObject>();

            //floorData[i].furnitureInfos = basefloorData[i].furnitureInfos;
            floorData[i].Copy(basefloorData[i]);
            //∞≠¡¶ «ÿ±›
            if (i > 1)
            {
                foreach (FurnitureData data in floorData[i].furnitureInfos)
                {
                    data.isUnlocked = true;
                }
            }
            else
            {
                foreach (FurnitureData data in floorData[i].furnitureInfos)
                {
                    data.isUnlocked = false;
                }
            }

          
        }
        rotateButton.onClick.AddListener(RotateFurniture);
        confirmButton.onClick.AddListener(ConfirmPlacement);
        cancelButton.onClick.AddListener(CancelPlacement);
    }
    private void Start()
    {
        for (int i = 0; i< floorData.Length;i++)
        {
            if (floorData[i].isUnlock)
                floor = i;
        }
        SwitchFloor(0);
        //SwitchFloor(0);
    }

    private void Update()
    {
        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0); // Ï≤? Î≤àÏß∏ ?Ñ∞ÏπòÎßå Ï≤òÎ¶¨

        //    switch (touch.phase)
        //    {
        //        case TouchPhase.Began:
        //            {
        //                // UI ?öî?ÜåÎ•? ?Ñ∞ÏπòÌñà?äîÏß? ?ôï?ù∏
        //                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        //                    return;

        //                if (selectedFurniture == null)
        //                {
        //                    // Í∞?Íµ¨Í?? ?Ñ†?Éù?êòÏß? ?ïä??? ?ÉÅ?Éú?óê?Ñú?äî Í∏∏Í≤å ?àÑÎ•¥Í∏∞ ?ãú?ûë
        //                    pressTime = Time.time;
        //                    isLongPress = false;
        //                }
        //                else
        //                {
        //                    Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
        //                    RaycastHit2D hit2D = Physics2D.Raycast(touchPosition, Vector2.zero);

        //                    if (hit2D.collider != null && hit2D.collider.gameObject == selectedFurniture)
        //                    {
        //                        isDragging = true;
        //                    }
        //                }
        //                break;
        //            }

        //        case TouchPhase.Stationary:
        //        case TouchPhase.Moved:
        //            {
        //                if (selectedFurniture == null && !isLongPress && Time.time - pressTime > longPressDuration)
        //                {
        //                    isLongPress = true;
        //                    // Í∏∏Í≤å ?àå?ü¨?Ñú Í∞?Íµ? ?Ñ†?Éù
        //                    SelectFurnitureAtPosition(touch.position);
        //                }
        //                else if (selectedFurniture != null && isDragging)
        //                {
        //                    // ?Ñ†?Éù?êú Í∞?Íµ? ?ù¥?èô
        //                    MoveFurniture(touch.position);
        //                }
        //                break;
        //            }

        //        case TouchPhase.Ended:
        //            {
        //                isDragging = false;
        //                break;
        //            }
        //    }
        //}

        // ?óê?îî?Ñ∞ Î∞? ?Öå?ä§?ä∏?ö© ÎßàÏö∞?ä§ ?ûÖ?†• Ï≤òÎ¶¨
//#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            // UI ?öî?ÜåÎ•? ?Å¥Î¶??ñà?äîÏß? ?ôï?ù∏
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (selectedFurniture == null)
            {
                // Í∞?Íµ¨Í?? ?Ñ†?Éù?êòÏß? ?ïä??? ?ÉÅ?Éú?óê?Ñú?äî Í∏∏Í≤å ?àÑÎ•¥Í∏∞ ?ãú?ûë
                pressTime = Time.time;
                isLongPress = false;
            }
            else
            {
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit2D = Physics2D.Raycast(mousePosition, Vector2.zero);
                if (hit2D.collider != null)
                {
                    if (hit2D.collider.gameObject == selectedFurniture)
                    {
                        dragOffset = (Vector3)mousePosition - selectedFurniture.transform.position;
                        isDragging = true;
                    }
                     
                }
                    // ?ù¥ÎØ? Í∞?Íµ¨Í?? ?Ñ†?Éù?êú ?ÉÅ?Éú?óê?Ñú?äî ?ìú?ûòÍ∑? ?ãú?ûë

            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (selectedFurniture == null && !isLongPress && Time.time - pressTime > longPressDuration)
            {
                isLongPress = true;
                // Í∏∏Í≤å ?àå?ü¨?Ñú Í∞?Íµ? ?Ñ†?Éù
                if (StateManager.instance.ButItem)
                {
                    return;
                }
                SelectFurnitureAtPosition(Input.mousePosition);
            }
            else if (selectedFurniture != null && isDragging)
            {
                    // ?Ñ†?Éù?êú Í∞?Íµ? ?ù¥?èô
                    MoveFurniture(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
//#endif
    }

    // ?Ñ∞Ïπ? ?úÑÏπòÏóê?Ñú Í∞?Íµ? ?Ñ†?Éù
    private void SelectFurnitureAtPosition(Vector2 screenPosition)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.Raycast(mousePosition, Vector2.zero);


        if (hit2D.collider != null)
        {
            GameObject hitObject = hit2D.collider.gameObject;

            // Í∞?Íµ¨Ïù∏Ïß? ?ôï?ù∏ (?ÉúÍ∑∏Î°ú Íµ¨Î∂Ñ?ïòÍ±∞ÎÇò Ïª¥Ìè¨?Ñå?ä∏Î°? ?ôï?ù∏)
            if (hitObject.CompareTag("Furniture"))
            {
                if (hitObject.GetComponent<FurnitureInfo>().FloorIndex != currentFloor)
                {
                    return;
                }
                // ?ù¥ÎØ? ?Ñ†?Éù?êú Í∞?Íµ¨Í?? ?ûà?úºÎ©? ?ï¥?†ú
                if (selectedFurniture != null)
                {
                    currentInfo.SettingSprites(spriteIndex);
                    DeselectFurniture();
                }

                // ?Éà Í∞?Íµ? ?Ñ†?Éù
                selectedFurniture = hitObject;
                //selectedFurniture.GetComponent<SpriteRenderer>().sortingOrder =23;
                currentInfo = hitObject.GetComponent<FurnitureInfo>();
                spriteIndex = currentInfo.SpriteNumber;
                tempIndex = currentInfo.SpriteNumber;
                setButtonImage();
                skinObject.SetActive(true);
                if (currentInfo.IsFloor)
                {
                    currentGrid = isometricGrids[0];
                    gridNumbers = 0;
                    RotateButtonUIOn(true);
                }
                else
                {
                    if (mousePosition.x >=0)
                    {
                        currentGrid = isometricGrids[1];
                        gridNumbers = 1;
                    }

                    else
                    {
                        currentGrid = isometricGrids[2];
                        gridNumbers = 2;
                    }
                    RotateButtonUIOn(false);
                }

                originalPosition = selectedFurniture.transform.position;
                originalRotation = currentInfo.Rotation;
                currentGrid.OccupiedCell(originalPosition.Value, currentInfo.Size, false);
                gridPosition = currentGrid.WorldToGridPosition(originalPosition.Value, currentInfo.HeightLimit);

                dragOffset = (Vector3)mousePosition - selectedFurniture.transform.position;

                Debug.Log(hitObject.name);

                // Ïª®Ìä∏Î°? ?å®?Ñê ?ëú?ãú
                //controlPanel.SetActive(true);

                // ÎßàÏ??Îß? ?ú†?ö® ?úÑÏπ? ????û•
                lastValidPosition = selectedFurniture.transform.position;
                currentGrid.TileSetting(hitObject.transform, currentInfo.Size, currentInfo.GridPosition);
                currentGrid.CanPlaceFurniture(currentInfo.GridPosition);
                CancelButtonUIOn(true);
                uiObject.SetActive(true);
                //uiObject.transform.SetParent(selectedFurniture.transform);
                uiObject.transform.position = new Vector2( selectedFurniture.GetComponent<SpriteRenderer>().bounds.center.x, selectedFurniture.transform.position.y);
                isDragging = true;
            }
        }
    }
    
    // Í∞?Íµ? ?ù¥?èô
    private void MoveFurniture(Vector2 screenPosition)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPosition);

        if (gridNumbers != 0)
        {
            if (pos.x >= 0 && currentGrid != isometricGrids[1])
            {
                currentGrid.FreeViewOff();
                selectedFurniture.transform.localScale = new Vector3(-1, 1, 1);
                currentGrid = isometricGrids[1];
                gridNumbers = 1;
                dragOffset = new Vector3(-dragOffset.x, dragOffset.y,0);
                pos = pos - dragOffset;
                gridPosition = currentGrid.WorldToGridPosition(pos, currentInfo.Size, currentInfo.HeightLimit);
                selectedFurniture.transform.position = currentGrid.GridPositionToWorld(gridPosition);
                currentGrid.TileSetting(selectedFurniture.transform, currentInfo.Size, currentGrid.WorldToGridPosition(selectedFurniture.transform.position,currentInfo.HeightLimit));
                uiObject.transform.position = new Vector2(selectedFurniture.GetComponent<SpriteRenderer>().bounds.center.x, selectedFurniture.transform.position.y);
                return;
            }
            else if (pos.x < 0 && currentGrid != isometricGrids[2])
            {
                currentGrid.FreeViewOff();
                selectedFurniture.transform.localScale = new Vector3(1, 1, 1);
                currentGrid = isometricGrids[2];
                gridNumbers = 2;
                dragOffset = new Vector3(-dragOffset.x, dragOffset.y, 0);
                pos = pos - dragOffset;
                gridPosition = currentGrid.WorldToGridPosition(pos, currentInfo.Size, currentInfo.HeightLimit);
                selectedFurniture.transform.position = currentGrid.GridPositionToWorld(gridPosition);
                currentGrid.TileSetting(selectedFurniture.transform, currentInfo.Size, currentGrid.WorldToGridPosition(selectedFurniture.transform.position, currentInfo.HeightLimit));
                uiObject.transform.position = new Vector2(selectedFurniture.GetComponent<SpriteRenderer>().bounds.center.x, selectedFurniture.transform.position.y);
                return;
            }
            else
            {
                pos = pos - dragOffset;
            }
        }
        else
        {
            pos = pos - dragOffset;
        }
       


        if (gridPosition != currentGrid.WorldToGridPosition(pos,currentInfo.Size, currentInfo.HeightLimit))
        {
            gridPosition = currentGrid.WorldToGridPosition(pos, currentInfo.Size, currentInfo.HeightLimit);
            selectedFurniture.transform.position = currentGrid.GridPositionToWorld(gridPosition);
            currentGrid.CanPlaceFurniture(gridPosition);
            uiObject.transform.position = new Vector2(selectedFurniture.GetComponent<SpriteRenderer>().bounds.center.x, selectedFurniture.transform.position.y);
        }

    }

    // Í∞?Íµ? ?öå?†Ñ
    public void RotateFurniture()
    {
        if (selectedFurniture != null)
        {
         
            currentInfo.RotateSprites();
            currentGrid.Refresh(currentInfo.Size, gridPosition);
            currentGrid.CanPlaceFurniture(gridPosition);
        }
    }

    // Í∞?Íµ? Î∞∞Ïπò ?ôï?†ï
    public void ConfirmPlacement()
    {
        if (selectedFurniture != null && currentGrid.CanPlaceFurniture(gridPosition))
        {
            // Í∑∏Î¶¨?ìú?óê ÎßûÏ∂îÍ∏?
            Vector3 snappedPosition = currentGrid.SortGrid(selectedFurniture.transform.position);
            selectedFurniture.GetComponent<FurnitureInfo>().GridPosition = gridPosition;
            selectedFurniture.transform.position = snappedPosition;
            currentGrid.OccupiedCell(selectedFurniture.transform.position, currentInfo.Size, true);
            // ?Ñ†?Éù ?ÉÅ?Éú ?ï¥?†ú
            DeselectFurniture();

            skinObject.SetActive(false);
            if (StateManager.instance.ButItem)
            {
                StateManager.instance.ButItem = false;
            }

            //∆©≈‰∏ÆæÛOR¥Î»≠ ¡∂∞« »Æ¿Œ
            TutorialManager.OnTriggerConditionEvent(TutorialCondition.∞°±∏º≥ƒ°);
            if (floorData[0].UnlockCounting() == floorData[0].furnitureInfos.Length && DialogueManager.currentEventNum == 2)
            {
                DialogueManager.OnStartDialogueEvent(2);
            }
            else if (floorData[1].UnlockCounting() == floorData[1].furnitureInfos.Length && DialogueManager.currentEventNum == 4)
            {
                DialogueManager.OnStartDialogueEvent(4);
            }
        }
    }
    public void Placement(GameObject gameObject)
    {
        if (gameObject != null)
        {
            // Í∑∏Î¶¨?ìú?óê ÎßûÏ∂îÍ∏?
            Vector3 snappedPosition = currentGrid.GridPositionToWorld(gameObject.GetComponent<FurnitureInfo>().GridPosition);
            gameObject.transform.position = snappedPosition;
            currentGrid.OccupiedCell(selectedFurniture.transform.position, gameObject.GetComponent<FurnitureInfo>().Size, true);
        }
    }
    // Í∞?Íµ? Î∞∞Ïπò Ï∑®ÏÜå
    public void CancelPlacement()
    {
        if (selectedFurniture != null && originalPosition.HasValue)
        {
            // ?õê?ûò ?úÑÏπòÎ°ú ?êò?èåÎ¶¨Í∏∞
            selectedFurniture.transform.position = originalPosition.Value;
            gridPosition = currentGrid.WorldToGridPosition(selectedFurniture.transform.position, currentInfo.HeightLimit);
            selectedFurniture.GetComponent<SpriteRenderer>().sortingOrder =0;
            currentInfo.SettingRotate(originalRotation);
            currentGrid.OccupiedCell(originalPosition.Value, currentInfo.Size, true);
            currentInfo.SettingSprites(spriteIndex);
            // ?Ñ†?Éù ?ÉÅ?Éú ?ï¥?†ú
            DeselectFurniture();
            skinObject.SetActive(false);
        }
    }

    // Í∞?Íµ? ?Ñ†?Éù ?ï¥?†ú
    private void DeselectFurniture()
    {
        if (selectedFurniture != null)
        {

            selectedFurniture = null;
            originalPosition = null;
            originalRotation = 0;
            currentGrid.FreeViewOff();
            currentInfo = null;
            currentGrid = null;
            uiObject.SetActive(false);
           
        }
    }
    // Ï∏? Î≥?Í≤?
    public void SwitchFloor(int floorNumber)
    {
        if (floorNumber < 0 || floorNumber >= floorData.Length || floorManagers[floorNumber].floorData.isUnlock == false || isSettingTime)
            return;
        isSettingTime = true;

        bg_Sprite.DOColor(floorData[floorNumber].bg_color, 0.25f);
        bg_tile_Sprite.DOColor(floorData[floorNumber].bg_tile_color, 0.25f);

        Managers.Asset.PlayBGMFadeInSound(floorData[floorNumber].floorBGM,0.5f);
        if (selectedFurniture != null)
        {
            currentInfo.SettingSprites(spriteIndex);
            DeselectFurniture();
        }
        floorManager = floorManagers[floorNumber];
        mainCamera.transform.DOMove(new Vector3(0, 7.5f + 5.15f * floorNumber, -10f),0.5f);
        for (int i = 0; i <= floorNumber; i++)
        {
            floorManagers[i].gameObject.SetActive(true);
        }
        for (int i = floorNumber +1; i<floorManagers.Length;i++)
        {
            floorManagers[i].gameObject.SetActive(false);
        }
        floorManager.gameObject.SetActive(true);
        floor = floorNumber;

        isometricGrids = floorManager.grids;
        furniturePrefab = floorManager.furniturePrefab;
        currentFloor = floorNumber;
        skinObject.SetActive(false);

        StartCoroutine(SettingDelay());
        if (floorNumber == 0)
        {
            upDownButtons[0].SetActive(false);
        }
        else if (floorNumber == floorData.Length-1)
        {
            upDownButtons[1].SetActive(false);
        }
        else
        {
            upDownButtons[0].SetActive(true);
            upDownButtons[1].SetActive(true);
        }
    }
    IEnumerator SettingDelay()
    {
        yield return new WaitForSeconds(0.5f);
        isSettingTime = false;
        yield return null;
    }
    // Í∞?Íµ? Ï∂îÍ??
    public void AddFurnitureToFloor(FurnitureData data, int floor)
    {
        SwitchFloor(floor);

        GameObject obj = floorManagers[floor].AddFurniture(data);
        if (obj != null)
        {
            // ?ù¥ÎØ? ?Ñ†?Éù?êú Í∞?Íµ¨Í?? ?ûà?úºÎ©? ?ï¥?†ú
            if (selectedFurniture != null)
            {
                currentInfo.SettingSprites(spriteIndex);
                DeselectFurniture();
            }

            // ?Éà Í∞?Íµ? ?Ñ†?Éù
            selectedFurniture = obj;
            //selectedFurniture.GetComponent<SpriteRenderer>().sortingOrder =23;
            currentInfo = obj.GetComponent<FurnitureInfo>();
            spriteIndex = currentInfo.SpriteNumber;
            tempIndex = currentInfo.SpriteNumber;
            setButtonImage();
            skinObject.SetActive(true);
            if (currentInfo.IsFloor)
            {
                currentGrid = isometricGrids[0];
                gridNumbers = 0;
                RotateButtonUIOn(true);
            }
            else
            {
                if (!data.isLeft)
                {
                    currentGrid = isometricGrids[1];
                    gridNumbers = 1;
                    obj.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    currentGrid = isometricGrids[2];
                    gridNumbers = 2;

                }
                RotateButtonUIOn(false);
            }
              
            originalPosition = selectedFurniture.transform.position;
            originalRotation = currentInfo.Rotation;

            gridPosition = currentGrid.WorldToGridPosition(originalPosition.Value, currentInfo.HeightLimit);

            dragOffset = Vector3.zero;

            currentGrid.TileSetting(obj.transform, currentInfo.Size, currentInfo.GridPosition);
            currentGrid.CanPlaceFurniture(currentInfo.GridPosition);
            CancelButtonUIOn(false);
            uiObject.SetActive(true);

            uiObject.transform.position = new Vector2(selectedFurniture.GetComponent<SpriteRenderer>().bounds.center.x, selectedFurniture.transform.position.y);
            isDragging = true;

            collection.UpdateSlider(floor);
        }
    }

    // UI ?óÖ?ç∞?ù¥?ä∏ (Ï∏? ?ëú?ãú)
    private void CancelButtonUIOn(bool b)
    {
        cancelButton.gameObject.SetActive(b);
    }
    private void RotateButtonUIOn(bool b)
    {
        rotateButton.gameObject.SetActive(b);
    }
    public void SwitchFloor(bool Up)
    {
        if (StateManager.instance.ButItem)
        {
            return;
        }
        if (Up)
        {
            SwitchFloor(floor + 1);
        }
        else
        {
            SwitchFloor(floor - 1);
        }
    }

    void setButtonImage()
    {
        Sprites[] sprites = selectedFurniture.GetComponent<FurnitureInfo>().MySprites;
        int index = selectedFurniture.GetComponent<FurnitureInfo>().SpriteNumber;
        if (tempIndex == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                buttonsItemImage[i].sprite = sprites[tempIndex + i].lockSprite;
                buttonsTexs[i].text = sprites[tempIndex + i].lockSprite.name;
                if (sprites[tempIndex + i].isUnlocked)
                {
                    buttonsItemImage[i].color = new Color(1, 1, 1, 1);
                }
                else
                {
                    buttonsItemImage[i].color = new Color(0, 0, 0, 1);
                }
                buttonsIndex[i] = tempIndex + i;

                if (tempIndex + i == index)
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[0];
                else
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[1];
            }
        }
        else if (tempIndex == sprites.Length - 1)
        {
            for (int i = 0; i < 3; i++)
            {
                buttonsItemImage[i].sprite = sprites[tempIndex + i - 2].lockSprite;
                buttonsTexs[i].text = sprites[tempIndex + i - 2].lockSprite.name;
                if (sprites[tempIndex + i - 2].isUnlocked)
                    buttonsItemImage[i].color = new Color(1, 1, 1, 1);
                else
                    buttonsItemImage[i].color = new Color(0, 0, 0, 1);
                buttonsIndex[i] = tempIndex + i;
                if (tempIndex + i - 2 == index)
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[0];
                else
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[1];
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                buttonsItemImage[i].sprite = sprites[tempIndex + i - 1].lockSprite;
                buttonsTexs[i].text = sprites[tempIndex + i - 1].lockSprite.name;

                if (sprites[tempIndex + i - 1].isUnlocked)
                    buttonsItemImage[i].color = new Color(1, 1, 1, 1);
                else
                    buttonsItemImage[i].color = new Color(0, 0, 0, 1);
                buttonsIndex[i] = tempIndex + i - 1;
                if (tempIndex + i - 1 == index)
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[0];
                else
                    buttons[i].GetComponent<Image>().sprite = buttonSprite[1];
            }
        }
    }

    public void MoveButton(bool isLeft)
    {
        ChangeIndex(ref tempIndex, selectedFurniture.GetComponent<FurnitureInfo>().MySprites.Length, isLeft);
        setButtonImage();
        currentInfo.SettingSprites(tempIndex);
    }
    void ChangeIndex(ref int index, int maxValue, bool isLeft)
    {
        if (isLeft)
        {
            index--;
            if (index < 0)
            {
                index = 0;
            }
        }
        else
        {
            index++;
            if (index >= maxValue)
            {
                index = maxValue - 1;
            }
        }

    }
    public void SetSkin(int index)
    {

        currentInfo.SettingSprites(buttonsIndex[index]);
        setButtonImage();
    }

}