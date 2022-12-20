using UnityEngine;

namespace JGCE.Scripts
{
    [CreateAssetMenu(menuName = "JGCE/Vehicle Settings")]
    public class VehicleSettings : ScriptableObject
    {
        public int FuelCapacity = 200;
        public int StartFuel = 150;
        public float FuelConsumptionRate = 1f;
        public AnimationCurve FuelConsumptionCurve;

        public float MaxSteerAngle = 30f;
        public float TorqueMultiplier = 1000f;
        public float BrakeTorque = 2000f;
    }
}