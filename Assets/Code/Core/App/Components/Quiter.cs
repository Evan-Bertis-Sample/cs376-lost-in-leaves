using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

using CurlyCore.Debugging;

namespace CurlyCore.CurlyApp
{
    public class Quiter : MonoBehaviour
    {
        public event Action OnQuit;

        [GlobalDefault] private GroupLogger _logger;

        private void OnApplicationQuit()
        {
            DependencyInjector.InjectDependencies(this);
            _logger.Log(LoggingGroupID.APP, "Quitting Sequence Started");
            OnQuit?.Invoke();
        }
    }
}
