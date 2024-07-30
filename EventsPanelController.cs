using Godot;

namespace OverwatchTranscriptViewer
{
    public partial class EventsPanelController : Node
    {
        private Panel panel;
        private bool visible = false;

        public override void _Ready()
        {
            panel = GetNode<Panel>("EventsPanel");
            panel.Visible = false;

            var list = GetNode<ItemList>("EventsPanel/ItemList");
            for (var i = 0; i < 1000; i++)
            {
                var item = list.AddItem("Item: " + i);
                list.SetItemDisabled(item, true);
            }
        }

        public void Toggle()
        {
            visible = !visible;
            panel.Visible = visible;
        }
    }
}
