using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography;
using System.IO;

namespace WPFGraph
{
    class FileCache : TokenCache
    {
        private string CacheFilePath;
        private static readonly object FileLock = new object();
        public FileCache(string filePath = @".\TokenCache.dat")
        {
            CacheFilePath = filePath;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;

            lock (FileLock)
            {
                this.DeserializeAdalV3(File.Exists(CacheFilePath) ? ProtectedData.Unprotect(
                    File.ReadAllBytes(CacheFilePath), null,
                    DataProtectionScope.CurrentUser) : null);
            }
        }

        public override void Clear()
        {
            base.Clear();
            File.Delete(CacheFilePath);
        }

        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                this.DeserializeAdalV3(File.Exists(CacheFilePath) ? ProtectedData.Unprotect(
                    File.ReadAllBytes(CacheFilePath), null,
                        DataProtectionScope.CurrentUser) : null);
            }
        }

        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                lock (FileLock)
                {
                    File.WriteAllBytes(CacheFilePath,
                        ProtectedData.Protect(this.SerializeAdalV3(), null,
                        DataProtectionScope.CurrentUser));
                    this.HasStateChanged = false;
                }
            }
        }
    }
}
