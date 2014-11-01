using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class Helper
{
    public static IEnumerator WaitThenCallback(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }

}

