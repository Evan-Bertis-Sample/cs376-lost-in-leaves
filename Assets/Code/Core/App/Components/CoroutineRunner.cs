using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlyCore.CurlyApp
{
    public class CoroutineRunner : MonoBehaviour
    {
        public Coroutine StartGlobalCoroutine(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }
    }
}

