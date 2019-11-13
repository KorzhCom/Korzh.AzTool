using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Korzh.AzTool
{

    public class ConnectionListItem
    {
        public string ConnectionId { get; set; }

        public string ConnectionString { get; set; }

        public ConnectionListItem(string connectionId, string connectionString)
        {
            ConnectionId = connectionId;
            ConnectionString = connectionString;
        }
    }

    public class ConnectionStorage
    {

        private readonly string _configFile;

        private readonly Dictionary<string, string> _connections;

        public ConnectionStorage(string configFile)
        {
            _configFile = configFile;


            if (File.Exists(_configFile))
            {
                var config = JObject.Parse(File.ReadAllText(_configFile));
                _connections = config["connections"].ToObject<Dictionary<string, string>>();
            }
            else
            {
                _connections = new Dictionary<string, string>();
            }

        }

        public string Get(string id)
        {
            if (_connections.TryGetValue(id, out var connectionString)) {
                return connectionString;
            }

            return null;
        }

        public void Add(string id, string connectionString)
        {
            _connections[id] = connectionString;
        }

        public void Remove(string id)
        {
            _connections.Remove(id);
        }

        public List<ConnectionListItem> List()
        {
            return _connections.Select(c => new ConnectionListItem(c.Key, c.Value)).ToList();
        }

        public void SaveChanges()
        {
            JObject config;
            if (File.Exists(_configFile))
            {
                config = JObject.Parse(File.ReadAllText(_configFile));
            }
            else
            {
                config = new JObject();
                Directory.CreateDirectory(Path.GetDirectoryName(_configFile));
            }

            config["connections"] = JObject.FromObject(_connections);
            File.WriteAllText(_configFile, config.ToString());
        }

    }
}
