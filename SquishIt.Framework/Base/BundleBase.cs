using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using SquishIt.Framework.Cachers;
using SquishIt.Framework.Minifiers;
using SquishIt.Framework.Resolvers;
using SquishIt.Framework.Renderers;
using SquishIt.Framework.Files;
using SquishIt.Framework.Utilities;

namespace SquishIt.Framework.Base
{
    public abstract class BundleBase<T> : IBundle where T : BundleBase<T>
    {
        private const string DEFAULT_GROUP = "default";
        protected IFileWriterFactory fileWriterFactory;
        protected IFileReaderFactory fileReaderFactory;
        protected IDebugStatusReader debugStatusReader;
        protected ICurrentDirectoryWrapper currentDirectoryWrapper;
        protected IHasher hasher;

        protected abstract IMinifier<T> DefaultMinifier { get; }
        private IMinifier<T> minifier;
        protected IMinifier<T> Minifier
        {
            get
            {
                return minifier ?? DefaultMinifier;
            }
            set { minifier = value; }
        }

        protected string HashKeyName { get; set; }
        private bool ShouldRenderOnlyIfOutputFileIsMissing { get; set; }
        internal List<string> DependentFiles = new List<string>();
        internal Dictionary<string, GroupBundle> GroupBundles = new Dictionary<string, GroupBundle>
        {
            { DEFAULT_GROUP, new GroupBundle() }
        };

        internal ICacher Cacher { get; set; }
        internal IRenderer Renderer { get; set; }
        internal string Named { get; set; }
        internal string RenderTo { get; set; }
        internal string Tag { get; set; }

        private bool IgnoreCache { get; set; }

        protected BundleBase(IFileWriterFactory fileWriterFactory, IFileReaderFactory fileReaderFactory, IDebugStatusReader debugStatusReader, ICurrentDirectoryWrapper currentDirectoryWrapper, IHasher hasher, ICacher cacher)
        {
            this.fileWriterFactory = fileWriterFactory;
            this.fileReaderFactory = fileReaderFactory;
            this.debugStatusReader = debugStatusReader;
            this.currentDirectoryWrapper = currentDirectoryWrapper;
            this.hasher = hasher;
            Cacher = cacher;
            ShouldRenderOnlyIfOutputFileIsMissing = false;
            HashKeyName = "r";
        }

        private List<string> GetResolvedSystemPaths(IEnumerable<Asset> assets)
        {
            var inputFiles = new List<string>();
            foreach (var asset in assets)
            {
                var path = GetResolvedSystemPath(asset);
                if (!String.IsNullOrEmpty(path))
                {
                    inputFiles.Add(path);
                }
            }

            return inputFiles;
        }

        private string GetResolvedSystemPath(Asset asset)
        {
            string path;
            if (asset.RemotePath == null)
            {
                path = GetFileSystemPath(asset.LocalPath);
            }
            else
            {
                if (asset.IsEmbeddedResource)
                {
                    path = GetEmbeddedResourcePath(asset.RemotePath);
                }
                else
                {
                    //Remote files do not need to be resolved as they cannot be a cache dependency
                    path = debugStatusReader.IsDebuggingEnabled() ? GetFileSystemPath(asset.LocalPath) : null;
                }
            }

            return path;
        }

        private string GetResolvedSystemPath<T>(string filePath) where T : IResolver
        {
            string resolvedPath = null;
            foreach (var path in ResolverFactory.Get<T>().Resolve(filePath))
            {
                resolvedPath = path;
            }

            return resolvedPath;
        }

        private string GetFileSystemPath(string localPath)
        {
            return GetResolvedSystemPath<FileResolver>(localPath);
        }

        private string GetEmbeddedResourcePath(string path)
        {
            return GetResolvedSystemPath<EmbeddedResourceResolver>(path);
        }

