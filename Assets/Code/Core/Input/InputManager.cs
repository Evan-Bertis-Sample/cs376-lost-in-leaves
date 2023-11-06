using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using CurlyCore.CurlyApp;

namespace CurlyCore.Input
{
    [CreateAssetMenu(fileName = "InputManager", menuName = "Curly/Core/Input Manager")]
    public class InputManager : RuntimeScriptableObject
    {
        [SerializeField] public InputActionAsset MasterActionAsset;
        [InputPath][SerializeField] private string _joinButton;
        [InputPath][SerializeField] private string _exitButton;
        public bool AllowPlayersToJoin = false;

        [SerializeField] private List<DeviceInputHandler> _devices = new List<DeviceInputHandler>();
        [SerializeField] private List<DeviceInputHandler> _playerDevices = new List<DeviceInputHandler>();

        public override void OnBoot(App app, Scene scene)
        {
            _devices = new List<DeviceInputHandler>();
            _playerDevices = new List<DeviceInputHandler>();
            foreach (InputDevice device in InputSystem.devices)
            {
                JoinDeviceSet(device);
            }

            InputSystem.onDeviceChange += HandleDeviceChange;
        }
        private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    JoinDeviceSet(device);
                    break;
                default:
                    break;
            }
        }
        public void JoinDeviceSet(params InputDevice[] devices)
        {
            Debug.Log("Joining Device");
            InputActionAsset assetClone = Instantiate(MasterActionAsset);
            DeviceInputHandler handler = new DeviceInputHandler(assetClone, devices);
            _devices.Add(handler);

            ParsePath(_joinButton, out string joinMapName, out string joinActionName);
            InputAction joinAction = handler.InputActions.FindActionMap(joinMapName)?.FindAction(joinActionName);
            joinAction.performed += x => JoinPlayer(handler);
        }
        public void JoinPlayer(DeviceInputHandler handler)
        {
            if (!AllowPlayersToJoin) return;

            if (_playerDevices.Contains(handler)) return;

            Debug.Log("Joining Player");
            _playerDevices.Add(handler);

            ParsePath(_exitButton, out string disconnectMap, out string disconnectActionName);
            InputAction disconnectAction = handler.InputActions.FindActionMap(disconnectMap)?.FindAction(disconnectActionName);
            disconnectAction.performed += x => DisconnectPlayer(handler);
        }

        public void DisconnectPlayer(DeviceInputHandler player)
        {
            Debug.Log("Removing Player");
            _playerDevices.Remove(player);
        }

        public InputAction.CallbackContext GetContext(string path, int player = 0)
        {
            if (player >= _playerDevices.Count) return default;
            if (_playerDevices[player] == null) return default;

            return _playerDevices[player].GetContextByPath(path);
        }

        public ActionStatus GetActionStatus(string path, int player)
        {
            if (player >= _playerDevices.Count) return default;
            if (_playerDevices[player] == null) return default;

            return _playerDevices[player].GetStatusByPath(path);
        }

        public T ReadInput<T>(string path, int player = 0) where T : struct
        {
            if (player >= _playerDevices.Count) return default;
            if (_playerDevices[player] == null) return default;

            return GetContext(path, player).ReadValue<T>();
        }

        public bool IsInputState(string path, ActionStatus.ActionState expectedState, int player = 0)
        {
            if (player >= _playerDevices.Count) return false;
            if (_playerDevices[player] == null) return false;

            return GetActionStatus(path, player).State == expectedState;
        }

        public bool GetInputDown(string path, int player = 0)
        {
            if (player >= _playerDevices.Count) return false;
            if (_playerDevices[player] == null) return false;
            
            var context = GetContext(path, player);
            if (context.action == null) return false;
            
            return context.action.WasPressedThisFrame();
        }

        public bool GetInputUp(string path, int player = 0)
        {
            if (player >= _playerDevices.Count) return false;
            if (_playerDevices[player] == null) return false;

            return GetContext(path, player).action.WasReleasedThisFrame();
        }

        public bool GetButtonInput(string path, int player = 0)
        {
            if (player >= _playerDevices.Count) return false;
            if (_playerDevices[player] == null) return false;

            return GetContext(path, player).action.ReadValue<float>() > 0;
        }
        
        #region Utilities
        private bool IsInputAssigned(string actionName, string mapName)
        {
            InputActionMap map = MasterActionAsset.FindActionMap(mapName);
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
        #endregion
    }

    public class ActionStatus
    {
        public enum ActionState
        {
            NotPerformed,
            Started,
            Performed,
            Canceled
        }

        public InputAction Action { get; private set; }
        public ActionState State { get; private set; }
        public InputAction.CallbackContext Context { get; private set; }
        public Queue<InputAction.CallbackContext> PastContexts;
        public ActionStatus(InputAction action, int queueLength = 15)
        {
            Action = action;
            PastContexts = new Queue<InputAction.CallbackContext>(queueLength);
            State = ActionState.NotPerformed;
            action.started += c => UpdateContext(c, ActionState.Started);
            action.performed += c => UpdateContext(c, ActionState.Performed);
            action.canceled += c => UpdateContext(c, ActionState.Canceled);
        }

        private void UpdateContext(InputAction.CallbackContext newContext, ActionState state)
        {
            PastContexts.Enqueue(newContext);
            Context = newContext;
            State = state;
        }
    }
}

