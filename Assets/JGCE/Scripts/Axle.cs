using System;
using UnityEngine;

namespace JGCE.Scripts
{
    [Serializable]
    public class Axle
    {
        public string Name;
        public WheelCollider LeftWheel;
        public WheelCollider RightWheel;
        public bool Steering = false;
        public bool Motor = true;
    }
}