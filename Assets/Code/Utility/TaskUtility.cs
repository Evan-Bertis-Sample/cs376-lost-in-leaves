using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace CurlyUtility
{
    public class TaskUtility
    {
        public static async Task WaitUntil(Func<bool> condition)
        {
            while (condition() == false) await Task.Yield();
        }

        public static IEnumerator TaskAsCoroutine(Task task)
        {
            while (!task.IsCompleted) yield return null;
        }
    }
}