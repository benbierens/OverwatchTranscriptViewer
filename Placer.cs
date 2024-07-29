using Godot;
using System;

namespace OverwatchTranscriptViewer
{
    public class Placer
    {
        private const float Radius = 2.0f;
        private int number = 10;
        private int count = 0;

        public Vector3 GetPlace()
        {
            count++;
            float f = count;
            double n = number;
            float t = f * Convert.ToSingle(Math.PI * 2.0 / n);

            return new Vector3(
                Mathf.Sin(t) * Radius,
                Mathf.Cos(t) * Radius,
                0.0f
            );
        }

        public void SetMaxPlaces(int max)
        {
            number = max;
        }
    }
}
