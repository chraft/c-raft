using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Chraft.PluginSystem.Server;

namespace Chraft.Utils
{
    public class BanSystem : IBanSystem
    {
        private List<Bans> _bans;
        private List<IpBans> _ipbans;
        private List<string> _whitelist;
        private XDocument _bansDoc;
        private const string BanFilePath = "Resources/Bans.xml";
        private const string WhiteList = "Resources/whitelist.txt";

        public void LoadBansAndWhiteList()
        {
            _bans = new List<Bans>();
            _ipbans = new List<IpBans>();
            _whitelist = new List<string>();

            _bansDoc = LoadBans(BanFilePath);
            var bansNode = _bansDoc.Descendants("Bans").ToList();
            if (!bansNode.Any())
            {
                return;
            }

            _bans = (from f in bansNode.Elements("PlayerBan")
                     select new Bans
                         {
                             Duration = DateTime.Parse(f.Element("Duration").Value),
                             PlayerName = f.Element("PlayerName").Value,
                             Reason = f.Element("Reason").Value
                         }).ToList();

            _ipbans = (from f in bansNode.Elements("IpBan")
                       select new IpBans
                       {
                           Ip = f.Element("Ip").Value,
                           Reason = f.Element("Reason").Value
                       }).ToList();

            LoadWhiteList();

        }

        public void LoadWhiteList()
        {
            if (!File.Exists(WhiteList))
            {
                File.Create(WhiteList);

            }
            _whitelist = File.ReadAllLines(WhiteList).ToList();

        }

        public  bool IsOnWhiteList(string player)
        {
            return _whitelist.Contains(player.ToLower());
        }

        public void AddToBanList(string player, string reason = "Banned", string[] tokenDuration = null)
        {
            DateTime dt;
            dt = tokenDuration == null ? new DateTime(1900, 01, 01, 00, 00, 00) : AddTime(tokenDuration);
            AddToBanList(player, dt, reason);
        }

        public void AddToBanList(string player, DateTime tokenDuration, string reason = "Banned")
        {

            if (!HasBan(player))
            {
                _bans.Add(new Bans { PlayerName = player, Reason = reason, Duration = tokenDuration });
            }
            SaveBans();
        }
        public void AddToIpBans(string ip, string reason = "Banned", string[] tokenDuration = null)
        {
            DateTime dt;
            dt = tokenDuration == null ? new DateTime(1900, 01, 01, 00, 00, 00) : AddTime(tokenDuration);
            AddToIpBans(ip, dt, reason);
        }

        public void AddToIpBans(string ip, DateTime duration, string reason = "Banned")
        {
            if (!HasIpBan(ip))
            {
                _ipbans.Add(new IpBans { Ip = ip, Reason = reason, Duration = duration });
            }
            SaveIpBans();
        }

        public void AddToWhiteList(string player)
        {
            if (!_whitelist.Contains(player.ToLower()))
                _whitelist.Add(player.ToLower());
            SaveWhiteList();
        }

        public void RemoveFromWhiteList(string player)
        {
            if (_whitelist.Contains(player.ToLower()))
                _whitelist.Remove(player.ToLower());
            SaveWhiteList();
        }

        public List<string> ListWhiteList()
        {
            return _whitelist;
        }

        public void RemoveFromBanList(string player)
        {
            bool save = false;
            IBans b = GetBan(player);
            if (b != null)
            {
                _bans.Remove(b as Bans);
                (from e in _bansDoc.Elements("Bans").Elements("PlayerBan")
                 where e.Element("PlayerName").Value == player
                 select e).Remove();
                save = true;
            }
            if (save)
                _bansDoc.Save(BanFilePath);

        }

        public void RemoveFromIpBanList(string ip)
        {
            bool save = false;
            IIpBans i = GetIpBan(ip);
            if (i != null)
            {
                _ipbans.Remove(i as IpBans);
                (from e in _bansDoc.Elements("Bans").Elements("IpBan")
                 where e.Element("IP").Value == ip
                 select e).Remove();
                save = true;
            }
            if (save)
                _bansDoc.Save(BanFilePath);
        }

        public IBans GetBan(string playerName)
        {
            return (from f in _bans where f.PlayerName == playerName select f).FirstOrDefault();
        }

        public IIpBans GetIpBan(string ip)
        {
            return (from f in _ipbans where f.Ip == ip select f).FirstOrDefault();
        }

        public bool HasBan(string playerName)
        {
            return _bansDoc.Elements("Bans").Elements("PlayerBan").Any(x => x.Element("PlayerName").Value.ToLower() == playerName.ToLower());
        }


        public bool HasIpBan(string ip)
        {
            return _bansDoc.Elements("Bans").Elements("IpBan").Any(x => x.Element("IP").Value.ToLower() == ip);
        }

        private void SaveBans()
        {
            bool toSave = false;
            foreach (var ele in from ban in _bans
                                where !HasBan(ban.PlayerName)
                                select new XElement("PlayerBan",
                                       new XElement("PlayerName", ban.PlayerName),
                                       new XElement("Reason", ban.Reason),
                                       new XElement("Duration", ban.Duration)))
            {
                _bansDoc.Root.Add(ele);
                toSave = true;
            }
            if (toSave)
                _bansDoc.Save(BanFilePath);
        }

        private void SaveIpBans()
        {
            bool toSave = false;
            foreach (var ele in from ip in _ipbans
                                where !HasIpBan(ip.Ip)
                                select new XElement(new XElement("IpBan",
                                       new XElement("IP", ip.Ip),
                                       new XElement("Reason", ip.Reason))))
            {
                _bansDoc.Root.Add(ele);
                toSave = true;
            }
            if (toSave)
                _bansDoc.Save(BanFilePath);
        }

        private void SaveWhiteList()
        {
            File.WriteAllLines(WhiteList, _whitelist);
        }

        private XDocument LoadBans(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    return XDocument.Load(fileName);
                }
                catch
                {
                    return null;
                }

            }
            return null;
        }

        public DateTime AddTime(string[] tokens)
        {
            DateTime dt = DateTime.Now;
            for (int i = 2; i < tokens.Length; i++)
            {
                if (tokens[i].StartsWith("d:"))
                {
                    dt = dt.AddDays(Double.Parse(tokens[i].Substring(2)));
                }
                if (tokens[i].StartsWith("h:"))
                {
                    dt = dt.AddHours(Double.Parse(tokens[i].Substring(2)));
                }
                if (tokens[i].StartsWith("m:"))
                {
                    dt = dt.AddMinutes(Double.Parse(tokens[i].Substring(2)));
                }
                if (tokens[i].StartsWith("s:"))
                {
                    dt = dt.AddSeconds(Double.Parse(tokens[i].Substring(2)));
                }
            }
            return dt;
        }
    }

    public class Bans : IBans
    {
        public string PlayerName { get; set; }
        public string Reason { get; set; }
        public DateTime Duration { get; set; }
    }

    public class IpBans : IIpBans
    {
        public string Ip { get; set; }
        public string Reason { get; set; }
        public DateTime Duration { get; set; }
    }
}
