using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGoGUI.ProxyManager
{
    public class ProxyHandler
    {
        public HashSet<GoProxy> Proxies { get; private set; }
        private Random _rand = new Random();

        public ProxyHandler()
        {
            Proxies = new HashSet<GoProxy>();
        }

        public void IncreaseFailCounter(GoProxy proxy)
        {
            lock (proxy)
            {
                ++proxy.CurrentConcurrentFails;
            }
        }

        public void ResetFailCounter(GoProxy proxy)
        {
            lock(proxy)
            {
                proxy.CurrentConcurrentFails = 0;
            }
        }

        public void MarkProxy(GoProxy proxy, bool isBanned = true)
        {
            lock (proxy)
            {
                proxy.IsBanned = isBanned;
            }
        }

        public void ProxyUsed(GoProxy proxy, bool addition = true)
        {
            lock (proxy)
            {
                if (addition)
                {
                    ++proxy.CurrentAccounts;
                }
                else
                {
                    --proxy.CurrentAccounts;
                }
            }
        }

        public void AddProxy(IEnumerable<GoProxy> proxies)
        {
            foreach (GoProxy proxy in proxies)
            {
                AddProxy(proxy);
            }
        }

        public bool AddProxy(GoProxy proxy)
        {
            lock(Proxies)
            {
                return Proxies.Add(proxy);
            }
        }

        public bool AddProxy(string data)
        {
            ProxyEx proxy = null;

            if(!ProxyEx.TryParse(data, out proxy))
            {
                return false;
            }


            GoProxy goProxy = new GoProxy
            {
                Address = proxy.Address,
                Password = proxy.Password,
                Port = proxy.Port,
                Username = proxy.Username
            };

            return AddProxy(goProxy);
        }

        public GoProxy GetRandomProxy()
        {
            if(Proxies == null)
            {
                return null;
            }

            List<GoProxy> availableProxies = new List<GoProxy>();

            lock(Proxies)
            {
                availableProxies = Proxies.Where(x =>
                            x.MaxConcurrentFails > x.CurrentConcurrentFails &&
                            x.MaxAccounts > x.CurrentAccounts &&
                            !x.IsBanned).ToList();

                if (availableProxies.Count == 0)
                {
                    return null;
                }

                GoProxy returnProxy = availableProxies[GetRandom(0, availableProxies.Count)];

                ProxyUsed(returnProxy, true);

                return returnProxy;
            }
        }

        public void RemoveProxy(GoProxy proxy)
        {
            lock(Proxies)
            {
                Proxies.Remove(proxy);
            }
        }

        private int GetRandom(int min, int max)
        {
            lock(_rand)
            {
                return _rand.Next(min, max);
            }
        }
    }
}
