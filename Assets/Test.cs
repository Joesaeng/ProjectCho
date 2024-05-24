using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        ulong i = 11;
        while (true)
        {
            Debug.Log(i);
            Debug.Log(Util.SummaryOfNumbers(i));
            i += 17;

            i *= 10;

            if (i > ulong.MaxValue / 10)
                break;
        }
    }
}
