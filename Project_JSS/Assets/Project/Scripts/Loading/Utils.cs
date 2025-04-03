using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneNames
{
    Logo=0,
    Login,
    Main
}
public static class Utils 
{
    public static string GetActiveScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    public static void LoadScene(string sceneName)
    {
        if(sceneName == "")
        {
            SceneManager.LoadScene(GetActiveScene());
        }
        else
        {
            SceneManager.LoadScene(sceneName);

        }
    }
    public static void LoadScene(SceneNames sceneName)
    {
         SceneManager.LoadScene(sceneName.ToString());
    }
    /// <summary>
    /// Degree 값을 Radian 값으로 변환
    /// 1도는 "PI/180" radian
    /// angle도는 "PI/180 * angle"radian
    /// </summary>
    public static float DegreeToRadian(float angle)
    {
        return Mathf.PI * angle / 180;
    }

    public static Vector2 GetNewPoint(Vector3 start, float angle, float r)
    {
        // Degree 각도 값을 Radian으로 변경
        angle = DegreeToRadian(angle);

        // 원점을 기준으로 x, y 좌표를 구하기 때문에 시작지점 좌표(start)를 더해준다
        Vector2 position = Vector2.zero;
        position.x = Mathf.Cos(angle) * r + start.x;
        position.y = Mathf.Sin(angle) * r + start.y;

        return position;
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }

    public static Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p1 = Lerp(a, b, t);
        Vector2 p2 = Lerp(b, c, t);

        return Lerp(p1, p2, t);
    }
}
