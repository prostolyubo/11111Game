using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    // MAX 11111 = 31
    public static readonly int[] MAX_VALUES = { 3, 7, 15, 31 };

    public static int level = 0;

    public TextMesh text;

    public Vector2Int gridPosition;

    public int decimalValue;
    public string binaryValue;
    public Transform cachedTransform;

    public Animation animation;

    public static string DecimalToBinary(int decimalInput)
    {
        string stringified = System.Convert.ToString(decimalInput, 2);
        return stringified.PadLeft(level + 2, '0');
    }

    public void Set(int startingValue)
    {
        decimalValue = startingValue;
        Display();
        animation.Rewind();
        animation.Play();
    }

    public void Display()
    {
        binaryValue = DecimalToBinary(decimalValue);
        text.text = binaryValue;
    }

    [ContextMenu("TestSet")]
    public void SetRandom()
    {
        Set(Random.Range(1, MAX_VALUES[level]/2));
    }

    public void Appear()
    {
        StopAllCoroutines();
        StartCoroutine(AppearingRoutine());
    }

    public IEnumerator AppearingRoutine()
    {
        float p = 0;
        cachedTransform.localScale = Vector3.zero;

        while (p < 0.4f)
        {
            yield return null;
            cachedTransform.localScale = Vector3.one * p / .4f;
            p += Time.deltaTime;
        }

        cachedTransform.localScale = Vector3.one;
    }
}
