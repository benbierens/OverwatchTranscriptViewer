using Godot;
using OverwatchTranscript;
using System;

namespace OverwatchTranscriptViewer
{
    public partial class Player : Node
    {
        private ITranscriptReader reader;
        private bool hold = false;
        private bool playing = false;
        private float speed = 1.0f;
        private double timeLeft = 0.0;

        public override void _Ready()
        {
            SceneController.Instance.Player = this;
        }

        public void Initialize(ITranscriptReader reader)
        {
            this.reader = reader;
        }

        public void Play()
        {
            playing = true;
        }

        public void Stop()
        {
            playing = false;
        }

        public void StepOne()
        {
            if (playing) throw new Exception("already playing");
            StepOneMoment();
        }

        public void SetSpeed(float speed)
        {
            this.speed = speed;
        }

        public void Proceed()
        {
            hold = false;
        }

        public override void _Process(double delta)
        {
            if (!playing) return;
            if (timeLeft > 0.0)
            {
                timeLeft -= delta * speed;
                if (timeLeft > 0.0) return;
            }

            hold = true;
            var duration = StepOneMoment();
            if (duration != null)
            {
                timeLeft += duration.Value.TotalSeconds;
                EventsPanelController.Instance.SetCurrentEventDuration(timeLeft);
            }
        }

        private TimeSpan? StepOneMoment()
        {
            var current = reader.Next();
            var duration = reader.GetDuration();

            if (current != null)
            {
                GuiController.Instance.UpdateProgressBar(current.Value.Item1, current.Value.Item2);
            }

            return duration;
        }

        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest)
            {
                SceneController.Instance.Quit();
            }
        }
    }
}
