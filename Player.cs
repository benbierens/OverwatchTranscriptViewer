using Godot;
using OverwatchTranscript;
using System;

namespace OverwatchTranscriptViewer
{
    public partial class Player : Node
    {
        private ITranscriptReader reader;
        private readonly object countLock = new object();
        private int animCount = 0;
        private bool playing = false;
        private float speed = 1.0f;
        private double timeLeft = 0.0;

        private bool stepped = false;

        public override void _Ready()
        {
            SceneController.Instance.Player = this;
        }

        public void Initialize(ITranscriptReader reader)
        {
            this.reader = reader;

            reader.AddMomentHandler(HandleMoment);
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
            if (AnimationConfig.WaitTillFinished && animCount > 0) return;
            StepOneMoment();
            stepped = true;
        }

        public void SetSpeed(float speed)
        {
            this.speed = speed;
        }

        public void AnimationBegin()
        {
            lock (countLock)
            {
                animCount++;
            }
        }

        public void AnimationFinished()
        {
            lock (countLock)
            {
                animCount--;
            }
        }

        public override void _Process(double delta)
        {
            if (!playing) return;

            if (timeLeft > 0.0)
            {
                timeLeft -= delta * speed;
                if (timeLeft > 0.0) return;
            }

            if (AnimationConfig.WaitTillFinished && animCount > 0)
            {
                EventsPanelController.Instance.WaitingForAnim();
                return;
            }

            StepOneMoment();
        }

        public TimeSpan? ApplySpeedToDuration(TimeSpan? duration)
        {
            if (duration == null) return null;
            return TimeSpan.FromSeconds(ApplySpeedToDuration(duration.Value.TotalSeconds));
        }

        public double ApplySpeedToDuration(double duration)
        {
            return Math.Min(duration * (1.0 / speed), AnimationConfig.MaxMomentDuration);
        }

        private void HandleMoment(ActivateMoment moment)
        {
            if (!moment.Duration.HasValue)
            {
                Stop();
                GD.Print("Playback finished.");
                return;
            }
            timeLeft = Math.Min(moment.Duration.Value.TotalSeconds, AnimationConfig.MaxMomentDuration);
        }

        private void StepOneMoment()
        {
            reader.Next();
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
