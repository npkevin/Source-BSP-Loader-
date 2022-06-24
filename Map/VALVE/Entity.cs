using System.Collections.Generic;

namespace VALVE
{
    class Entity : List<KeyValuePair<string, string>>
    {
        public string this[string key]
        {
            get
            {
                KeyValuePair<string, string> query = this.Find(kvp => kvp.Key == key);
                if (query.Equals(default(KeyValuePair<string, string>)))
                    return null;
                return query.Value;
            }
        }

        public void AddParam(string key, string value)
        {
            this.Add(new KeyValuePair<string, string>(key, value));
        }

        // return true on first case of key
        public bool Contains(string key)
        {
            foreach (var Pair in this)
                if (Pair.Key == key)
                    return true;
            return false;
        }

        // returns true on first case of (key, value)
        public bool Contains(string key, string value)
        {
            foreach (var Pair in this)
                if (Pair.Key == key && Pair.Value == value)
                    return true;
            return false;
        }

        public override string ToString()
        {
            string str = "classname: " + this["classname"] + '\n';
            foreach (var pair in this)
                if (pair.Key != "classname")
                    str += pair.Key + ": " + pair.Value + '\n';
            return str;
        }
    }
}
