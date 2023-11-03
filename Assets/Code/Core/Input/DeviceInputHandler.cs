using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

namespace CurlyCore.Input
{
    [System.Serializable]
    public class DeviceInputHandler
    {
        public InputActionAsset InputActions { get; private set; }
        [field: SerializeField] public InputDevice InputDevice { get; private set; }
        private Dictionary<string, List<ActionStatus>> _statusesByMap = new Dictionary<string, List<ActionStatus>>();

        public DeviceInputHandler(InputActionAsset asset, params InputDevice[] devices)
        {
            InputActions = asset;
            InputActions.Enable();

            InputActions.devices = new UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>(devices);
            _statusesByMap = new Dictionary<string, List<ActionStatus>>();
            CreateStatusDictionary(InputActions);
        }
        
        private void CreateStatusDictionary(InputActionAsset actionAsset)
        {
            _statusesByMap = new Dictionary<string, List<ActionStatus>>();

            foreach (InputActionMap map in actionAsset.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    ActionStatus status = new ActionStatus(action);
                    _statusesByMap.TryGetValue(map.name, out List<ActionStatus> statuses);

                    if (statuses == null)
                    {
                        statuses = new List<ActionStatus>();
                        statuses.Add(status);
                        _statusesByMap.Add(map.name, statuses);
                        continue;
                    }

                    statuses.Add(status);
                }
            }
        }

        private InputAction.CallbackContext GetContext(string actionName, string mapName = "Player")
        {
            ActionStatus status = GetStatus(actionName, mapName);
            return status.Context;
        }

        public InputAction.CallbackContext GetContextByPath(string path)
        {
            ParsePath(path, out string mapName, out string actionName);
            return GetContext(actionName, mapName);
        }

        private ActionStatus GetStatus(string actionName, string mapName = "Player")
        {
            _statusesByMap.TryGetValue(mapName, out List<ActionStatus> statuses);
            if (statuses == null) throw new System.Exception($"Action Map: '{mapName}' does not exist in the Input Action Asset.");

            ActionStatus currentStatus = statuses.FirstOrDefault(status => status.Action.name == actionName);
            if (currentStatus == null) throw new System.Exception($"Action : '{actionName}' in Action Map : '{mapName}' does not exist in the Input Action Asset.");

            return currentStatus;
        }
        public ActionStatus GetStatusByPath(string path)
        {
            ParsePath(path, out string mapName, out string actionName);
            return GetStatus(actionName, mapName);
        }

        #region Utilities
        private void ParsePath(string path, out string mapName, out string actionName)
        {
            int pos = path.LastIndexOf("/");
            if (pos == -1)
            {
                mapName = "";
                actionName = "";
                return;
            }

            mapName = path.Substring(0, pos);
            actionName = path.Substring(pos + 1, path.Length - pos - 1);
        }

        private bool IsInputAssigned(string actionName, string mapName)
        {
            InputActionMap map = InputActions.FindActionMap(mapName);
            if (map == null) return false;

            InputAction action = map.FindAction(actionName);
            if (action == null) return false;

            return true;
        }

        public bool IsInputAssigned(string path)
        {
            ParsePath(path, out string mapName, out string actionName);
            return IsInputAssigned(actionName, mapName);
        }
        #endregion
    }

}