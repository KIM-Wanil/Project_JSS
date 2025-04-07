using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FurniturePlacementManager : MonoBehaviour
{
    [SerializeField] FloorData[] floorData;
    [Header("Settings")]
    public float longPressDuration = 0.5f;
    public float gridSize = 1.0f;
    public LayerMask floorLayerMask;
    public float tileOffsetX = 32f; // 타일 X 오프셋 (픽셀)
    public float tileOffsetY = 16f; // 타일 Y 오프셋 (픽셀)
    // 내부 상태 변수
    [SerializeField] IsoGird[] isometricGrids;
    IsoGird currentGrid;
    FurnitureInfo currentInfo;

    private GameObject selectedFurniture;
    private Vector3? originalPosition;
    private Vector2Int gridPosition;
    private int originalRotation;

    private float pressTime = 0f;
    private bool isLongPress = false;
    private int currentFloor = 0;

    public Transform furnitureParent; // 가구들을 담을 부모 객체
    public GameObject furniturePrefab;
    public List<GameObject> availableFurnitureObject;
    Queue<GameObject> furnitureObjectQueue;
    private Dictionary<int, List<GameObject>> furnitureByFloor = new Dictionary<int, List<GameObject>>();
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 lastValidPosition;

    private Vector3 dragOffset;

    [SerializeField] GameObject tile;
    List<GameObject> tiles;
    [Header("UI")]
    [SerializeField] GameObject uiObject;
    [SerializeField] Button rotateButton;            // 회전 버튼
    [SerializeField] Button confirmButton;           // 확정 버튼
    [SerializeField] Button cancelButton;            // 취소 버튼

    private void Awake()
    {
        mainCamera = Camera.main;
        // 층별 가구 목록 초기화
        for (int i = 0; i < 5; i++) // 최대 5층으로 가정
        {
            furnitureByFloor[i] = new List<GameObject>();
        }
        tiles = new List<GameObject>();
        rotateButton.onClick.AddListener(RotateFurniture);
        confirmButton.onClick.AddListener(ConfirmPlacement);
        cancelButton.onClick.AddListener(CancelPlacement);
        SwitchFloor(0);
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
                
                // 이미 선택된 가구가 있으면 해제
                if (selectedFurniture != null)
                {
                    DeselectFurniture();
                }

                // 새 가구 선택
                selectedFurniture = hitObject;
                //selectedFurniture.GetComponent<SpriteRenderer>().sortingOrder =23;
                currentInfo = hitObject.GetComponent<FurnitureInfo>();
                if (currentInfo.IsFloor)
                {
                    currentGrid = isometricGrids[0];
                }
                else
                {
                    if (mousePosition.x >=0) 
                        currentGrid = isometricGrids[1];
                    else
                        currentGrid = isometricGrids[2];
                }

                originalPosition = selectedFurniture.transform.position;
                originalRotation = currentInfo.Rotation;
                currentGrid.OccupiedCell(originalPosition.Value, currentInfo.Size, false);
                gridPosition = currentGrid.WorldToGridPosition(originalPosition.Value);

                dragOffset = (Vector3)mousePosition - selectedFurniture.transform.position;

                // 선택 표시 (예: 외곽선 효과)
                HighlightFurniture(true);
                Debug.Log(hitObject.name);

                // 컨트롤 패널 표시
                //controlPanel.SetActive(true);

                // 마지막 유효 위치 저장
                lastValidPosition = selectedFurniture.transform.position;

                for (int x = 0;x <currentInfo.Size.x;x++)
                {
                    for (int y = 0; y < currentInfo.Size.y; y++)
                    {
                       GameObject obj = Instantiate(tile,new Vector2(originalPosition.Value.x + tileOffsetX*(x-y)*0.5f, originalPosition.Value.y + tileOffsetY* (x + y) * 0.5f), hitObject.transform.rotation, hitObject.transform);
                       tiles.Add(obj);
                    }
                }
                uiObject.SetActive(true);
                uiObject.transform.SetParent(selectedFurniture.transform);
                uiObject.transform.position = selectedFurniture.transform.position;
                isDragging = true;
            }
        }
    }
    
    public bool CanPlaceFurniture()
    {
        Vector2Int size = currentInfo.Size;
        bool IsCan = true;
        Vector2Int pos = gridPosition;
        int num = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (!currentGrid.CanPlaceFurniture(new Vector2Int(pos.x+x, pos.y+y)))
                {
                    Debug.Log("Can not Place");
                    tiles[num].GetComponent<SpriteRenderer>().color = Color.red;
                    IsCan =  false;
                }
                else
                {
                    tiles[num].GetComponent<SpriteRenderer>().color = Color.green;
                }
                num++;
            }
        }
        return IsCan;
    }
    // 가구 이동
    private void MoveFurniture(Vector2 screenPosition)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(screenPosition);
        pos = pos - dragOffset;
        if (gridPosition != currentGrid.WorldToGridPosition(pos))
        {
            gridPosition = currentGrid.WorldToGridPosition(pos);
            Vector3 snappedPosition = currentGrid.GridPositionToWorld(gridPosition);
            selectedFurniture.transform.position = snappedPosition; 
            CanPlaceFurniture();
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
        if (selectedFurniture != null && CanPlaceFurniture())
        {
            // 그리드에 맞추기
            Vector3 snappedPosition = currentGrid.SortGrid(selectedFurniture.transform.position);
            selectedFurniture.GetComponent<FurnitureInfo>().GridPosition = gridPosition;
            selectedFurniture.transform.position = snappedPosition;
            currentGrid.OccupiedCell(selectedFurniture.transform.position, currentInfo.Size, true);
            // 선택 상태 해제
            DeselectFurniture();
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
            // 선택 상태 해제
            DeselectFurniture();
        }
    }

    // 가구 선택 해제
    private void DeselectFurniture()
    {
        if (selectedFurniture != null)
        {
            // 선택 효과 제거
            HighlightFurniture(false);

            selectedFurniture = null;
            originalPosition = null;
            originalRotation = 0;
            currentInfo = null;
            currentGrid = null;
            uiObject.SetActive(false);
            FreeViewOff();
        }
    }
    public void FreeViewOff()
    {
        foreach (GameObject gameObject in tiles)
        {
            Destroy(gameObject);
        }
        tiles.Clear();
    }
    // 그리드 스냅 기능
    private Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = position.y; // 높이는 유지
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
    }

    // 가구 하이라이트 표시
    private void HighlightFurniture(bool highlight)
    {
        // 가구의 모든 렌더러 컴포넌트 찾기
        Renderer[] renderers = selectedFurniture.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            // 재질의 외곽선 효과 설정 (쉐이더에 따라 다를 수 있음)
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                if (highlight)
                {
                    // 선택 효과 설정 (예: Outline 효과나 색상 변경)
                    material.SetFloat("_OutlineWidth", 0.02f);
                    material.SetColor("_OutlineColor", Color.yellow);
                }
                else
                {
                    // 선택 효과 제거
                    material.SetFloat("_OutlineWidth", 0f);
                }
            }
        }
    }
    // 층 변경
    public void SwitchFloor(int floorNumber)
    {
        if (floorNumber < 0 || floorNumber >= floorData.Length)
            return;

        // 선택된 가구가 있으면 선택 해제
        if (selectedFurniture != null)
        {
            DeselectFurniture();
        }

        // 현재 층 가구 비활성화
        foreach (GameObject furniture in availableFurnitureObject)
        {
            furniture.GetComponent<FurnitureInfo>().SetSpriterenderColor();
            furniture.gameObject.SetActive(false);
            furnitureObjectQueue.Enqueue(furniture);
        }

        // 새 층으로 변경
        currentFloor = floorNumber;
        FurnitureData[] furnitureInfos= floorData[currentFloor].furnitureInfos;

        foreach (FurnitureData data in furnitureInfos)
        {
            if (data.isUnlocked)
            {
                if (furnitureObjectQueue.Count == 0)
                {
                    GameObject newObj = Object.Instantiate(furniturePrefab, furnitureParent);
                    newObj.GetComponent<FurnitureInfo>().SettingData(data);
                    newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset;
                    newObj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset2;
                    availableFurnitureObject.Add(newObj);

                }
                else
                {
                    GameObject obj = furnitureObjectQueue.Dequeue();
                    obj.SetActive(true);
                    obj.GetComponent<FurnitureInfo>().SettingData(data);
                    obj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset;
                    obj.GetComponent<IsoSpriteSorting>().SorterPositionOffset = data.SorterPositionOffset2;
                    availableFurnitureObject.Add(obj);
                }
            }
        }
        // UI 업데이트 (필요하다면)
        UpdateFloorUI();
    }

    // 가구 추가
    public void AddFurnitureToFloor(GameObject furniture, int floor)
    {
        if (floor >= 0 && floor < furnitureByFloor.Count)
        {
            furnitureByFloor[floor].Add(furniture);

            // 현재 층이 아니면 비활성화
            if (floor != currentFloor)
            {
                furniture.SetActive(false);
            }

            // 가구의 레이어 설정 (층에 따라)
            furniture.layer = 8 + floor; // 8부터 시작 가정
        }
    }

    // UI 업데이트 (층 표시)
    private void UpdateFloorUI()
    {
        // 층 표시 UI 업데이트 구현
        // 예: 현재 층 번호를 텍스트로 표시
    }
}