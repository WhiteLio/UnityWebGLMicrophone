using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MyHelper
{
    public static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    public static float Remap (this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static T GetRandom<T>(this T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static string ToString<T>(this T[] array)
    {
        return $"[{string.Join(", ", array)}]";
    }

    public static Vector2 RandomVector2(float minInclusive, float maxInclusive)
    {
        return new Vector2(Random.Range(minInclusive, maxInclusive), Random.Range(minInclusive, maxInclusive));
    }
    
    public static Vector3 RandomVector3(float minInclusive, float maxInclusive)
    {
        return new Vector3(Random.Range(minInclusive, maxInclusive), Random.Range(minInclusive, maxInclusive),Random.Range(minInclusive, maxInclusive));
    }

    public static string TimeToString(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    public static Vector2 Round(this Vector2 vector, int decimalPlaces = 0)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return new Vector2(
            Mathf.Round(vector.x * multiplier) / multiplier,
            Mathf.Round(vector.y * multiplier) / multiplier);
    }

    public static Vector2 GridPosition(this Vector2 position, float gridSize = 1f)
    {
        Vector2 gridPos = Vector2.zero;
        float gridHalf = gridSize / 2;
        gridPos.x = Mathf.Round(position.x - gridHalf) + gridHalf;
        gridPos.y = Mathf.Round(position.y - gridHalf) + gridHalf;
        return gridPos;
    }
    
    public static Vector2 GridPosition(this Vector3 position)
    {
        return GridPosition((Vector2) position);
    }
    
    public static List<string> ToStringList(this TextAsset textAsset)
    {
        return textAsset.text.Split("\r\n"[0]).Select(line => line.Trim()).ToList();
    }
    
    public static string ToMeters(this int number)
    {
        return number.ToString("N0") + "m";
    }
}