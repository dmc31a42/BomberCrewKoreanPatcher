using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using BomberCrewKoreanPatcherManagedCpp;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

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
        const string currentVersion = "20171216";
        const string currentVersionURL = @"https://github.com/dmc31a42/BomberCrewKoreanPatcher/raw/master/currentVersion.txt";
        static string[] categorys =
                {
                    "Traits",
                    "Crewman Gear (NON-UI)",
                    "Tutorial",
                    "How To Play",
                    "Speech",
                    "UI",
                    "Bomber Upgrades (NON-UI)",
                    "Missions",
                    "MissionSpeech",
                    "BombLoads",
                    "Ranks",
                    "Skills"
            };

        static void Main(string[] args)
        {
            switch(args.Length)
            {
                case 0:
                    {
                        Console.WriteLine(GAME_NAME + " 게임 폴더 위치 찾는중...");
                        BomberCrewPath = FindUnityFolderPath();
                        if (BomberCrewPath == "ERROR")
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
                        Console.WriteLine("임시폴더 비우는중");
                        if (CreateFolderOrClean(TEMP_FOLDER_NAME) == false)
                        {
                            Console.WriteLine(TEMP_FOLDER_NAME + " 폴더를 삭제할 수 없습니다.");
                            Console.WriteLine("직접 삭제하신 후 다시 실행하여주세요");
                            Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                            Console.Read();
                            return;
                        }
                        Console.WriteLine("온라인상에서 최신버전이 있는지 확인하는 중...");
                        if (CheckLastVersion() == false)
                        {
                            Console.WriteLine("온라인상에 업데이트된 최신버전이 존재합니다.");
                            Console.Write("그래도 계속 진행하겠습니까?(Y/n) : ");
                            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                            if (consoleKeyInfo.KeyChar == 'Y')
                            {
                                Console.WriteLine("");
                            }
                            else
                            {
                                Console.WriteLine("\n인터넷에서 새로운 패치를 다운받아주세요. 패치를 중단합니다.");
                                Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                                Console.Read();
                                return;
                            }
                        }
                        Console.WriteLine("에셋 정보 추출중...");
                        BomberCrewKoreanPatcherManagedCpp.ManagedPatcher managedPatcher
                            = new BomberCrewKoreanPatcherManagedCpp.ManagedPatcher(BomberCrewPath, currentDirectoryPath);
                        BomberCrewKoreanPatcherManagedCpp.AssetInfo[] AssetInfos = managedPatcher.GetAssetInfos();
                        Console.WriteLine("번역된 문장 다운로드 및 적용중...");
                        DownloadWebTranslatorNPatch();
                        Console.WriteLine("패치 파일 생성중...");
                        MakeAssetFile(AssetInfos);
                        Console.WriteLine("패치된 유니티 에셋 생성중...");
                        managedPatcher.MakeModdedAssets();
                        managedPatcher.Dispose();
                        Console.WriteLine("유니티 에셋 원본과 패치된 파일 교체중...");
                        SwitchFile();
                        Console.WriteLine("한글 폰트 이미지 파일 복사중...");
                        File.Copy(RESOURCE_FOLDER_PATH + @"koreanAtlas.assets.resS", BomberCrewPath + @"koreanAtlas.assets.resS", true);
                        Console.WriteLine("한글패치 완료!");
                        Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오."); ;
                        Console.Read();
                    }
                    break;
                case 1:
                    {
                        switch (args[0])
                        {
                            case "-Update":
                                Console.WriteLine(GAME_NAME + " 게임 폴더 위치 찾는중...");
                                BomberCrewPath = FindUnityFolderPath();
                                if (BomberCrewPath == "ERROR")
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
                                Console.WriteLine("임시폴더 비우는중");
                                CreateFolderOrClean(TEMP_FOLDER_NAME);
                                currentDirectoryPath = Directory.GetCurrentDirectory() + @"\";
                                Console.WriteLine("에셋 정보 추출중...");
                                BomberCrewKoreanPatcherManagedCpp.ManagedPatcher managedPatcher
                                    = new BomberCrewKoreanPatcherManagedCpp.ManagedPatcher(BomberCrewPath, currentDirectoryPath);
                                BomberCrewKoreanPatcherManagedCpp.AssetInfo[] AssetInfos = managedPatcher.GetAssetInfos();
                                Console.WriteLine("번역된 문장 다운로드 및 적용중...");
                                DownloadWebTranslationNUpdate();
                                Console.WriteLine("프로그램이 종료되면 임시로 생성된 파일들이 지워집니다.");
                                Console.WriteLine("종료하려면 창을 끄거나 아무 키나 누르시오.");
                                Console.Read();
                                Console.WriteLine("임시로 생성된 파일 삭제 중...");
                                DeleteFolder(currentDirectoryPath + TEMP_FOLDER_NAME);
                                break;
                        }
                        break;
                    }
            }
        }

        static void DeleteFolder(string folderName)
        {
            DirectoryInfo di = new DirectoryInfo(folderName);
            if (di.Exists == true)
            {
                Directory.Delete(folderName, true);
            }
        }

        static bool CheckLastVersion()
        {
            string currentVersionFilePath = TEMP_FOLDER_NAME + "currentVersion.txt";
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile(currentVersionURL, currentVersionFilePath);
                string onlineVersion = System.IO.File.ReadAllText(currentVersionFilePath);
                if (onlineVersion == currentVersion)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("최신버전을 확인하는 도중 오류가 발생하였습니다. 현재 버전으로 패치를 계속합니다.");
                return true;
            }
        }

        private static void MakeUpdateText(string originalTXTPath, string translatedPoPath, string newTranslatedPoPath)
        {
            
            string[] categorys =
                {
                    "Traits",
                    "Crewman Gear (NON-UI)",
                    "Tutorial",
                    "How To Play",
                    "Speech",
                    "UI",
                    "Bomber Upgrades (NON-UI)",
                    "Missions",
                    "MissionSpeech",
                    "BombLoads",
                    "Ranks",
                    "Skills"
            };
            string originalTextString = System.IO.File.ReadAllText(originalTXTPath);
            string downloadedPoString = System.IO.File.ReadAllText(translatedPoPath);
            Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
            for(int i=0; i<categorys.Length; i++)
            {
                dictionary.Add(categorys[i], new Dictionary<string, string>());
            }

        }

        static List<string> DownloadWebFileOrCopyOfflineFile(string URLListTXTPath)
        {
            string CSVURLString = System.IO.File.ReadAllText(URLListTXTPath);
            CSVURLString = CSVURLString.Replace("\r", "");
            string[] CSVURL = CSVURLString.Split('\n');
            List<string[]> ListFileNameAndURL = new List<string[]>();
            List<string> ListFileName = new List<string>();
            List<string> ListFileURL = new List<string>();
            for (int i = 0; i < CSVURL.Length; i++)
            {
                if (CSVURL[i] != string.Empty)
                {
                    string[] tempNAMEURL = CSVURL[i].Split('\\');
                    ListFileNameAndURL.Add(tempNAMEURL);
                    ListFileName.Add(tempNAMEURL[0]);
                    ListFileURL.Add(tempNAMEURL[1]);
                }
            }
            foreach (string[] NameAndURL in ListFileNameAndURL)
            {
                DownloadWebFileOrCopyOfflineFile(NameAndURL[0], NameAndURL[1], ".po");
            }
            return ListFileName;
        }
        static void DownloadWebFileOrCopyOfflineFile(string name, string url, string extension)
        {
            if (extension != string.Empty & extension[0] != '.')
            {
                extension = "." + extension;
            }
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile(url, TEMP_FOLDER_NAME + "\\" + name + extension);
            }
            catch
            {
                Console.WriteLine(name + "      \t번역 파일을 받는 중 오류가 발생하였습니다. 오프라인 파일을 사용합니다.");
                File.Copy(RESOURCE_FOLDER_PATH + name + ".po", TEMP_FOLDER_NAME + name + extension, true);
            }
        }

        static Dictionary<string, string> LoadFromPo(string translatedPoPath)
        {
            Dictionary<string, string> translatedPoDictionary = new Dictionary<string, string>();
            string translatedPoString = System.IO.File.ReadAllText(translatedPoPath);
            // https://regexr.com/3hnbt
            MatchCollection translatedPoMatchCollection = Regex.Matches(translatedPoString, "msgctxt \"(.*)\"\\nmsgid ([\\s\\S]*?)\\nmsgstr ([\\s\\S]+?)\"\\n\\n");
            foreach (Match match in translatedPoMatchCollection)
            {
                string key = match.Groups[1].Value.Replace("\"", string.Empty);
                string value = match.Groups[3].Value;
                value = Regex.Replace(value, "^\"|\"\n\"|\"$", string.Empty);
                value = value.Trim().Replace("\\n", "\n").Replace("\\\"", "\"").Trim();
                translatedPoDictionary.Add(key, value);
            }
            return translatedPoDictionary;
        }

        static void WriteToPo(JObject originalJSON)
        {
            foreach (string category in categorys)
            {
                WriteToPo(originalJSON, category, new Dictionary<string, string>());
            }
        }
        static void WriteToPo(JObject originalJSON, string category, Dictionary<string, string> translatedDictionary)
        {
            int i = 1;
            string potString = "";
            foreach (JToken token in originalJSON[category].Children())
            {
                string key = token["key"].Value<string>();
                string originalText = token["value"].Value<string>().Replace("\r",string.Empty);
                string translatedText = "";
                if(translatedDictionary.TryGetValue(key, out translatedText) == false)
                {
                    translatedText = "";
                }
                potString += "#: " + i++ + "\n" +
                    "msgctxt \"" + key +
                    "\"\nmsgid \"" +
                    originalText.Replace("\"", "\\\"").Replace("=", @"\=").Replace("\n", @"\n").Trim() +
                    "\"\n" +
                    "msgstr \"" +
                    translatedText.Replace("\"", "\\\"").Replace("=", @"\=").Replace("\n", @"\n").Trim() +
                    "\"\n\n";
            }
            System.IO.File.WriteAllText(TEMP_FOLDER_NAME + "\\" + category.Replace(" ", "_") + ".pot", potString);
            System.IO.File.WriteAllText(TEMP_FOLDER_NAME + "\\" + category.Replace(" ", "_") + "_updated.po", potString);
        }

        //static JObject PatchText(JObject originalJSON, List<string> ListName)
        //{

        //}
        static JObject PatchText(JObject originalJSON, List<string> listName)
        {
            foreach (string category in categorys)
            {
                Dictionary<string, string> tempTranslatedDictionary = LoadFromPo(TEMP_FOLDER_NAME + category + ".po");
                originalJSON = PatchText(originalJSON, category, tempTranslatedDictionary);
            }
            return originalJSON;
        }
        static JObject PatchText(JObject originalJSON, string category, Dictionary<string,string> translatedDictionary)
        {
            foreach (JToken token in originalJSON[category].Children())
            {
                string translatedText = "";
                if (translatedDictionary.TryGetValue(token["key"].Value<string>(), out translatedText) && translatedText != string.Empty)
                {
                    token["value"] = translatedText;
                }
            }
            return originalJSON;
        }

        static void DownloadWebTranslatorNPatch()
        {
            List<string> ListName = DownloadWebFileOrCopyOfflineFile(RESOURCE_FOLDER_PATH + @"CSVURL.txt");
            string originalText = System.IO.File.ReadAllText(TEMP_FOLDER_NAME + "LanguageData.json.txt");
            JObject originalJSON = JObject.Parse(originalText);
            JObject translatedJSON = PatchText(originalJSON, ListName);
            System.IO.File.WriteAllText(TEMP_FOLDER_NAME + "LanguageData.json.txt", translatedJSON.ToString());
        }

        static void DownloadWebTranslationNUpdate()
        {
            string originalText = System.IO.File.ReadAllText(TEMP_FOLDER_NAME + "LanguageData.json.txt");
            JObject originalJSON = JObject.Parse(originalText);
            List<string> ListName;
            try
            {
                ListName = DownloadWebFileOrCopyOfflineFile(RESOURCE_FOLDER_PATH + @"CSVURL.txt");
            }
            catch
            {
                WriteToPo(originalJSON);
                return;
            }
            foreach(string category in categorys)
            {
                WriteToPo(originalJSON, category, LoadFromPo(TEMP_FOLDER_NAME + category + ".po"));
            }
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
            if (FolderExist(@"..\" + GAME_NAME + @"_Data\"))
            {
                return @"..\" + GAME_NAME + @"_Data\";
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
            
            WebClient webClient = new WebClient();
            try
            {
                // 폴더없는 단일경로의 경우 앞에 .\ 무조건 붙여야함 아니면 예외뜸
                // 그리고 파일이 있으면 예외뜸
                webClient.DownloadFile(downloadURL, TEMP_FOLDER_NAME + "downloaded_" + downloadFileName);
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

        static bool CreateFolderOrClean(string folderName, int count = 10)
        {
            DirectoryInfo di = new DirectoryInfo(folderName);
            if (count == 0)
            {
                return false;
            }
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }
            else
            {
                try
                {
                    Directory.Delete(folderName, true);
                }
                catch
                {
                    Console.WriteLine(folderName + " 폴더 삭제 시도 " + count + "번 남음");
                }
                return CreateFolderOrClean(folderName, count - 1);
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

            string[] fontGroupNames =
            {
                "MonoBehaviour FontGroupUIMedium",
                "MonoBehaviour FontGroupUISmall"
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
                    if(gameObjectName[i] != "UILargedata")
                    {
                        // FontGroup
                        fsMono2.Seek(0x0002C940, SeekOrigin.Begin);
                        string temp1 = "MonoBehaviour FontGroup" + gameObjectName[i].Replace("data", "");
                        AssetInfo fontGroupInfo = Array.Find(AssetInfos, x => x.name == temp1);
                        byte[] byteFontGroupPathID = BitConverter.GetBytes(fontGroupInfo.pathID);
                        for (int j = 0; j < 4; j++)
                        {
                            fsMono2.WriteByte(byteFontGroupPathID[j]);
                        }
                    }

                    // Material
                    fsMono2.Seek(0x0002CE80, SeekOrigin.Begin);
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
                while(fsLanguageData.Length%4 !=0)
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