        private string ExpandAppRelativePath(string file)
        {
            if (file.StartsWith("~/"))
            {
                string appRelativePath = HttpRuntime.AppDomainAppVirtualPath;
                if (appRelativePath != null && !appRelativePath.EndsWith("/"))
                    appRelativePath += "/";
                return file.Replace("~/", appRelativePath);
            }
            return file;
        }

        protected string ReadFile(string file)
        {
            using (var sr = fileReaderFactory.GetFileReader(file))
            {
                return sr.ReadToEnd();
            }
        }

        protected bool FileExists(string file)
        {
            return fileReaderFactory.FileExists(file);
        }

        private string GetAdditionalAttributes(GroupBundle groupBundle)
        {
            var result = new StringBuilder();
            foreach (var attribute in groupBundle.Attributes)
            {
                result.Append(attribute.Key);
                result.Append("=\"");
                result.Append(attribute.Value);
                result.Append("\" ");
            }
            return result.ToString();
        }

        private string GetRemoteTags(List<Asset> remoteAssets, GroupBundle groupBundle)
        {
            var sb = new StringBuilder();
            foreach (var asset in remoteAssets)
            {
                sb.Append(FillTemplate(groupBundle, asset.RemotePath));
            }

            return sb.ToString();
        }

        private void AddAsset(Asset asset, string group = DEFAULT_GROUP)
        {
            GroupBundle groupBundle;
            if (GroupBundles.TryGetValue(group, out groupBundle))
            {
                groupBundle.Assets.Add(asset);
            }
            else
            {
                groupBundle = new GroupBundle();
                groupBundle.Assets.Add(asset);
                GroupBundles[group] = groupBundle;
            }
        }

        public T Add(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                AddAsset(new Asset(filePath));
            }

            return (T)this;
        }

/*
        public T Add(string filePath)
        {
            AddAsset(new Asset(filePath));
            return (T)this;
        }
*/
  
        public T AddToGroup(string group, params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                AddAsset(new Asset(filePath), group);
            }

