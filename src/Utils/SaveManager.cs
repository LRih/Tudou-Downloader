using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TudouDownloader
{
    public static class SaveManager
    {
        //===================================================================== VARIABLES
        private static readonly string SAVE_PATH = Application.StartupPath + Path.DirectorySeparatorChar + "save.json";

        private const int MAX_HISTORY = 15;
        private const int MAX_SPEED_LIMIT = 99999;

        private static readonly object _lock = new object();

        //===================================================================== FUNCTIONS
        private static dynamic Load()
        {
            dynamic json = new JObject();

            if (File.Exists(SAVE_PATH))
                json = JObject.Parse(File.ReadAllText(SAVE_PATH));

            // fill missing properties
            if (json.Property("speed_limit") == null)
            {
                json.Add(new JProperty("speed_limit", null));
                Save(json);
            }

            if (json.Property("soku_priorities") == null)
            {
                json.Add(new JProperty("soku_priorities", new JArray("youku", "tudou", "letv", "qq", "56", "fengxing", "cntv")));
                Save(json);
            }

            if (json.Property("history") == null)
            {
                json.Add(new JProperty("history", new JArray()));
                Save(json);
            }

            return json;
        }

        public static List<HistoryItem> LoadHistory()
        {
            List<HistoryItem> history = new List<HistoryItem>();

            foreach (var item in Load().history)
                history.Add(new HistoryItem((string)item.name, (string)item.url));

            return history;
        }
        public static string[] LoadSokuPriorities()
        {
            List<string> priorities = new List<string>();

            foreach (var priority in Load().soku_priorities)
                priorities.Add((string)priority);

            return priorities.ToArray();
        }
        public static int LoadSpeedLimit()
        {
            var speedLimit = Load().speed_limit;

            if (speedLimit == null)
                return MAX_SPEED_LIMIT;

            return Math.Min(Math.Max((int)speedLimit, 1), MAX_SPEED_LIMIT);
        }

        public static void AddHistoryItem(string url)
        {
            AddHistoryItem(null, url);
        }
        public static void AddHistoryItem(string name, string url)
        {
            dynamic json = Load();

            // get default name if empty
            if (name == null)
                name = GetHistoryUrlName(json.history, url);

            RemoveHistoryItem(json.history, url); // remove duplicate item
            json.history.AddFirst(JObject.FromObject(new { name = name, url = url }));

            // only store MAX_HISTORY items in history
            if (json.history.Count > MAX_HISTORY)
                json.history.RemoveAt(MAX_HISTORY);

            Save(json);
        }

        private static void RemoveHistoryItem(dynamic history, string url)
        {
            for (int i = history.Count - 1; i >= 0; i--)
                if (history[i].url == url)
                    history.RemoveAt(i);
        }

        private static void Save(dynamic json)
        {
            lock (_lock)
                File.WriteAllText(SAVE_PATH, json.ToString());
        }


        private static string GetHistoryUrlName(dynamic history, string url)
        {
            foreach (var item in history)
                if (url == (string)item.url)
                    return item.name;

            return Strings.UNKNOWN;
        }
    }
}
