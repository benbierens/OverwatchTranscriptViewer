using Godot;
using OverwatchTranscript;

namespace OverwatchTranscriptViewer
{
    public partial class Player : Node
    {
        private ITranscriptReader reader;
        private bool hold = false;
        private bool playing = false;

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

        public void Proceed()
        {
            hold = false;
        }

        public override void _Process(double delta)
        {
            if (!playing) return;
            if (hold) return;

            hold = true;
            var current = reader.Next();

            if (current != null)
            {
                //GuiController.Instance.UpdateProgressBar(header.EarliestUct, header.LatestUtc, current.Value);
            }
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
