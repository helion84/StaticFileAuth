using System;

namespace StaticFileAuth.Cache
{
    public interface IKeyCache
    {
        Guid GenerateKey();
        bool IsValid(string key);
        void RefreshKey(string key);
    }
}
