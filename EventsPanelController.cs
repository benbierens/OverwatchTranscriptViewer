using Godot;
using OverwatchTranscript;
using System;

namespace OverwatchTranscriptViewer
{
	public partial class EventsPanelController : Node
	{
		public static EventsPanelController Instance;

		private Panel panel;
		private bool visible = false;
		private ItemList list;

		public override void _Ready()
		{
			Instance = this;

			panel = GetNode<Panel>("EventsPanel");
			list = GetNode<ItemList>("EventsPanel/ItemList");

			panel.Visible = false;

			for (var i = 0; i < 1000; i++)
			{
				var item = list.AddItem("Item: " + i + "\n multiline!");
				list.SetItemDisabled(item, true);
			}
		}

		public void Initialize(ITranscriptReader reader)
		{
			DeleteAll();

			reader.PreviewEvents(PreviewEvent);
		}

		public void _on_item_list_item_clicked(long index, Vector2 at_position, long mouse_button_index)
		{
		}

		public void Toggle()
		{
			visible = !visible;
			panel.Visible = visible;
		}

		private bool PreviewEvent(OverwatchEvent e, DateTime utc)
		{
			return true;
		}

		private void DeleteAll()
		{
		}


	}
}

