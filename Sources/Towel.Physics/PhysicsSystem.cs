﻿using System;
using Towel;
using Towel.DataStructures;
using Towel.Mathematics;

namespace Towel.Physics
{
    public class PhysicsSystem<T>
    {
        public static Vector<T> DefaultGravity { get { return new Vector<T>(Constant<T>.Zero, Compute.Divide(Compute.FromInt32<T>(-981), Compute.FromInt32<T>(100)), Constant<T>.Zero); } }

        public Vector<T> _gravity;
        public OmnitreePoints<RigidPhysicsObject<T>, T, T, T> _rigidPhysicsObjects;

        public PhysicsSystem()
        {
            this._gravity = DefaultGravity;
            this._rigidPhysicsObjects = new OmnitreePointsLinked<RigidPhysicsObject<T>, T, T, T>(
                (RigidPhysicsObject<T> value, out T x, out T y, out T z) =>
                {
                    x = value.Shape.Position.X;
                    y = value.Shape.Position.Y;
                    z = value.Shape.Position.Z;
                });
        }

        public Vector<T> Gravity { get { return this._gravity; } set { this._gravity = value; } }

        public OmnitreePoints<RigidPhysicsObject<T>, T, T, T> PhysicsObjects
        {
            get
            {
                return this._rigidPhysicsObjects;
            }
        }

        public void AddBody(RigidPhysicsObject<T> rigidPhysicsObject)
        {
            if (rigidPhysicsObject == null)
            {
                throw new ArgumentNullException(nameof(rigidPhysicsObject));
            }
            this._rigidPhysicsObjects.Add(rigidPhysicsObject);
        }

        public void Run(float timeStep)
        {
            if (timeStep < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeStep), timeStep, "!(" + nameof(timeStep) + " >= 0)");
            }

            if (timeStep == 0.0f)
                return;


        }

        public void ClearObjects()
        {
            this._rigidPhysicsObjects.Clear();
        }
    }
}
