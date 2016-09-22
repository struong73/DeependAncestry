using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeependAncestry.Web.Interface
{
    public interface ICacheHelper
    {
        void Add(string key, object itemToCache);

        void Add(string key, object itemToCache, int cacheTime);

        T Get<T>(string key);

        void Remove(string key);
    }
}
