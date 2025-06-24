using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Linq;
using UnityEditor.U2D.Animation;
using UnityEngine.TextCore.Text;

public class FurniturePlacementManager : MonoBehaviour
{
    public FloorData[] floorData;
    [SerializeField] FloorManager[] floorManagers;
    FloorManager floorManager;
    [SerializeField] Collection collection;
    [Header("Settings")]
    public float longPressDuration = 0.5f;
    public float gridSize = 1.0f;
    public LayerMask floorLayerMask;
    public float tileOffsetX = 32f; // 타일 X 오프셋 (픽셀)
    public float tileOffsetY = 16f; // 타일 Y 오프셋 (픽셀)
    // 내부 상태 변수
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
    [SerializeField] Button rotateButton;            // 회전 버튼
    [SerializeField] Button confirmButton;           // 확정 버튼
    [SerializeField] Button cancelButton;            // 취소 버튼


    [SerializeField] GameObject skinObject;
    int spriteIndex;
    int tempIndex;
    [SerializeField] Button[] buttons;
    [SerializeField] Image[] buttonsItemImage;

    [SerializeField] Sprite[] buttonSprite;
    [SerializeField] int[] buttonsIndex;

    Coroutine settinCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;
        // 층별 가구 목록 초기화
        for (int i = 0; i < 5; i++) // 최대 5층으로 가정
        {
            furnitureByFloor[i] = new List<GameObject>();
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
        SwitchFloor(floor);
        //SwitchFloor(0);
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // 첫 번째 터치만 처리

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
                        // UI 요소를 터치했는지 확인
                        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                            return;

                        if (selectedFurniture == null)
                        {
                            // 가구가 선택되지 않은 상태에서는 길게 누르기 시작
                            pressTime = Time.time;
                            isLongPress = false;
                        }
                        else
                        {
                            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                            RaycastHit2D hit2D = Physics2D.Raycast(touchPosition, Vector2.zero);

                            if (hit2D.collider != null && hit2D.collider.gameObject == selectedFurniture)
                            {
                                isDragging = true;
                            }
                        }
                        break;
                    }

                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    {
                        if (selectedFurniture == null && !isLongPress && Time.time - pressTime > longPressDuration)
                        {
                            isLongPress = true;
                            // 길게 눌러서 가구 선택
                            SelectFurnitureAtPosition(touch.position);
                        }
                        else if (selectedFurniture != null && isDragging)
                        {
                            // 선택된 가구 이동
                            MoveFurniture(touch.position);
                        }
                        break;
                    }

