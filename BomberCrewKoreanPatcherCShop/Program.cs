using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using BomberCrewKoreanPatcherManagedCpp;

namespace BomberCrewKoreanPatcherCShop
{
    class Program
    {
        static string currentDirectoryPath;
        static string BomberCrewPath;
        const string TEMP_FOLDER_NAME = @"temp\";
        const string RESOURCE_FOLDER_PATH = @"Resource\";
        const string SHAREDASSETS0_PATCH_NAME = "sharedassets0_patch";
        const string SHAREDASSETS0_PATCH_PATH = TEMP_FOLDER_NAME + SHAREDASSETS0_PATCH_NAME + @"\";
        const string RESOURCE_PATCH_NAME = "resources_patch";
        const string RESOURCE_PATCH_PATH = TEMP_FOLDER_NAME + RESOURCE_PATCH_NAME + @"\";
        const string GAME_NAME = "BomberCrew";
        const string UNITY_RESOURCES_ASSETS_NAME = "resources.assets";
        const string UNITY_SHARED0_ASSETS_NAME = "sharedassets0.assets";
        const bool DEBUG = true;

        static void Main(string[] args)
        {
            BomberCrewPath = FindUnityFolderPath();
            if(BomberCrewPath == "ERROR")
            {
                Console.WriteLine("Please Write " + GAME_NAME + "_Data\\ Folder Full Path and Press ENTER : ");
                string readFolderPath = Console.ReadLine();
                if (readFolderPath.Substring(readFolderPath.Length - 1, 1) != "\\")
                {
                    readFolderPath = readFolderPath + "\\";
                }
                if (FolderExist(readFolderPath) && FileExist(readFolderPath + UNITY_RESOURCES_ASSETS_NAME))
                {
                    BomberCrewPath = readFolderPath;
                }
            }
            currentDirectoryPath = Directory.GetCurrentDirectory() + @"\";
            string downloadedFileName = DownloadStringFile();

            BomberCrewKoreanPatcherManagedCpp.ManagedPatcher managedPatcher
                = new BomberCrewKoreanPatcherManagedCpp.ManagedPatcher(BomberCrewPath, currentDirectoryPath);

            BomberCrewKoreanPatcherManagedCpp.AssetInfo[] AssetInfos =  managedPatcher.GetAssetInfos();
            MakeAssetFile(AssetInfos);

            managedPatcher.MakeModdedAssets();
            managedPatcher.Dispose();
            SwitchFile();

            Console.WriteLine("한글패치 완료 Press any key to exit");
            Console.Read();
        }

        static void SwitchFile()
        {
            FileDeleteIfExist(BomberCrewPath + UNITY_RESOURCES_ASSETS_NAME);
            FileDeleteIfExist(BomberCrewPath + UNITY_SHARED0_ASSETS_NAME);
            System.IO.File.Move(BomberCrewPath + UNITY_RESOURCES_ASSETS_NAME + ".modded", BomberCrewPath + UNITY_RESOURCES_ASSETS_NAME);
            System.IO.File.Move(BomberCrewPath + UNITY_SHARED0_ASSETS_NAME + ".modded", BomberCrewPath + UNITY_SHARED0_ASSETS_NAME);
        }

        //static void FileDeleteIfExist(string filePath)
        //{
        //    if (System.IO.File.Exists(filePath))
        //    {
        //        // Use a try block to catch IOExceptions, to
        //        // handle the case of the file already being
        //        // opened by another process.
        //        try
        //        {
        //            System.IO.File.Delete(filePath);
        //        }
        //        catch (System.IO.IOException e)
        //        {
        //            Console.WriteLine(e.Message);
        //            return;
        //        }
        //    }
        //}
        static void FileDeleteIfExist(string filePath)
        {
            FileInfo fileDel = new FileInfo(filePath);
            if (fileDel.Exists) //삭제할 파일이 있는지
            {
                fileDel.Delete(); //없어도 에러안남
            }
        }

