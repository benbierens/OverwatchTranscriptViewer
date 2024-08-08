using Godot;
using System;

namespace OverwatchTranscriptViewer
{
    public class Placer
    {
        private float radius = 2.0f;
        private int number = 10;
        private int count = 0;

        public Vector3 GetPlace()
        {
            count++;
            float f = count;
            double n = number;
            float t = f * Convert.ToSingle(Math.PI * 2.0 / n);

            return new Vector3(
                Mathf.Sin(t) * radius,
                Mathf.Cos(t) * radius,
                0.0f
            );
        }

        public void SetMaxPlaces(int max)
        {
            number = max;

            float c = max;
            float m = 50.0f;
            var w = Math.Clamp(c / m, 0.0f, 1.0f);
            radius = 1.0f + Mathf.Lerp(0.0f, 5.0f, w);
        }
    }
}
