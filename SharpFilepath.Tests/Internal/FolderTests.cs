﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RoseByte.SharpFiles.Extensions;
using RoseByte.SharpFiles.Internal;
using File = System.IO.File;

namespace RoseByte.SharpFiles.Tests
{
    [TestFixture]
    public class FolderTests
    {
        private static FsFolder AppFsFolder => 
            (FsFolder)Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName.ToPath();

        private readonly FsFolder _folder = AppFsFolder.CombineFolder(nameof(FolderTests));

        [OneTimeSetUp]
        public void Setup()
        {
            _folder.Create();

            _folder.CombineFile("Test_0_1.txt").Write("0_1");
            _folder.CombineFile("Test_0_2.txt").Write("0_2");
            
            var subfolder1 = _folder.CombineFolder("SubFolder_1");
            subfolder1.Create();
            subfolder1.CombineFile("Test_1_1.txt").Write("1_1");
            subfolder1.CombineFile("Test_1_2.txt").Write("1_2");
            
            var subfolder11 = _folder.CombineFolder("SubFolder_1_1");
            subfolder11.Create();
            subfolder11.CombineFile("Test_1_1_1.txt").Write("1_1_1");
            subfolder11.CombineFile("Test_1_1_2.txt").Write("1_1_2");
            
            var subfolder12 = _folder.CombineFolder("SubFolder_1_2");
            subfolder12.Create();
            subfolder12.CombineFile("Test_1_2_1.txt").Write("1_2_1");
            subfolder12.CombineFile("Test_1_2_2.txt").Write("1_2_2");
            
            var subfolder2 = _folder.CombineFolder("SubFolder_2");
            subfolder2.Create();
            subfolder2.CombineFile("Test_2_1.txt").Write("2_1");
            subfolder2.CombineFile("Test_2_2.txt").Write("2_2");
            
            var subfolder21 = _folder.CombineFolder("SubFolder_2_1");
            subfolder21.Create();
            subfolder21.CombineFile("Test_2_1_1.txt").Write("2_1_1");
            subfolder21.CombineFile("Test_2_1_2.txt").Write("2_1_2");
            
            var subfolder22 = _folder.CombineFolder("SubFolder_2_2");
            subfolder22.Create();
            subfolder22.CombineFile("Test_2_2_1.txt").Write("2_2_1");
            subfolder22.CombineFile("Test_2_2_2.txt").Write("2_2_2");
        }
        
        [OneTimeTearDown]
        public void TearDown() => _folder.Remove();

        [Test]
        public void ShouldReturnAllFiles()
        {
            var sut = _folder;
            
            Assert.That(
                sut.Files.Count(), 
                Is.EqualTo(Directory.EnumerateFiles(_folder, "*", SearchOption.AllDirectories).Count()));
        }
        
        [Test]
        public void ShouldReturnAllFolders()
        {
            var sut = _folder;
            
            Assert.That(
                sut.Folders.Count(), 
                Is.EqualTo(Directory.EnumerateDirectories(_folder, "*", SearchOption.AllDirectories).Count()));
        }

        [Test]
        public void ShouldFilterToFilesEndingWIthOne()
        {
            var sut = _folder.SetRecursivity(true).SetFileFilter(new Regex(".*1\\.txt"));

            var result = sut.Files;
            
            Assert.That(result.Count(), Is.EqualTo(7));
        }
        
        [Test]
        public void ShouldSkipFilesEndingWithOne()
        {
            var sut = _folder.SetRecursivity(true).SetFileSkip(new Regex(".*1\\.txt"));

            var result = sut.Files;
            
            Assert.That(result.Count(), Is.EqualTo(8));
        }

        [Test]
        public void ShouldTestIfFolderExists()
        {
            Assert.That(_folder.Exists, Is.True);
            Assert.That(_folder.CombineFolder("NOT_HERE").Exists, Is.False);
        }

        [Test]
        public void ShouldSumSIzeOfFolder()
        {
            Assert.That(_folder.CombineFolder("SubFolder_1").Size, Is.EqualTo(12));
        }

        [Test]
        public void ShouldCreateFolderInstance()
        {
            var path = "C:\\";
            var sut = path.ToFolder();
            Assert.That(sut.Value, Is.EqualTo(path));
        }
        
        [Test]
        public void ShouldReturnFalseToIsFolder()
        {
            var sut = "C:\\".ToFolder();
            Assert.That(sut.IsFile, Is.False);
            Assert.That(sut.IsFolder, Is.True);
        }
        
        [Test]
        public void ShouldCombineFile()
        {
            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var sut = Path.Combine(dir).ToPath();
            var parent = sut.Parent;
            var name = sut.ToString().Split('\\').Last();
            
            Assert.That(parent.CombineFolder(name).ToString(), Is.EqualTo(sut.ToString()));
        }
        
        [Test]
        public void ShouldCombineFolder()
        {
            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var sut = Path.Combine(dir).ToPath();
            var parent = sut.Parent;
            var name = sut.ToString().Split('\\').Last();
            
            Assert.That(parent.CombineFolder(name).ToString(), Is.EqualTo(sut.ToString()));
        }
        
        [Test]
        public void ShouldGetParentDirectory()
        {
            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var parentDir = Directory.GetParent(dir).FullName;
            var sut = Path.Combine(dir).ToPath();
            var parent = sut.Parent;
            
            Assert.That(parent.ToString(), Is.EqualTo(parentDir));
        }
        
