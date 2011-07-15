using System.Collections;
using System.Collections.Generic;

namespace SquishIt.Framework.Base
{
    internal class Asset
    {
        internal string LocalPath { get; set; }
        internal string RemotePath { get; set; }
        internal int Order { get; set; }
        internal bool IsEmbeddedResource { get; set; }

        internal Asset()
        {
        }

        internal Asset(string localPath, string remotePath = null, int order = 0, bool isEmbeddedResource = false)
        {
            LocalPath = localPath;
            RemotePath = remotePath;
            Order = order;
            IsEmbeddedResource = isEmbeddedResource;
        }

        internal class EqualityComparer : EqualityComparer<Asset>
        {
            public override bool Equals(Asset a1, Asset a2)
            {
                if (a1.LocalPath == a2.LocalPath &
                    a1.RemotePath == a2.RemotePath &
                    a1.Order == a2.Order &
                    a1.IsEmbeddedResource == a2.IsEmbeddedResource)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode(Asset a)
            {
                int hCode =
                    (a.LocalPath != null ? a.LocalPath.GetHashCode() : 0) ^
                    (a.RemotePath != null ? a.RemotePath.GetHashCode() : 0) ^
                    a.Order ^
                    (a.IsEmbeddedResource ? 1 : 0);
                return hCode.GetHashCode();
            }
        }
    }
}