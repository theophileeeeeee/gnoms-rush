using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelPathLine : MonoBehaviour
{
    private static readonly Color COLOR_NORMAL   = new Color(130f/255f, 130f/255f, 130f/255f, 1f);
    private static readonly Color COLOR_SELECTED = new Color(255f/255f, 255f/255f, 255f/255f, 1f);

    public List<GameObject> waypoints = new List<GameObject>();
    public Text levelInfoText;
    public Color lineColor = Color.white;
    public float lineWidth = 10f;
    public RectTransform lineContainer;

    [Range(0, 100)]
    public int unlockedWaypointCount = 0;

    private int selectedLevel = 1;
    private Canvas rootCanvas;
    private List<GameObject> lineSegments = new List<GameObject>();

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        while (rootCanvas.transform.parent != null && rootCanvas.transform.parent.GetComponent<Canvas>() != null)
            rootCanvas = rootCanvas.transform.parent.GetComponent<Canvas>();

        InitWaypoints();
    }

    void Start()
    {
        selectedLevel = PlayerPrefs.GetInt("LastSelectedLevel", 1);
        Select(selectedLevel);
        DrawLine();
    }

    private void InitWaypoints()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Image img = waypoints[i].GetComponent<Image>();
            if (img != null)
                img.color = COLOR_NORMAL;

            Button btn = waypoints[i].GetComponent<Button>();
            if (btn != null)
                btn.interactable = (i == 0 || i < unlockedWaypointCount);
        }
    }

    public void OnWaypointClicked(int number)
    {
        selectedLevel = number;
        Select(number);
    }

    public void LaunchSelectedLevel()
    {
        SceneManager.LoadScene("Level" + selectedLevel);
        Debug.Log("Loading scene: Level" + selectedLevel);
    }

    public void Select(int number)
    {
        int index = number - 1;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Image img = waypoints[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == index) ? COLOR_SELECTED : COLOR_NORMAL;
        }

        if (levelInfoText != null)
            levelInfoText.text = "JOUER :\nNIVEAU " + number.ToString("D2");

        PlayerPrefs.SetInt("LastSelectedLevel", number);
        PlayerPrefs.Save();
    }

    public void RevealUpTo(int waypointIndex)
    {
        unlockedWaypointCount = Mathf.Clamp(waypointIndex + 1, 0, waypoints.Count);

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Button btn = waypoints[i].GetComponent<Button>();
            if (btn != null)
                btn.interactable = (i <= waypointIndex);
        }

        DrawLine();
    }

    public void RevealAll()
    {
        unlockedWaypointCount = 0;

        foreach (var wp in waypoints)
        {
            if (wp == null) continue;

            Button btn = wp.GetComponent<Button>();
            if (btn != null)
                btn.interactable = true;
        }

        DrawLine();
    }

    private Vector2 GetScreenPos(RectTransform canvasRect, GameObject wp)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, wp.GetComponent<RectTransform>().position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
        return local;
    }

    private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    private void DrawLine()
    {
        foreach (var seg in lineSegments)
            Destroy(seg);
        lineSegments.Clear();

        int limit = (unlockedWaypointCount > 0)
            ? Mathf.Min(unlockedWaypointCount, waypoints.Count)
            : waypoints.Count;

        if (limit < 2) return;

        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

        List<Vector2> pts = new List<Vector2>();
        for (int i = 0; i < limit; i++)
            pts.Add(GetScreenPos(canvasRect, waypoints[i]));

        // points fantomes pour que la courbe commence et finisse bien
        pts.Insert(0, pts[0] + (pts[0] - pts[1]));
        pts.Add(pts[pts.Count - 1] + (pts[pts.Count - 1] - pts[pts.Count - 2]));

        List<Vector2> curvePoints = new List<Vector2>();
        int steps = 20;

        for (int p = 1; p < pts.Count - 2; p++)
        {
            for (int s = 0; s <= steps; s++)
            {
                float t = s / (float)steps;
                curvePoints.Add(CatmullRom(pts[p - 1], pts[p], pts[p + 1], pts[p + 2], t));
            }
        }

        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            Vector2 posA = curvePoints[i];
            Vector2 posB = curvePoints[i + 1];
            Vector2 dir = posB - posA;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            GameObject seg = new GameObject("Seg_" + i);
            seg.transform.SetParent(rootCanvas.transform, false);

            RectTransform rect = seg.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = (posA + posB) / 2f;
            rect.sizeDelta = new Vector2(distance + 1f, lineWidth);
            rect.localRotation = Quaternion.Euler(0, 0, angle);

            Image img = seg.AddComponent<Image>();
            img.color = lineColor;

            seg.transform.SetParent(lineContainer, true);
            lineSegments.Add(seg);
        }
    }
}