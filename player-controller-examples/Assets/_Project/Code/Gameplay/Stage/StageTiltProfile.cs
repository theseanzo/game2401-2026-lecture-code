using UnityEngine;

namespace _Project.Code.Gameplay.Stage
{
    [CreateAssetMenu(fileName = "StageTiltProfile", menuName = "ScriptableObjects/Stage/Stage Tilt Profile")]
    public class StageTiltProfile : ScriptableObject
    {
        [field: SerializeField, Range(0f, 45f), Tooltip("Largest tilt the stage will reach at full stick, in degrees")]
        public float MaxTiltAngle { get; private set; } = 20f;

        [field: SerializeField, Tooltip("How fast the stage eases toward the target tilt, in degrees per second")]
        public float TiltSpeed { get; private set; } = 90f;
    }
}
