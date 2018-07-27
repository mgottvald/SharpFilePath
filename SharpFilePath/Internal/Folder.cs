﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RoseByte.SharpFiles.Extensions;

namespace RoseByte.SharpFiles.Internal
{
    public class Folder : FsFolder
    {
        internal Folder(string value) : base(value) { }

        private Folder(string value, bool resursive, Regex filterFolders, Regex skipFolders, Regex filterFiles,
            Regex skipFiles) : base(value)
        {
            Recursive = resursive;
            FilesFilter = filterFiles;
            FoldersFilter = filterFolders;
            FilesSkip = skipFiles;
            FoldersSkip = skipFolders;
        }

        public override bool Recursive { get; } = true;
        public override Regex FilesFilter { get; }
        public override Regex FilesSkip { get; }
        public override Regex FoldersFilter { get; }
        public override Regex FoldersSkip { get; }

        public override FsFolder SetFileSkip(Regex rgx)
        {
            return new Folder(this, Recursive, FoldersFilter, FoldersSkip, FilesFilter, rgx);
        }
        
        public override FsFolder SetFileFilter(Regex rgx)
        {
            return new Folder(this, Recursive, FoldersFilter, FoldersSkip, rgx, FilesSkip);
        }
        
        public override FsFolder SetFolderSkip(Regex rgx)
        {
            return new Folder(this, Recursive, FoldersFilter, rgx, FilesFilter, FilesSkip);
        }
        
        public override FsFolder SetFolderFilter(Regex rgx)
        {
            return new Folder(this, Recursive, rgx, FoldersSkip, FilesFilter, FilesSkip);
        }
        
        public override FsFolder SetRecursivity(bool recursivity)
        {
            return new Folder(this, recursivity, FoldersFilter, FoldersSkip, FilesFilter, FilesSkip);
        }
        
        public override bool Exists => Directory.Exists(Value);
        protected override long GetSize() => Files.Sum(x => x.Child.Size);
        
        public override FsFile CombineFile(string pathPart) => new File(Path.Combine(this, pathPart));       
        public override FsFolder CombineFolder(string pathPart) => new Folder(Path.Combine(Value, pathPart));
        public override FsFolder Parent => new Folder(Directory.GetParent(Value).FullName);
        public override IEnumerable<FsChild<FsFile>> Files => GetFiles(Value);
        public override IEnumerable<FsChild<FsFolder>> Folders => GetFolders(Value);

        private IEnumerable<FsChild<FsFile>> GetFiles(string path)
        {
            IEnumerable<FsFolder> folders = new []{path.ToFolder()};

            if (Recursive)
            {
                folders = folders.Union(GetFolders(path).Select(x => x.Child));
            }

            foreach (var folder in folders)
            {
                var files = Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    var subpath = new FsChild<FsFile>(this, new File(file));
                    
                    if (!FilesFilter?.IsMatch(subpath.Value) ?? false)
                    {
                        continue;
                    }
                
                    if (FilesSkip?.IsMatch(subpath.Value) ?? false)
                    {
                        continue;
                    }
                
                    yield return subpath;
                }
            }
        }
        
        private IEnumerable<FsChild<FsFolder>> GetFolders(string path)
        {
            var files = Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
            
            foreach (var file in files)
            {
                var subpath = new FsChild<FsFolder>(this, new Folder(file));
                
                if (!FoldersFilter?.IsMatch(subpath.Value) ?? false)
                {
                    continue;
                }
                
                if (FoldersSkip?.IsMatch(subpath.Value) ?? false)
                {
                    continue;
                }
                
                yield return subpath;

                if (Recursive)
                {
                    foreach (var subFolder in GetFolders(subpath.Child))
                    {
                        yield return subFolder;
                    }
                }
            }
        }
        
        public override void Copy(FsChild<FsFile> child) => child.Child.Copy(CombineFile(child.Value));
        public override void Create(FsChild<FsFolder> child) => CombineFolder(child.Value).Create();
        public override void Remove(FsChild<FsFolder> child) => CombineFolder(child.Value).Remove();
        public override void Remove(FsChild<FsFile> child) => CombineFile(child.Value).Remove();
        
        public override void SyncStructure(FsFolder destination, bool force)
        {
            var folders = Folders.ToList();
            var destFolders = destination.Folders.ToList();

            foreach (var subFolder in destFolders.Except(folders))
            {
                destination.CombineFolder(subFolder.Value).Remove();
            }
            
            foreach (var subFile in destination.Files.Except(Files))
            {
                destination.CombineFile(subFile.Value).Remove();
            }

            foreach (var folder in folders.Except(destFolders))
            {
                destination.CombineFolder(folder.Value).Create();
            }
        }
        
        public override void Remove()
        {
            if (!Exists)
            {
                return;
            }

            foreach (var file in Files)
            {
                file.Child.Remove();
            }
            
            Directory.Delete(Value, true);
        }
        
        public override void Create()
        {
            if (Exists)
            {
                return;
            }

            if (Parent == null)
            {
                throw new Exception("Top level node (e.g. drive) can't be created.");
            }
            
            if (!Parent.Exists)
            {
                Parent.Create();
            }

            Directory.CreateDirectory(Value);
        }
    }
}