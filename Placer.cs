using CodexPlugin.OverwatchSupport;
using Godot;
using OverwatchTranscript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace OverwatchTranscriptViewer
{
    public partial class Placer : Node
    {
        private List<ControlledNode> nodes = new List<ControlledNode>();
        private bool kademliaPositions = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            SceneController.Instance.Placer = this;
            SceneController.Instance.LineOptionsChanged += OnLineOptionsChanged;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            foreach (var node in nodes) node.Process(delta);
        }

        public bool PlacesUpdating => nodes.Any(n => n.Updating);

        public void Control(Node3D node, float kPos)
        {
            if (kademliaPositions)
            {
                AddNodeAtKademliaPos(node, kPos);
            }
            else
            {
                AddNodeToCircle(node, kPos);
            }
        }

        private void OnLineOptionsChanged(LineOptionsEvent @event)
        {
            if (@event.KademliaPositions)
            {
                SetKademliaPositions();
            }
            else
            {
                SetCirclePositions();
            }
        }

        private void SetCirclePositions()
        {
            if (!kademliaPositions) return;
            kademliaPositions = false;

            UpdateCircleNodeTargetPositions();
        }

        private void SetKademliaPositions()
        {
            if (kademliaPositions) return;
            kademliaPositions = true;

            UpdateKademliaNodeTargetPositions();
        }

        private void AddNodeAtKademliaPos(Node3D node, float kPos)
        {
            StartAtKademliaPos(node, kPos);
            UpdateKademliaNodeTargetPositions();
        }

        private void AddNodeToCircle(Node3D node, float kPos)
        {
            StartOnCicle(node, kPos);
            UpdateCircleNodeTargetPositions();
        }

        private void StartOnCicle(Node3D node, float kPos)
        {
            StartNode(node, kPos, GetCirclePosition(nodes.Count, nodes.Count + 1));
        }

        private void StartAtKademliaPos(Node3D node, float kPos)
        {
            StartNode(node, kPos, GetKademliaPosition(kPos, nodes.Count + 1));
        }

        private void StartNode(Node3D node, float kPos, Vector3 startPos)
        {
            var n = new ControlledNode(node, kPos, startPos);
            nodes.Add(n);
        }

        private void UpdateCircleNodeTargetPositions()
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                nodes[i].MoveTo(GetCirclePosition(i, nodes.Count));
            }
        }

        private void UpdateKademliaNodeTargetPositions()
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                nodes[i].MoveTo(GetKademliaPosition(nodes[i].KPos, nodes.Count));
            }
        }

        private Vector3 GetCirclePosition(int index, int max)
        {
            float n = index;
            double maxN = max;
            float radians = n * Convert.ToSingle(Math.PI * 2.0 / maxN);
            var radius = GetRadius(max);

            return new Vector3(
                Mathf.Sin(radians) * radius,
                Mathf.Cos(radians) * radius,
                0.0f
            );
        }

        private Vector3 GetKademliaPosition(float kPos, int max)
        {
            float radians = Convert.ToSingle(Math.PI * (kPos - 0.5f));
            var radius = GetRadius(max);

            return new Vector3(
                Mathf.Sin(radians) * radius,
                Mathf.Cos(radians) * radius,
                0.0f
            );
        }

        private float GetRadius(int max)
        {
            float c = max;
            float m = 50.0f;
            var w = Math.Clamp(c / m, 0.0f, 1.0f);
            var result = 1.0f + Mathf.Lerp(0.0f, 5.0f, w);
            if (kademliaPositions) result *= 2.0f;
            return result;
        }

        public class ControlledNode
        {
            private readonly Node3D node;
            private Vector3 target;
            public bool Updating { get; private set; } = false;
            public float KPos { get; }

            public ControlledNode(Node3D node, float kPos, Vector3 position)
            {
                this.node = node;
                KPos = kPos;
                target = position;
                Updating = false;

                SetPosition(target);
            }

            public void MoveTo(Vector3 position)
            {
                target = position;
                Updating = true;
            }

            public void Process(double delta)
            {
                if (!Updating) return;

                var newPos = node.Transform.Origin.Lerp(target, 1.5f * Convert.ToSingle(delta));
                SetPosition(newPos);

                if ((newPos - target).Length() < 0.1f)
                {
                    Updating = false;
                }
            }

            private void SetPosition(Vector3 pos)
            {
                node.Transform = new Transform3D
                {
                    Origin = pos,
                    Basis = node.Transform.Basis
                };
            }
        }
    }
}