                case TouchPhase.Ended:
                    {
                        isDragging = false;
                        break;
                    }
            }
        }

        // 에디터 및 테스트용 마우스 입력 처리
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            // UI 요소를 클릭했는지 확인
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (selectedFurniture == null)
            {
                // 가구가 선택되지 않은 상태에서는 길게 누르기 시작
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
                    // 이미 가구가 선택된 상태에서는 드래그 시작

            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (selectedFurniture == null && !isLongPress && Time.time - pressTime > longPressDuration)
            {
                isLongPress = true;
                // 길게 눌러서 가구 선택
                if (StateManager.instance.ButItem)
                {
                    return;
                }
                SelectFurnitureAtPosition(Input.mousePosition);
            }
            else if (selectedFurniture != null && isDragging)
            {
                    // 선택된 가구 이동
                    MoveFurniture(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
#endif
    }

    // 터치 위치에서 가구 선택
    private void SelectFurnitureAtPosition(Vector2 screenPosition)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.Raycast(mousePosition, Vector2.zero);


        if (hit2D.collider != null)
        {
            GameObject hitObject = hit2D.collider.gameObject;

            // 가구인지 확인 (태그로 구분하거나 컴포넌트로 확인)
            if (hitObject.CompareTag("Furniture"))
            {
                if (hitObject.GetComponent<FurnitureInfo>().FloorIndex != currentFloor)
                {
                    return;
                }
                // 이미 선택된 가구가 있으면 해제
                if (selectedFurniture != null)
                {
                    currentInfo.SettingSprites(spriteIndex);
                    DeselectFurniture();
                }

                // 새 가구 선택
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
                      
                }

                originalPosition = selectedFurniture.transform.position;
                originalRotation = currentInfo.Rotation;
                currentGrid.OccupiedCell(originalPosition.Value, currentInfo.Size, false);
                gridPosition = currentGrid.WorldToGridPosition(originalPosition.Value);

                dragOffset = (Vector3)mousePosition - selectedFurniture.transform.position;

                Debug.Log(hitObject.name);

                // 컨트롤 패널 표시
                //controlPanel.SetActive(true);

                // 마지막 유효 위치 저장
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
    
    // 가구 이동
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
                gridPosition = currentGrid.WorldToGridPosition(pos, currentInfo.Size);
                selectedFurniture.transform.position = currentGrid.GridPositionToWorld(gridPosition);
                currentGrid.TileSetting(selectedFurniture.transform, currentInfo.Size, currentGrid.WorldToGridPosition(selectedFurniture.transform.position));
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
                gridPosition = currentGrid.WorldToGridPosition(pos, currentInfo.Size);
                selectedFurniture.transform.position = currentGrid.GridPositionToWorld(gridPosition);
                currentGrid.TileSetting(selectedFurniture.transform, currentInfo.Size, currentGrid.WorldToGridPosition(selectedFurniture.transform.position));
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
       


        if (gridPosition != currentGrid.WorldToGridPosition(pos,currentInfo.Size))
        {
            gridPosition = currentGrid.WorldToGridPosition(pos, currentInfo.Size);
            selectedFurniture.transform.position = currentGrid.GridPositionToWorld(gridPosition);
            currentGrid.CanPlaceFurniture(gridPosition);
            uiObject.transform.position = new Vector2(selectedFurniture.GetComponent<SpriteRenderer>().bounds.center.x, selectedFurniture.transform.position.y);
        }

    }

    // 가구 회전
    public void RotateFurniture()
    {
        if (selectedFurniture != null)
        {
            currentInfo.RotateSprites();
        }
    }

    // 가구 배치 확정
    public void ConfirmPlacement()
    {
        if (selectedFurniture != null && currentGrid.CanPlaceFurniture(gridPosition))
        {
            // 그리드에 맞추기
            Vector3 snappedPosition = currentGrid.SortGrid(selectedFurniture.transform.position);
            selectedFurniture.GetComponent<FurnitureInfo>().GridPosition = gridPosition;
            selectedFurniture.transform.position = snappedPosition;
            currentGrid.OccupiedCell(selectedFurniture.transform.position, currentInfo.Size, true);
            // 선택 상태 해제
            DeselectFurniture();

            skinObject.SetActive(false);
            if (StateManager.instance.ButItem)
            {
                StateManager.instance.ButItem = false;
            }
        }
    }
    public void Placement(GameObject gameObject)
    {
        if (gameObject != null)
        {
            // 그리드에 맞추기
            Vector3 snappedPosition = currentGrid.GridPositionToWorld(gameObject.GetComponent<FurnitureInfo>().GridPosition);
            gameObject.transform.position = snappedPosition;
            currentGrid.OccupiedCell(selectedFurniture.transform.position, gameObject.GetComponent<FurnitureInfo>().Size, true);
        }
    }
    // 가구 배치 취소
    public void CancelPlacement()
    {
        if (selectedFurniture != null && originalPosition.HasValue)
        {
            // 원래 위치로 되돌리기
            selectedFurniture.transform.position = originalPosition.Value;
            gridPosition = currentGrid.WorldToGridPosition(selectedFurniture.transform.position);
            selectedFurniture.GetComponent<SpriteRenderer>().sortingOrder =0;
            currentInfo.SettingRotate(originalRotation);
            currentGrid.OccupiedCell(originalPosition.Value, currentInfo.Size, true);
            currentInfo.SettingSprites(spriteIndex);
            // 선택 상태 해제
            DeselectFurniture();
            skinObject.SetActive(false);
        }
    }

    // 가구 선택 해제
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
    // 층 변경
    public void SwitchFloor(int floorNumber)
    {
        if (floorNumber < 0 || floorNumber >= floorData.Length || floorManagers[floorNumber].floorData.isUnlock == false)
            return;
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
      //  floorManager.gameObject.SetActive(true);
        floor = floorNumber;

        isometricGrids = floorManager.grids;
        furniturePrefab = floorManager.furniturePrefab;

        if (settinCoroutine != null)
        {
            StopCoroutine(settinCoroutine);
        }
        // 선택된 가구가 있으면 선택 해제
        if (selectedFurniture != null)
        {
            currentInfo.SettingSprites(spriteIndex);
            DeselectFurniture();
        }
        currentFloor = floorNumber;
        skinObject.SetActive(false);
    }
   

    // 가구 추가
    public void AddFurnitureToFloor(FurnitureData data, int floor)
    {
        SwitchFloor(floor);

        GameObject obj = floorManagers[floor].AddFurniture(data);
        if (obj != null)
        {
            // 이미 선택된 가구가 있으면 해제
            if (selectedFurniture != null)
            {
                currentInfo.SettingSprites(spriteIndex);
                DeselectFurniture();
            }

            // 새 가구 선택
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
            }
            else
            {
                if (!data.isLeft)
                {
                    currentGrid = isometricGrids[1];
                    gridNumbers = 1;
                }
                else
                {
                    currentGrid = isometricGrids[2];
                    gridNumbers = 2;
                }
            }

            originalPosition = selectedFurniture.transform.position;
            originalRotation = currentInfo.Rotation;

            gridPosition = currentGrid.WorldToGridPosition(originalPosition.Value);

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

    // UI 업데이트 (층 표시)
    private void CancelButtonUIOn(bool b)
    {
        cancelButton.gameObject.SetActive(b);
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
                buttonsItemImage[i].sprite = sprites[tempIndex + i].sprites[0];
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
                buttonsItemImage[i].sprite = sprites[tempIndex + i - 2].sprites[0];
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
                buttonsItemImage[i].sprite = sprites[tempIndex + i - 1].sprites[0];
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