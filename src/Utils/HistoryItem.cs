using System;
using System.Text.RegularExpressions;

namespace TudouDownloader
{
    public struct HistoryItem
    {
        //===================================================================== VARIABLES
        public string Name { get; }
        public string Url { get; }

        //===================================================================== INITIALIZE
        public HistoryItem(string name, string url)
        {
            Name = name;
            Url = url;
        }

        //===================================================================== FUNCTIONS
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is HistoryItem)
            {
                HistoryItem item = (HistoryItem)obj;
                return Url == item.Url;
            }
            else
                return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
