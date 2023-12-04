using UnityEngine;

namespace LostInLeaves.Components
{
    [RequireComponent(typeof(Light))]
    public class FlickeringLight : MonoBehaviour
    {
        private Light _pointLight;

        [SerializeField, Tooltip("Minimum light intensity")]
        private float _minIntensity = 0.5f;

        [SerializeField, Tooltip("Maximum light intensity")]
        private float _maxIntensity = 1.5f;

        [SerializeField, Tooltip("How fast the light changes intensity")]
        private float _flickerSpeed = 0.1f;

        private void Awake()
        {
            _pointLight = GetComponent<Light>();
        }

        private void Update()
        {
            // Randomly change the light intensity to simulate flickering
            float intensity = Mathf.Lerp(_minIntensity, _maxIntensity, Mathf.PingPong(Time.time * _flickerSpeed, 1));
            _pointLight.intensity = intensity;
        }
    }
}