            return (T)this;
        }

        /*
        public T AddToGroup(string group, string filePath)
        {
            AddAsset(new Asset(filePath), group);
            return (T)this;
        }
        */
        public T AddRemote(string localPath, string remotePath)
        {
            AddAsset(new Asset(localPath, remotePath));
            return (T)this;
        }

        public T AddEmbeddedResource(string localPath, string embeddedResourcePath)
        {
            AddAsset(new Asset(localPath, embeddedResourcePath, 0, true));
            return (T)this;
        }

        private void RemoveAsset(string[] filePaths, string group = DEFAULT_GROUP)
        {
            foreach (var filePath in filePaths)
            {
                GroupBundles[group].Assets.RemoveWhere(asset => asset.LocalPath == filePath || asset.RemotePath == filePath);
            }
        }

        public T Remove(params string[] filePaths)
        {
            RemoveAsset(filePaths);            
            return (T)this;
        }

        public T RemoveFromGroup(string group, params string[] filePaths)
        {
            RemoveAsset(filePaths, group);
            return (T)this;
        }

        public T RenderOnlyIfOutputFileMissing()
        {
            ShouldRenderOnlyIfOutputFileIsMissing = true;
            return (T)this;
        }

        public T ForceDebug()
        {
            debugStatusReader.ForceDebug();
            return (T)this;
        }

        public T ForceRelease()
        {
            debugStatusReader.ForceRelease();
            return (T)this;
        }

        public string Render(string renderTo)
        {
            string key = renderTo + GroupBundles.GetHashCode();
            return Render(renderTo, key, new FileRenderer(fileWriterFactory), Cacher);
        }

        internal string Render(string renderTo, string key, IRenderer renderer, ICacher cacher)
        {
            Named = key;
            RenderTo = renderTo;
            Renderer = renderer;
            Cacher = cacher;
            
            key = CachePrefix + key;
            if (debugStatusReader.IsDebuggingEnabled())
            {
                return RenderDebug(key);
            }
            return RenderRelease(key);
        }


        public T GetNamed(string name)
        {
            var bundle = CacherFactory.Get<ApplicationCache>().Get<T>(CachePrefix + name);
            bundle.IgnoreCache = true;
            return bundle;
        }

        private string RenderNamedTag(string name)
        {
            return GetNamed(name).Tag;
        }

        public string RenderNamed(string name)
        {
            return IgnoreCache ? AsNamed(name, RenderTo) : RenderNamedTag(name);
        }

        public string RenderCached(string name)
        {
            return IgnoreCache ? AsCached(name, RenderTo) : CacheRenderer.Get(CachePrefix, name);
        }

        public string RenderCachedAssetTag(string name)
        {
            return RenderNamedTag(name);
        }

        public string AsNamed(string name, string renderTo)
        {
            return Render(renderTo, name, new FileRenderer(fileWriterFactory), CacherFactory.Get<ApplicationCache>());
        }

        public string AsCached(string name, string filePath)
        {
            return Render(filePath, name, new CacheRenderer(CachePrefix, name), CacherFactory.Get<ApplicationCache>());
        }

        protected string RenderDebug(string key)
        {
            var cacher = Cacher;

            T bundle = null;
            var inCache = cacher.TryGetValue<T>(key, out bundle);
            Tag = inCache ? bundle.Tag : null;

            if (!inCache || IgnoreCache)
            {
                DependentFiles.Clear();

                var modifiedGroupBundles = BeforeRenderDebug();
                var sb = new StringBuilder();
                foreach (var groupBundleKVP in modifiedGroupBundles)
                {
                    var groupBundle = groupBundleKVP.Value;
                    var attributes = GetAdditionalAttributes(groupBundle);
                    var assets = groupBundle.Assets;

                    DependentFiles.AddRange(GetResolvedSystemPaths(assets));
                    foreach (var asset in assets)
                    {
                        string processedFile = ExpandAppRelativePath(asset.LocalPath);

                        if (asset.IsEmbeddedResource)
                        {                           
                            var contents = ReadFile(GetEmbeddedResourcePath(asset.RemotePath));
                            new FileRenderer(fileWriterFactory).Render(contents, FileSystem.ResolveAppRelativePathToFileSystem(processedFile));
                        }

                        sb.Append(FillTemplate(groupBundle, processedFile));
                        sb.Append("\n");
                    }
                }

                IgnoreCache = false;
                Tag = sb.ToString();
                cacher.Add<T>(key, (T)this);
            }

            return Tag;
        }

        private string RenderRelease(string key)
        {
            var renderTo = RenderTo;
            var cacher = Cacher;
            var renderer = Renderer;

            T bundle = null;
            var inCache = cacher.TryGetValue<T>(key, out bundle);
            Tag = inCache ? bundle.Tag : null;

            if (!inCache || IgnoreCache)
            {
                var files = new List<string>();
                foreach (var groupBundleKVP in GroupBundles)
                {
                    var group = groupBundleKVP.Key;
                    var groupBundle = groupBundleKVP.Value;

                    string minifiedContent = null;
                    string hash = null;
                    bool hashInFileName = false;

                    DependentFiles.Clear();

                    string outputFile = FileSystem.ResolveAppRelativePathToFileSystem(renderTo);

                    var localAssets = new List<Asset>();
                    var remoteAssets = new List<Asset>();
                    var embeddedAssets = new List<Asset>();
                    foreach (var asset in groupBundle.Assets)
                    {
                        if (asset.RemotePath == null)
                        {
                            localAssets.Add(asset);
                        }
                        else if (!asset.IsEmbeddedResource)
                        {
                            remoteAssets.Add(asset);
                        }
                        else if (asset.IsEmbeddedResource)
                        {
                            embeddedAssets.Add(asset);
                        }
                    }

                    var assetsToResolve = new List<Asset>(localAssets);
                    assetsToResolve.AddRange(embeddedAssets);

                    files.AddRange(GetResolvedSystemPaths(groupBundle.Assets));
                    DependentFiles.AddRange(files);

                    if (renderTo.Contains("#"))
                    {
                        hashInFileName = true;
                        minifiedContent = Minifier.Minify(BeforeMinify(outputFile, files));
                        hash = hasher.GetHash(minifiedContent);
                        outputFile = outputFile.Replace("#", hash);
                    }

                    if (ShouldRenderOnlyIfOutputFileIsMissing && FileExists(outputFile) && minifiedContent == null)
                    {
                        minifiedContent = ReadFile(outputFile);
                    }
                    else
                    {
                        minifiedContent = minifiedContent ?? Minifier.Minify(BeforeMinify(outputFile, files));
                        renderer.Render(minifiedContent, outputFile);
                    }

                    if (hash == null)
                    {
                        hash = hasher.GetHash(minifiedContent);
                    }

                    string renderedTag;
                    if (hashInFileName)
                    {
                        renderedTag = FillTemplate(groupBundle, outputFile);
                    }
                    else
                    {
                        var hashedPath = outputFile + "?" + HashKeyName + "=" + hash;

                        var queryStringIndexOf = renderTo.IndexOf("?");
                        if (queryStringIndexOf > -1)
                        {
                            hashedPath += "&" + renderTo.Substring(queryStringIndexOf + 1);
                        }

                        renderedTag = FillTemplate(groupBundle, hashedPath);
                    }

                    Tag += String.Concat(GetRemoteTags(remoteAssets, groupBundle), renderedTag);
                }

                IgnoreCache = false;
                cacher.Add<T>(key, (T)this);
            }

            return Tag;
        }

        public void ClearCache()
        {
            CacherFactory.Get<ApplicationCache>().Clear();
            //CacherFactory.Get<MemoryCache>().Clear();
        }

        private void AddAttributes(Dictionary<string, string> attributes, string group = DEFAULT_GROUP, bool merge = true)
        {
            GroupBundle groupBundle;
            if (GroupBundles.TryGetValue(group, out groupBundle))
            {
                if (merge)
                {
                    foreach (var attribute in attributes)
                    {
                        groupBundle.Attributes[attribute.Key] = attribute.Value;
                    }
                }
                else
                {
                    groupBundle.Attributes = attributes;
                }
            }
            else
            {
                GroupBundles[group] = new GroupBundle(attributes);
            }
        }

        public T WithAttribute(string name, string value)
        {
            AddAttributes(new Dictionary<string, string> { { name, value } });
            return (T)this;
        }

        public T WithAttributes(Dictionary<string, string> attributes, bool merge = true)
        {
            AddAttributes(attributes, merge: merge);
            return (T)this;
        }

        public T WithGroupAttribute(string name, string value, string group)
        {
            AddAttributes(new Dictionary<string, string> { { name, value } }, group);
            return (T)this;
        }

        public T WithGroupAttributes(Dictionary<string, string> attributes, string group, bool merge = true)
        {
            AddAttributes(attributes, group, merge);
            return (T)this;
        }

        public T WithMinifier<TMin>() where TMin : IMinifier<T>
        {
            Minifier = MinifierFactory.Get<TMin>();
            return (T)this;
        }

        public T WithMinifier<TMin>(TMin minifier) where TMin : IMinifier<T>
        {
            Minifier = minifier;
            return (T)this;
        }

        private string FillTemplate(GroupBundle groupBundle, string path)
        {
            return string.Format(Template, GetAdditionalAttributes(groupBundle), path);
        }

        public T HashKeyNamed(string hashQueryStringKeyName)
        {
            HashKeyName = hashQueryStringKeyName;
            return (T)this;
        }

        protected virtual string BeforeMinify(string outputFile, List<string> files)
        {
            var sb = new StringBuilder();
            foreach (var file in files)
            {
                sb.Append(ReadFile(file) + "\n");
            }

            return sb.ToString();
        }

        internal virtual Dictionary<string, GroupBundle> BeforeRenderDebug()
        {
            return GroupBundles;
        }

        protected abstract string Template { get; }
        internal abstract string CachePrefix { get; }
    }
}