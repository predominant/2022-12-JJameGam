using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JGCE.Scripts
{
    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    public class Vehicle : MonoBehaviour
    {
        public VehicleSettings Settings;
        public PlayerInput PlayerInput;
        public List<Axle> Axles = new();
        
        private Rigidbody _rigidbody;

        private InputControl _control;
        private InputAction _turnAction, _speedAction, _lookAction, _honkAction, _brakeAction;

        public bool EnableCustomCenterOfMass = false;
        public Vector3 CustomCenterOfMass = Vector3.zero;

        private float _wheelBase = 0f;

        private float WheelBase
        {
            get
            {
                if (this._wheelBase == 0f)
                    this._wheelBase = Vector3.Distance(
                        this.Axles[0].LeftWheel.transform.position,
                        this.Axles[1].LeftWheel.transform.position);

                return this._wheelBase;
            }
        }

        private float _trackWidth = 0f;
        private float TrackWidth
        {
            get
            {
                if (this._trackWidth == 0f)
                    this._trackWidth = Vector3.Distance(
                        this.Axles[0].LeftWheel.transform.position,
                        this.Axles[0].RightWheel.transform.position);

                return this._trackWidth;
            }
        }
        
        private void Awake()
        {
            this._rigidbody = this.GetComponent<Rigidbody>();
            this._turnAction = this.PlayerInput.actions["Turn"];
            this._speedAction = this.PlayerInput.actions["Speed"];
            this._lookAction = this.PlayerInput.actions["Look"];
            this._honkAction = this.PlayerInput.actions["Honk"];
            this._brakeAction = this.PlayerInput.actions["Brake"];

            if (this.EnableCustomCenterOfMass)
                this._rigidbody.centerOfMass = this.CustomCenterOfMass;
        }

        private void Update()
        {
            this.ProcessInput();
            this.UpdateWheelPositions();
        }

        private void OnDrawGizmos()
        {
            if (this.EnableCustomCenterOfMass)
                Gizmos.DrawSphere(this.transform.position + this.CustomCenterOfMass, 0.2f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                this.Axles[0].LeftWheel.transform.position,
                this.Axles[0].LeftWheel.transform.position - this.Axles[0].LeftWheel.transform.GetChild(0).right * 10f);
            Gizmos.DrawLine(
                this.Axles[0].RightWheel.transform.position,
                this.Axles[0].RightWheel.transform.position + this.Axles[0].RightWheel.transform.GetChild(0).right * 10f);
            Gizmos.DrawLine(
                this.Axles[1].LeftWheel.transform.position,
                this.Axles[1].LeftWheel.transform.position - this.Axles[1].LeftWheel.transform.GetChild(0).right * 10f);
            Gizmos.DrawLine(
                this.Axles[1].RightWheel.transform.position,
                this.Axles[1].RightWheel.transform.position + this.Axles[1].RightWheel.transform.GetChild(0).right * 10f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                this.Axles[0].LeftWheel.transform.position,
                this.Axles[0].LeftWheel.transform.position + this.Axles[0].LeftWheel.transform.GetChild(0).right * 10f);
            Gizmos.DrawLine(
                this.Axles[0].RightWheel.transform.position,
                this.Axles[0].RightWheel.transform.position - this.Axles[0].RightWheel.transform.GetChild(0).right * 10f);
            Gizmos.DrawLine(
                this.Axles[1].LeftWheel.transform.position,
                this.Axles[1].LeftWheel.transform.position + this.Axles[1].LeftWheel.transform.GetChild(0).right * 10f);
            Gizmos.DrawLine(
                this.Axles[1].RightWheel.transform.position,
                this.Axles[1].RightWheel.transform.position - this.Axles[1].RightWheel.transform.GetChild(0).right * 10f);
        }

        private void ProcessInput()
        {
            var timeDelta = Time.deltaTime;

            var steeringInput = this._turnAction.ReadValue<float>();
            var steering = steeringInput * this.Settings.MaxSteerAngle;
            
            foreach (var axle in this.Axles)
                this.ApplySteering(axle, steering);

            var speedInput = this._speedAction.ReadValue<float>();
            var speed = speedInput * 1f;
            foreach (var axle in this.Axles)
                this.ApplyAcceleration(axle, speed * this.Settings.TorqueMultiplier);

            var braking = this._brakeAction.IsPressed();
            foreach (var axle in this.Axles)
                this.ApplyBrake(axle, braking);
        }

        private void ApplySteering(Axle a, float s)
        {
            if (!a.Steering)
                return;

            if (s < 0f)
            {
                a.LeftWheel.steerAngle = this.AckermanSteering(s, this.WheelBase, this.TrackWidth);
                a.RightWheel.steerAngle = s;
            }
            else if (s > 0f)
            {
                a.RightWheel.steerAngle = this.AckermanSteering(s, this.WheelBase, this.TrackWidth);
                a.LeftWheel.steerAngle = s;
            }
            else
            {
                a.RightWheel.steerAngle = s;
                a.LeftWheel.steerAngle = s;
            }
        }
        
        public float AckermanSteering(float angleDegrees, float wheelbase, float trackWidth)
        {
            var angleRadians = angleDegrees * Math.PI / 180.0;
            var outsideWheelAngle = Math.Atan2(wheelbase * Math.Tan(angleRadians), trackWidth);
            var outsideWheelAngleDegrees = (float)(outsideWheelAngle * 180.0 / Math.PI);
            return outsideWheelAngleDegrees;
        }

        private void ApplyAcceleration(Axle a, float s)
        {
            if (!a.Motor)
                return;

            a.RightWheel.motorTorque = s;
            a.LeftWheel.motorTorque = s;
        }

        private void ApplyBrake(Axle axle, bool enable)
        {
            var force = enable ? this.Settings.BrakeTorque : 0f;
            axle.RightWheel.brakeTorque = force;
            axle.LeftWheel.brakeTorque = force;
        }

        private void UpdateWheelPositions()
        {
            foreach (var axle in this.Axles)
            {
                if (axle.LeftWheel != null)
                    this.UpdateWheelPosition(axle.LeftWheel);
                if (axle.RightWheel != null)
                    this.UpdateWheelPosition(axle.RightWheel);
            }
        }

        private void UpdateWheelPosition(WheelCollider c)
        {
            Quaternion q;
            Vector3 p;
            c.GetWorldPose(out p, out q);
            var shapeTransform = c.transform.GetChild(0);
            shapeTransform.position = p;
            shapeTransform.rotation = q;
        }
    }
}