        [Test]
        public void ShouldCreateFolderWithParentFolder()
        {
            AppFsFolder.CombineFolder("FolderCreationTest\\Subfolder\\OneMoreSubfolder").Create();

            var firstFolder = Path.Combine(AppFsFolder, "FolderCreationTest");
            var secondFolder = Path.Combine(AppFsFolder, "FolderCreationTest\\Subfolder");
            var thirdFolder =Path.Combine(AppFsFolder, "FolderCreationTest\\Subfolder\\OneMoreSubfolder");
            
            Assert.That(Directory.Exists(firstFolder), Is.True);
            Assert.That(Directory.Exists(secondFolder), Is.True);
            Assert.That(Directory.Exists(thirdFolder), Is.True);
            
            AppFsFolder.CombineFolder("FolderCreationTest").Remove();
            
            Assert.That(Directory.Exists(firstFolder), Is.False);
            Assert.That(Directory.Exists(secondFolder), Is.False);
            Assert.That(Directory.Exists(thirdFolder), Is.False);
        }
        
        [Test]
        public void ShouldCreateFolderWithDot()
        {
            var sut = new Folder("C:\\Test.Folder");
            
            Assert.That(sut.ToString(), Is.EqualTo("C:\\Test.Folder"));
        }

        [Test]
        public void ShouldCopySubfile()
        {
            var value = "SubFolder_1\\Test_1_1.txt";
            var parent = _folder.CombineFolder(nameof(ShouldCopySubfile));
            var subFile = new FsChild<FsFile>(_folder, _folder.CombineFile(value));
            
            Assert.That(parent.CombineFile(value).Exists, Is.False);
            parent.Copy(subFile);
            Assert.That(parent.CombineFile(value).Content, Is.EqualTo("1_1"));
            parent.Remove();
        }
        
        [Test]
        public void ShouldRemoveSubfolder()
        {
            var value = nameof(ShouldRemoveSubfolder);
            var subFile = new FsChild<FsFolder>(_folder, _folder.CombineFolder(value));
            
            _folder.Create(subFile);
            Assert.That(_folder.CombineFolder(value).Exists, Is.True);
            _folder.Remove(subFile);
            Assert.That(_folder.CombineFolder(value).Exists, Is.False);
        }

        [Test]
        public void ShouldRemoveSubfile()
        {
            var value = $"{nameof(ShouldRemoveSubfile)}\\{nameof(ShouldRemoveSubfile)}";
            var subFile = new FsChild<FsFile>(_folder, _folder.CombineFile(value));
            subFile.Child.Write("A");
            Assert.That(_folder.CombineFile(value).Exists, Is.True);
            _folder.Remove(subFile);
            Assert.That(_folder.CombineFile(value).Exists, Is.False);
        }

        [Test]
        public void ShouldCreateSubFolder()
        {
            var value = nameof(ShouldCreateSubFolder);
            Assert.That(_folder.CombineFolder(value).Exists, Is.False);
            var subFile = new FsChild<FsFolder>(_folder, _folder.CombineFolder(value));
            _folder.Create(subFile);
            Assert.That(_folder.CombineFolder(subFile.Value).Exists, Is.True);
            _folder.CombineFolder(nameof(ShouldCreateSubFolder)).Remove();
        }

        [Test]
        public void ShouldRemove()
        {
            var subFolder = _folder.CombineFolder(nameof(ShouldRemove));
            subFolder.Create();
            subFolder.CombineFile($"{nameof(ShouldRemove)}_1").Write("A");
            subFolder.CombineFile($"{nameof(ShouldRemove)}_2").Write("B");
            Assert.That(subFolder.Exists, Is.True);
            subFolder.Remove();
            Assert.That(subFolder.Exists, Is.False);
        }
        
        [Test]
        public void ShouldSyncStructures()
        {
            var donor = AppFsFolder.CombineFolder("FolderSyncingTestDonor");
            var acceptor = AppFsFolder.CombineFolder("FolderSyncingTestAcceptor");
            
            donor.CombineFolder("Subfolder").Create();
            donor.CombineFolder("Subfolder2").Create();
            File.WriteAllText(donor.CombineFile("test1.txt").ToString(), "A");
            File.WriteAllText(donor.CombineFile("test2.txt").ToString(), "A");
            File.WriteAllText(donor.CombineFile("Subfolder\\test3.txt").ToString(), "A");
            File.WriteAllText(donor.CombineFile("Subfolder2\\test4.txt").ToString(), "A");
            File.WriteAllText(donor.CombineFile("Subfolder2\\test5.txt").ToString(), "A");
            
            acceptor.CombineFolder("Subfolder2").Create();
            acceptor.CombineFolder("Subfolder3").Create();
            File.WriteAllText(acceptor.CombineFile("test1.txt").ToString(), "A");
            File.WriteAllText(acceptor.CombineFile("test3.txt").ToString(), "A");
            File.WriteAllText(acceptor.CombineFile("Subfolder2\\test4.txt").ToString(), "A");
            File.WriteAllText(acceptor.CombineFile("Subfolder2\\test1.txt").ToString(), "A");
            File.WriteAllText(acceptor.CombineFile("Subfolder3\\test5.txt").ToString(), "A");
            
            donor.SyncStructure(acceptor);

            var files = acceptor.Files.Select(x => x.Value);
            var folders = acceptor.Folders.Select(x => x.Value);
            
            Assert.That(
                folders, 
                Is.EquivalentTo(new []
                {
                    "Subfolder",
                    "Subfolder2"
                }));
            
            Assert.That(
                files, 
                Is.EquivalentTo(new []
                {
                    "test1.txt",
                    "Subfolder2\\test4.txt"
                }));

            AppFsFolder.CombineFile("FolderSyncingTestDonor").Remove();
            AppFsFolder.CombineFile("FolderSyncingTestAcceptor").Remove();
        }
    }
}