using Godot;
using Godot.Collections;

namespace OverwatchTranscriptViewer
{
    public static class Lookup
    {
        private static readonly Dictionary<string, Node3D> map = new Dictionary<string, Node3D>();

        public static void Add(string id, Node3D obj)
        {
            map.Add(id, obj);
        }

        public static void Remove(string id)
        {
            map.Remove(id);
        }

        public static T Get<T>(string id) where T : Node3D
        {
            return (T)map[id];
        }
    }
}