        static string FindUnityFolderPath()
        {
            string programFilesPath = Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            string programFilesX64Path = "";
            if (programFilesPath.Substring(programFilesPath.Length-5,5) == "(x86)")
            {
                programFilesX64Path = programFilesPath.Substring(0, programFilesPath.Length - 6) + @"\";
            }
            programFilesPath = programFilesPath + @"\";
            string steamRelativePath = @"Steam\steamapps\common\" + GAME_NAME + @"\" + GAME_NAME + @"_Data\";
            string steamLibraryRelativePath = @"SteamLibrary\steamapps\common\" + GAME_NAME + @"\" + GAME_NAME + @"_Data";
            string[] rootDirectoryPath = new string[26];
            char ch = 'A';
            for (int i=0; i< rootDirectoryPath.Length; i++)
            {
                rootDirectoryPath[i] = ch + @":\";
                ch++;
            }
            if (FolderExist(GAME_NAME + @"_Data\"))
            {
                return GAME_NAME + @"_Data\";
            }
            if (FileExist(UNITY_RESOURCES_ASSETS_NAME))
            {
                return @".\";
            }
            if (FileExist(@"..\" + UNITY_RESOURCES_ASSETS_NAME))
            {
                return @"..\";
            }
            if (programFilesPath != "" && FolderExist(programFilesPath + steamRelativePath))
            {
                return programFilesPath + steamRelativePath;
            }
            if (programFilesX64Path != "" && FolderExist(programFilesX64Path + steamRelativePath))
            {
                return programFilesX64Path + steamRelativePath;
            }
            for (int i = 0; i < rootDirectoryPath.Length; i++)
            {
                if (FolderExist(rootDirectoryPath[i] + steamRelativePath))
                {
                    return rootDirectoryPath[i] + steamRelativePath;
                }
            }
            for (int i = 0; i < rootDirectoryPath.Length; i++)
            {
                if (FolderExist(rootDirectoryPath[i] + steamLibraryRelativePath))
                {
                    return rootDirectoryPath[i] + steamLibraryRelativePath;
                }
            }
            return "ERROR";
        }

        static bool FolderExist(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.Exists;

        }
        
        static bool FileExist(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            return fi.Exists;
        }

        static string DownloadStringFile()
        {
            const string downloadURL = @"https://github.com/dmc31a42/BomberCrewKoreanPatcher/raw/master/LanguageData.json.txt";
            string downloadFileName = "";
            string[] splitedDownloadURL = downloadURL.Split('/');
            downloadFileName = splitedDownloadURL[splitedDownloadURL.Length - 1];
            CreateFolderOrClean(TEMP_FOLDER_NAME);
            WebClient webClient = new WebClient();
            try
            {
                // 폴더없는 단일경로의 경우 앞에 .\ 무조건 붙여야함 아니면 예외뜸
                // 그리고 파일이 있으면 예외뜸
                webClient.DownloadFile(downloadURL, TEMP_FOLDER_NAME + downloadFileName);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
            return downloadFileName;
        }

        static void CreateFolderOrClean(string folderName)
        {
            DirectoryInfo di = new DirectoryInfo(folderName);
            if (di.Exists == false)
            {
                di.Create();
            }
            else
            {
                Directory.Delete(folderName, true);
                di.Create();
            }
        }

        static void MakeAssetFile(BomberCrewKoreanPatcherManagedCpp.AssetInfo[] AssetInfos)
        {
            //string transformSuffix = "_Transform";
            string monoBehaviour2Suffix = "_MonoBehaviour2";
            string shaderSuffix = "_Shader";
            string atlasSuffix = "_Atlas";
            string tk2dFontDataSuffix = "_tk2FontData";
            string[] materialName =
            {
                "UILargematerial",
                "UIMediummaterial",
                "UISmallmaterial"
            };
            string[] gameObjectName =
            {
                "UILargedata",
                "UISmalldata",
                "UIMediumdata"
            };

            CreateFolderOrClean(SHAREDASSETS0_PATCH_PATH);
            List<string> PatchList = new List<string>();
                // Material
                for (int i = 0; i < materialName.Length; i++)
                {
                    AssetInfo materialInfo = Array.Find(AssetInfos, x => x.name == materialName[i]);
                    File.Copy(RESOURCE_FOLDER_PATH + materialName[i] + @".dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat", true);
                PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat");
                    using (FileStream fsMaterial = new FileStream(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + materialInfo.pathID + ".dat", FileMode.Open, FileAccess.ReadWrite))
                    {
                        // Shader
                        fsMaterial.Seek(0x00000018, SeekOrigin.Begin);
                        AssetInfo shaderInfo = Array.Find(AssetInfos, x => x.name == materialName[i] + shaderSuffix);
                        byte[] byteShaderPathID = BitConverter.GetBytes(shaderInfo.pathID);
                        for (int j = 0; j < 4; j++)
                        {
                            fsMaterial.WriteByte(byteShaderPathID[j]);
                        }

                        // Atlas
                        fsMaterial.Seek(0x0000004C, SeekOrigin.Begin);
                        AssetInfo atlasInfo = Array.Find(AssetInfos, x => x.name == materialName[i] + atlasSuffix);
                        byte[] byteAtlasPathID = BitConverter.GetBytes(atlasInfo.pathID);
                        for (int j = 0; j < 4; j++)
                        {
                            fsMaterial.WriteByte(byteAtlasPathID[j]);
                        }
                        File.Copy(RESOURCE_FOLDER_PATH + materialName[i] + atlasSuffix + @".dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + atlasInfo.pathID + ".dat", true);
                    PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + atlasInfo.pathID + ".dat");
                    }
                }

            for (int i = 0; i < gameObjectName.Length; i++)
            {
                AssetInfo mono2Info = Array.Find(AssetInfos, x => x.name == gameObjectName[i] + monoBehaviour2Suffix);
                File.Copy(RESOURCE_FOLDER_PATH + gameObjectName[i] + monoBehaviour2Suffix + @".dat", SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat", true);
                PatchList.Add(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat");
                using (FileStream fsMono2 = new FileStream(SHAREDASSETS0_PATCH_PATH + "Raw_0_" + mono2Info.pathID + ".dat", FileMode.Open, FileAccess.ReadWrite))
                {
                    // GameObject
                    fsMono2.Seek(0x00000004, SeekOrigin.Begin);
                    AssetInfo gameObjectInfo = Array.Find(AssetInfos, x => x.name == gameObjectName[i]);
                    byte[] byteGameObjectPathID = BitConverter.GetBytes(gameObjectInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteGameObjectPathID[j]);
                    }

                    // tk2FontData
                    fsMono2.Seek(0x00000014, SeekOrigin.Begin);
                    AssetInfo tk2FontDataInfo = Array.Find(AssetInfos, x => x.name == gameObjectName[i] + tk2dFontDataSuffix);
                    byte[] bytetk2FontDataPathID = BitConverter.GetBytes(tk2FontDataInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(bytetk2FontDataPathID[j]);
                    }

                    // Material
                    fsMono2.Seek(0x0002C0B4, SeekOrigin.Begin);
                    string temp = gameObjectName[i].Replace("data", "") + "material";
                    AssetInfo materialInfo = Array.Find(AssetInfos, x => x.name == temp);
                    byte[] byteMaterialPathID = BitConverter.GetBytes(materialInfo.pathID);
                    for (int j = 0; j < 4; j++)
                    {
                        fsMono2.WriteByte(byteMaterialPathID[j]);
                    }
                }
            }
            using (StreamWriter swPatchList = new StreamWriter(SHAREDASSETS0_PATCH_PATH + SHAREDASSETS0_PATCH_NAME + "_list.txt"))
            {
                for(int i=0; i< PatchList.Count;i++)
                {
                    if (i != PatchList.Count-1)
                    {
                        swPatchList.WriteLine(PatchList[i]);
                    }
                    else
                    {
                        swPatchList.Write(PatchList[i]);
                    }
                    
                }
                
            }

            //res
            CreateFolderOrClean(RESOURCE_PATCH_PATH);
            AssetInfo languageDataInfo = AssetInfos[0];
            byte[] preData = { 0x11, 0x00, 0x00, 0x00, 0x4C, 0x61, 0x6E, 0x67, 0x75, 0x61, 0x67, 0x65, 0x44, 0x61, 0x74, 0x61, 0x2E, 0x6A, 0x73, 0x6F, 0x6E, 0x00, 0x00, 0x00 };
            using (FileStream fsLanguageData = new FileStream(RESOURCE_PATCH_PATH + "Raw_0_" + languageDataInfo.pathID + ".dat", FileMode.Create, FileAccess.ReadWrite))
            {
                for (int i = 0; i < preData.Length; i++)
                {
                    fsLanguageData.WriteByte(preData[i]);
                }
                using (FileStream fsDownloaded = new FileStream(TEMP_FOLDER_NAME + "LanguageData.json.txt", FileMode.Open, FileAccess.Read))
                {
                    byte[] fsDownloadedSize = BitConverter.GetBytes(fsDownloaded.Length);
                    for(int i=0;i<4;i++)
                    {
                        fsLanguageData.WriteByte(fsDownloadedSize[i]);
                    }
                    for (int i = 0; i < fsDownloaded.Length; i++)
                    {
                        fsLanguageData.WriteByte((byte)fsDownloaded.ReadByte());
                    }
                }
                while(fsLanguageData.Length%4 ==0)
                {
                    fsLanguageData.WriteByte((byte)0);
                }
                for(int i=0; i<4; i++)
                {
                    fsLanguageData.WriteByte((byte)0);
                }
            }

            using (StreamWriter swPatchList = new StreamWriter(RESOURCE_PATCH_PATH + RESOURCE_PATCH_NAME + "_list.txt"))
            {
                swPatchList.Write(RESOURCE_PATCH_PATH + "Raw_0_" + languageDataInfo.pathID + ".dat");
            }


        }

        //static BomberCrewKoreanPatcherManagedCpp.AssetInfo FindAssetFromName(BomberCrewKoreanPatcherManagedCpp.AssetInfo[] AssetInfos, string name)
        //{
        //    AssetInfos.
        //}
    }
}
