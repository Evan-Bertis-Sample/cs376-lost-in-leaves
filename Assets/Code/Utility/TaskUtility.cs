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

        public class WaitForTask : CustomYieldInstruction
        {
            private Task _task;

            public WaitForTask(Task task)
            {
                _task = task;
            }

            public override bool keepWaiting
            {
                get
                {
                    if (_task.IsCompleted)
                    {
                        // check for errors, and debug message them
                        if (_task.IsFaulted)
                        {
                            Debug.LogException(_task.Exception);
                        }
                        if (_task.IsCanceled)
                        {
                            Debug.LogWarning("Task was cancelled");
                        }
                        return false;
                    }

                    return true;
                }
            }
        }
    }
}