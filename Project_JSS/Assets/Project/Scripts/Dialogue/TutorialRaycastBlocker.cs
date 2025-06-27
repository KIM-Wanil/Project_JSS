using UnityEngine;
using UnityEngine.UI;

public class TutorialRaycastBlocker : MonoBehaviour
{
    //public Canvas rootCanvas;
    public Canvas rootCanvas;
    private RectTransform[] blockers = new RectTransform[4];

    void Start()
    {
        if (rootCanvas == null)
            rootCanvas = GetComponent<Canvas>();
        CreateBlockers();
    }

    void Update()
    {
        //UpdateBlockers();
    }

    public void CreateBlockers()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject go = new GameObject("Blocker" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(rootCanvas.transform, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // 완전 투명
            img.raycastTarget = true;
            blockers[i] = go.GetComponent<RectTransform>();
            blockers[i].anchorMin = Vector2.zero;
            blockers[i].anchorMax = Vector2.zero;
            blockers[i].pivot = Vector2.zero;
        }
    }

    public void UpdateBlockers(Vector2 highlightPos, Vector2 hilightSize)
    {
        Vector3[] corners = new Vector3[4];
        //highlightRect.GetLocalCorners(corners);

        // 월드 좌표를 rootCanvas의 로컬 좌표로 변환
        Vector2 min = highlightPos;
        Vector2 max = highlightPos + hilightSize;
        //Debug.Log($"Blocker Update: Min: {min}, Max: {max}");
        float canvasW = rootCanvas.GetComponent<RectTransform>().rect.width;
        float canvasH = rootCanvas.GetComponent<RectTransform>().rect.height;
        Debug.Log(WorldToCanvasLocal(corners[0]) + " " + WorldToCanvasLocal(corners[2]) + " " + canvasW + " " + canvasH);
        // Blocker0: 상단
        SetBlocker(blockers[0], 0, max.y, canvasW, canvasH - max.y);
        // Blocker1: 하단
        SetBlocker(blockers[1], 0, 0, canvasW, min.y);
        // Blocker2: 좌측
        SetBlocker(blockers[2], 0, min.y, min.x, max.y - min.y);
        // Blocker3: 우측
        SetBlocker(blockers[3], max.x, min.y, canvasW - max.x, max.y - min.y);
    }

    // 월드 좌표를 캔버스 로컬 좌표로 변환
    private Vector2 WorldToCanvasLocal(Vector3 world)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            Camera.main.WorldToScreenPoint(world),
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out localPoint);
        return localPoint;
    }

    void SetBlocker(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
}