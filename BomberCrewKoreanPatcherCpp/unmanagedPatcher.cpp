// defined _ITERATOR_DEBUG_LEVEL, _CRT_SECURE_NO_WARNINGS for UABE_API
#define _ITERATOR_DEBUG_LEVEL 0
#define _CRT_SECURE_NO_WARNINGS

#include "stdafx.h"
#include "unmanagedPatcher.h"
#include <iostream>
#include <vector>
#include <map>

#include "AssetsTools\defines.h"
#include "AssetsTools\AssetsFileFormat.h"
#include "AssetsTools\AssetsFileReader.h"
#include "AssetsTools\AssetsFileTable.h"
#include "AssetsTools\AssetTypeClass.h"
#include "AssetsTools\AssetsBundleFileFormat.h"
#include "AssetsTools\ClassDatabaseFile.h"

//#define MY_DEBUG
using namespace std;

void TextAsset_FreeMemoryResource(void *pResource)
{
	free(pResource);
}

int filesize(FILE* file)
{
	fseek(file, 0L, SEEK_END);
	int fileLen = ftell(file);
	fseek(file, 0L, SEEK_SET);
	return fileLen;
}

int FindPathID(string str)
{
	std::size_t foundUnderBar = str.find_last_of('_');
	std::size_t foundDot = str.find_last_of('.');
	string ID_string = str.substr(foundUnderBar + 1, foundDot - foundUnderBar - 1);
	return std::stoi(ID_string);
}

unmanagedPatcher::unmanagedPatcher(string gameFolderPath, string currentDirectory)
{
	_gameFolderPath = gameFolderPath;
	_currentDirectory = currentDirectory;
	resAssetsFileName = "resources.assets";
	sharedAssetsFileName = "sharedassets0.assets";
	classDatabaseFileName = "Resource\\U5.6.0f3.dat";
	
#ifdef MY_DEBUG
	cout << "_gameFolderPath : " << _gameFolderPath << endl;
	cout << "_currentDirectory : " << _currentDirectory << endl;
	cout << "(_gameFolderPath + resAssetsFileName).c_str() : " << (_gameFolderPath + resAssetsFileName).c_str() << endl;
	cout << "(_gameFolderPath + sharedAssetsFileName).c_str() : " << (_gameFolderPath + sharedAssetsFileName).c_str() << endl;
	cout << "(_currentDirectory + classDatabaseFileName).c_str() : " << (_currentDirectory + classDatabaseFileName).c_str() << endl;
#endif

	/*pResAssetsFile = fopen((_gameFolderPath + resAssetsFileName).c_str(), "rb");
	psharedAssetsFile = fopen((_gameFolderPath + sharedAssetsFileName).c_str(), "rb");
	pClassDatabaseFile = fopen((_currentDirectory + classDatabaseFileName).c_str(), "rb");*/
	fopen_s(&pResAssetsFile, (_gameFolderPath + resAssetsFileName).c_str(), "rb");
	fopen_s(&psharedAssetsFile, (_gameFolderPath + sharedAssetsFileName).c_str(), "rb");
	fopen_s(&pClassDatabaseFile, (_currentDirectory + classDatabaseFileName).c_str(), "rb");

#ifdef MY_DEBUG
	//cout << pResAssetsFile->
#endif

	resAssetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)pResAssetsFile);
	sharedAssetsFile = new AssetsFile(AssetsReaderFromFile, (LPARAM)psharedAssetsFile);
	classDatabaseFile = new ClassDatabaseFile();
	classDatabaseFile->Read(AssetsReaderFromFile, (LPARAM)pClassDatabaseFile);

	resAssetsFileTable = new AssetsFileTable(resAssetsFile);
	sharedAssetsFileTable = new AssetsFileTable(sharedAssetsFile);

	//findByClassID = new map<int, unsigned int>();
	for (int i = 0; i < classDatabaseFile->classes.size(); i++)
	{
		int classId = classDatabaseFile->classes[i].classId;
		findByClassID.insert(map<int, unsigned int>::value_type(classId, i));
	}
	
	FindInformation();
}

unmanagedPatcher::~unmanagedPatcher()
{
	if (resAssetsFileTable)
	{
		delete resAssetsFileTable;
		resAssetsFileTable = NULL;
	}
	if (sharedAssetsFileTable)
	{
		delete sharedAssetsFileTable;
		sharedAssetsFileTable = NULL;
	}

	if (classDatabaseFile)
	{
		delete classDatabaseFile;
		classDatabaseFile = NULL;
	}
	if (sharedAssetsFile)
	{
		delete sharedAssetsFile;
		sharedAssetsFile = NULL;
	}
	if (resAssetsFile)
	{
		delete resAssetsFile;
		resAssetsFile = NULL;
	}

	if (pResAssetsFile)
	{
		fclose(pResAssetsFile);
		pResAssetsFile = NULL;
	}
	if (psharedAssetsFile)
	{
		fclose(psharedAssetsFile);
		psharedAssetsFile = NULL;
	}
	if (pClassDatabaseFile)
	{
		fclose(pClassDatabaseFile);
		pClassDatabaseFile = NULL;
	}
}

void unmanagedPatcher::FindInformation()
{
	vector<string> materialNames;
	vector<string> gameObjectNames;
	string languageDataName = "LanguageData.json";

	materialNames.push_back("UILargematerial");
	materialNames.push_back("UIMediummaterial");
	materialNames.push_back("UISmallmaterial");

	gameObjectNames.push_back("UILargedata");
	gameObjectNames.push_back("UISmalldata");
	gameObjectNames.push_back("UIMediumdata");

	int currentPathID = 1;
	for (; currentPathID <= resAssetsFileTable->assetFileInfoCount; currentPathID++)
	{
		AssetFileInfoEx *tempAssetFileInfoEx = resAssetsFileTable->getAssetInfo(currentPathID);
		if (languageDataName == tempAssetFileInfoEx->name)
		{
			UnmanagedAssetInfo tempAssetInfo;
			tempAssetInfo.pathID = currentPathID;
			tempAssetInfo.name = tempAssetFileInfoEx->name;
			tempAssetInfo.offset = tempAssetFileInfoEx->absolutePos;
			tempAssetInfo.size = tempAssetFileInfoEx->curFileSize;
			assetInfos.push_back(tempAssetInfo);
			break;
		}
	}

	int fountAssetCount = 0;
	currentPathID = 1;
	for (; currentPathID <= sharedAssetsFileTable->assetFileInfoCount; currentPathID++)
	{
		if (materialNames.size() == 0)
		{
			break;
		}
		AssetFileInfoEx *tempAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(currentPathID);
#ifdef MY_DEBUG
		cout << "currentPathID : " << currentPathID << "->name : " << tempAssetFileInfoEx->name << endl;
#endif
		for (int i = 0; i < materialNames.size(); i++)
		{
			if (materialNames[i] == tempAssetFileInfoEx->name)
			{
				UnmanagedAssetInfo tempAssetInfo;
				tempAssetInfo.pathID = currentPathID;
				tempAssetInfo.name = tempAssetFileInfoEx->name;
				tempAssetInfo.offset = tempAssetFileInfoEx->absolutePos;
				tempAssetInfo.size = tempAssetFileInfoEx->curFileSize;
				assetInfos.push_back(tempAssetInfo);
				AssetTypeTemplateField *tempAssetTypeTemplateField = new AssetTypeTemplateField;
				tempAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[tempAssetFileInfoEx->curFileType]], (DWORD)0);
				AssetTypeInstance tempAssetTypeInstance((DWORD)1, &tempAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)psharedAssetsFile, (bool)sharedAssetsFile->header.endianness, tempAssetFileInfoEx->absolutePos);
				AssetTypeValueField *pBase = tempAssetTypeInstance.GetBaseField();
				
				if (pBase) {
					AssetTypeValueField *pShader = pBase->Get("m_Shader")->Get("m_PathID");
					AssetTypeValueField *pAtlas = pBase->Get("m_SavedProperties")->Get("m_TexEnvs")->Get("Array")->Get((unsigned int)0)->Get("second")->Get("m_Texture")->Get("m_PathID");
					//->Get("m_TexEnvs")->Get("Array")->Get((unsigned int)0)->Get("data")->Get("second")->Get("m_Texture")->Get("m_PathID");

				// 만약 한번에 필드를 못얻겠으면 순차적으로 확인해볼것
				//AssetTypeValueField *pAtlas = pBase->Get("m_SavedProperties");
				// //->Get("m_TexEnvs")->Get("Array")->Get((unsigned int)0)->Get("data")->Get("second")->Get("m_Texture")->Get("m_PathID");
#ifdef MY_DEBUG
					bool pShaderExist = (bool)pShader;
					bool pAtlasExist = (bool)pAtlas;
					bool pShaderIsDummy = pShader->IsDummy();
					bool pAtlasIsDummy = pAtlas->IsDummy();
					cout << "[" << i << "]" << "pShaderExist : " << pShaderExist << endl;
					cout << "[" << i << "]" << "pAtlasExist : " << pAtlasExist << endl;
					cout << "[" << i << "]" << "pShaderIsDummy : " << pShaderIsDummy << endl;
					cout << "[" << i << "]" << "pAtlasIsDummy : " << pAtlasIsDummy << endl;
#endif
					if (pShader && pAtlas && !pShader->IsDummy() && !pAtlas->IsDummy())
					{
						int shaderPathID = pShader->GetValue()->AsInt();
						int atlasPathID = pAtlas->GetValue()->AsInt();

						AssetFileInfoEx *shaderAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(shaderPathID);
						UnmanagedAssetInfo shaderAssetInfo;
						shaderAssetInfo.pathID = shaderPathID;
						shaderAssetInfo.name = materialNames[i] + "_Shader";
						shaderAssetInfo.offset = shaderAssetFileInfoEx->absolutePos;
						shaderAssetInfo.size = shaderAssetFileInfoEx->curFileSize;
						assetInfos.push_back(shaderAssetInfo);

						AssetFileInfoEx *atlasAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(atlasPathID);
						UnmanagedAssetInfo atlasAssetInfo;
						atlasAssetInfo.pathID = atlasPathID;
						atlasAssetInfo.name = materialNames[i] + "_Atlas";
						shaderAssetInfo.offset = atlasAssetFileInfoEx->absolutePos;
						atlasAssetInfo.size = atlasAssetFileInfoEx->curFileSize;
						assetInfos.push_back(atlasAssetInfo);

						materialNames.erase(materialNames.begin() + i);
						break;
					}					
				}
			}
		}
		// Almost Same as upper
		/*AssetTypeTemplateField *tempAssetTypeTemplateField = new AssetTypeTemplateField;
		tempAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[tempAssetFileInfoEx->curFileType]], (DWORD)0);
		AssetTypeInstance tempAssetTypeInstance((DWORD)1, &tempAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)psharedAssetsFile, (bool)sharedAssetsFile->header.endianness, tempAssetFileInfoEx->absolutePos);
		AssetTypeValueField *pBase = tempAssetTypeInstance.GetBaseField();
		if (pBase)
		{
			AssetTypeValueField *m_Name = pBase->Get("m_Name");
			if (m_Name && m_Name->IsDummy() == false)
			{
				string tempName = m_Name->GetValue()->AsString();
				for (int i = 0; i < materialNames.size(); i++)
				{
					if (materialNames[i] == tempName)
					{
						UnmanagedAssetInfo tempAssetInfo;
						tempAssetInfo.pathID = currentPathID;
						tempAssetInfo.name = tempName;
						tempAssetInfo.offset = tempAssetFileInfoEx->absolutePos;
						tempAssetInfo.size = tempAssetFileInfoEx->curFileSize;
						assetInfos.push_back(tempAssetInfo);

						AssetTypeValueField *pShader = pBase->Get("m_Shader")->Get("m_PathID");
						AssetTypeValueField *pAtlas = pBase->Get("m_SavedProperties")->Get("m_TextEnvs")->Get("Array")->Get((unsigned int)0)->Get("data")->Get("Second")->Get("m_Texture")->Get("m_PathID");
						if (pShader && pAtlas && !pShader->IsDummy() && !pAtlas->IsDummy())
						{
							int shaderPathID = pShader->GetValue()->AsInt();
							int atlasPathID = pAtlas->GetValue()->AsInt();
						}
					}
				}
			}
		}*/

	}

	for (; currentPathID <= sharedAssetsFileTable->assetFileInfoCount; currentPathID++)
	{
		if (gameObjectNames.size() == 0)
		{
			break;
		}
		AssetFileInfoEx *tempAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(currentPathID);
		AssetTypeTemplateField *tempAssetTypeTemplateField = new AssetTypeTemplateField;
		tempAssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[tempAssetFileInfoEx->curFileType]], (DWORD)0);
		AssetTypeInstance tempAssetTypeInstance((DWORD)1, &tempAssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)psharedAssetsFile, (bool)sharedAssetsFile->header.endianness, tempAssetFileInfoEx->absolutePos);
		AssetTypeValueField *pBase = tempAssetTypeInstance.GetBaseField();
		if (pBase)
		{
			AssetTypeValueField *pm_Name = pBase->Get("m_Name");
			if (pm_Name && pm_Name->IsDummy() == false)
			{
				string m_Name = pm_Name->GetValue()->AsString();
#ifdef MY_DEBUG
				cout << "[PathID : " << currentPathID << "] : " << m_Name << endl;
#endif
				for (int i = 0; i < gameObjectNames.size(); i++)
				{
					if (gameObjectNames[i] == m_Name)
					{
						UnmanagedAssetInfo tempAssetInfo;
						tempAssetInfo.pathID = currentPathID;
						tempAssetInfo.name = gameObjectNames[i];
						tempAssetInfo.offset = tempAssetFileInfoEx->absolutePos;
						tempAssetInfo.size = tempAssetFileInfoEx->curFileSize;
						assetInfos.push_back(tempAssetInfo);

						AssetTypeValueField *pTransform = pBase->Get("m_Component")->Get("Array")->Get((unsigned int)0)->Get("component")->Get("m_PathID");
						AssetTypeValueField *pMonoBehaviour2 = pBase->Get("m_Component")->Get("Array")->Get((unsigned int)1)->Get("component")->Get("m_PathID");
						if (pTransform && pMonoBehaviour2 && pTransform->IsDummy() == false && pMonoBehaviour2->IsDummy() == false)
						{
							int transformPathID = pTransform->GetValue()->AsInt();
							int monoBehaviour2PathID = pMonoBehaviour2->GetValue()->AsInt();

							AssetFileInfoEx *transformAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(transformPathID);
							UnmanagedAssetInfo transformAssetInfo;
							transformAssetInfo.pathID = transformPathID;
							transformAssetInfo.name = gameObjectNames[i] + "_Transform";
							transformAssetInfo.offset = transformAssetFileInfoEx->absolutePos;
							transformAssetInfo.size = transformAssetFileInfoEx->curFileSize;
							assetInfos.push_back(transformAssetInfo);

							AssetFileInfoEx *monoBehaviour2AssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(monoBehaviour2PathID);
							UnmanagedAssetInfo monoBehaviour2AssetInfo;
							monoBehaviour2AssetInfo.pathID = monoBehaviour2PathID;
							monoBehaviour2AssetInfo.name = gameObjectNames[i] + "_MonoBehaviour2";
							monoBehaviour2AssetInfo.offset = monoBehaviour2AssetFileInfoEx->absolutePos;
							monoBehaviour2AssetInfo.size = monoBehaviour2AssetFileInfoEx->curFileSize;
							assetInfos.push_back(monoBehaviour2AssetInfo);

							AssetTypeTemplateField *monoBehaviour2AssetTypeTemplateField = new AssetTypeTemplateField;
							monoBehaviour2AssetTypeTemplateField->FromClassDatabase(classDatabaseFile, &classDatabaseFile->classes[findByClassID[0x00000072]], (DWORD)0);
							AssetTypeInstance monoBehaviour2AssetTypeInstance((DWORD)1, &monoBehaviour2AssetTypeTemplateField, AssetsReaderFromFile, (LPARAM)psharedAssetsFile, (bool)sharedAssetsFile->header.endianness, monoBehaviour2AssetFileInfoEx->absolutePos);
							AssetTypeValueField *pMonoBehaviour2Base = monoBehaviour2AssetTypeInstance.GetBaseField();

							if (pMonoBehaviour2Base)
							{
								AssetTypeValueField *ptk2FontData = pMonoBehaviour2Base->Get("m_Script")->Get("m_PathID");
								if (ptk2FontData && ptk2FontData->IsDummy() == false)
								{
									int tk2FontDataPathID = ptk2FontData->GetValue()->AsInt();
									AssetFileInfoEx *tk2FontDataAssetFileInfoEx = sharedAssetsFileTable->getAssetInfo(tk2FontDataPathID);
									UnmanagedAssetInfo tk2FontDataAssetInfo;
									tk2FontDataAssetInfo.pathID = tk2FontDataPathID;
									tk2FontDataAssetInfo.name = gameObjectNames[i] + "_tk2FontData";
									tk2FontDataAssetInfo.offset = tk2FontDataAssetFileInfoEx->absolutePos;
									tk2FontDataAssetInfo.size = tk2FontDataAssetFileInfoEx->curFileSize;
									assetInfos.push_back(tk2FontDataAssetInfo);

									gameObjectNames.erase(gameObjectNames.begin() + i);
									break;
								}
							}
						}
					}
				}
			}
		}
	}
}

vector<UnmanagedAssetInfo> unmanagedPatcher::GetAssetInfos()
{
	return this->assetInfos;
}

void unmanagedPatcher::MakeModdedAssets()
{
	// sharedPatch
	string sharedPatchListFilePath = "temp\\sharedassets0_patch\\sharedassets0_patch_list.txt";

	ifstream ifsSharedPatchListFile(sharedPatchListFilePath);
	std::vector<string> sharedPatchFileName;
	if (!ifsSharedPatchListFile.is_open())
	{
		cout << "cannot open patchFileList text file" << endl;
		cout << "Exit" << endl;
		return;
	}
#ifdef MY_DEBUG
	cout << "Patch File List : " << endl;
#endif
	std::vector<FILE*> pSharedPatchFile;
	while (!ifsSharedPatchListFile.eof())
	{
		string temp;
		FILE *pTempPatchFile = NULL;
		ifsSharedPatchListFile >> temp;
		if (temp == "")
		{
			continue;
		}
		sharedPatchFileName.push_back(temp);
#ifdef MY_DEBUG
		cout << sharedPatchFileName[sharedPatchFileName.size() - 1] << endl;
#endif
		pTempPatchFile = fopen(temp.c_str(), "rb");
		if (pTempPatchFile == NULL)
		{
			cout << "cannot open patch file : " << temp << endl;
			cout << "Exit" << endl;
			fclose(pTempPatchFile);
			for (int i = 0; i < pSharedPatchFile.size(); i++)
			{
				fclose(pSharedPatchFile[i]);
			}
			return;
		}
		pSharedPatchFile.push_back(pTempPatchFile);
	}
	ifsSharedPatchListFile.close();

	string sharedModdedFilePath = _gameFolderPath + sharedAssetsFileName + ".modded";
	std::vector<AssetsReplacer*> sharedReplacors;
	std::vector<AssetFileInfoEx*> sharedAssetsFileInfos;

	for (unsigned int i = 0; i < sharedPatchFileName.size(); i++)
	{
		int tempPathID = FindPathID(sharedPatchFileName[i]);
		sharedAssetsFileInfos.push_back(sharedAssetsFileTable->getAssetInfo(tempPathID)); // I know the ID - no need to search
		sharedReplacors.push_back(MakeAssetModifierFromFile(0, (*sharedAssetsFileInfos[i]).index, (*sharedAssetsFileInfos[i]).curFileType, (*sharedAssetsFileInfos[i]).inheritedUnityClass,
			pSharedPatchFile[i], 0, (QWORD)filesize(pSharedPatchFile[i]))); // I expect that the size parameter refers to the file size but I couldn't check this until now
	}
	FILE *pModdedSharedFile = fopen((sharedModdedFilePath).c_str(), "wb");
	sharedAssetsFile->Write(AssetsWriterToFile, (LPARAM)pModdedSharedFile, 0, sharedReplacors.data(), sharedReplacors.size(), 0);

	for (unsigned int i = 0; i < pSharedPatchFile.size(); i++)
	{
		if (pSharedPatchFile[i])
		{
			fclose(pSharedPatchFile[i]);
			pSharedPatchFile[i] = NULL;
		}
	}
	if (pModdedSharedFile)
	{
		fclose(pModdedSharedFile);
		pModdedSharedFile = NULL;
	}
	//////////////////////////////////////////////
	// resources
	//////////////////////////////////////////////
	string resPatchListFilePath = "temp\\sharedassets0_patch\\sharedassets0_patch_list.txt";

	ifstream ifsResPatchListFile(resPatchListFilePath);
	std::vector<string> resPatchFileName;
	if (!ifsResPatchListFile.is_open())
	{
		cout << "cannot open patchFileList text file" << endl;
		cout << "Exit" << endl;
		return;
	}
#ifdef MY_DEBUG
	cout << "Patch File List : " << endl;
#endif
	std::vector<FILE*> pResPatchFile;
	while (!ifsResPatchListFile.eof())
	{
		string temp;
		FILE *pTempPatchFile = NULL;
		ifsResPatchListFile >> temp;
		if (temp == "")
		{
			continue;
		}
		resPatchFileName.push_back(temp);
#ifdef MY_DEBUG
		cout << resPatchFileName[resPatchFileName.size() - 1] << endl;
#endif
		pTempPatchFile = fopen(temp.c_str(), "rb");
		if (pTempPatchFile == NULL)
		{
			cout << "cannot open patch file : " << temp << endl;
			cout << "Exit" << endl;
			fclose(pTempPatchFile);
			for (int i = 0; i < pResPatchFile.size(); i++)
			{
				fclose(pResPatchFile[i]);
			}
			return;
		}
		pResPatchFile.push_back(pTempPatchFile);
	}
	ifsResPatchListFile.close();

	string resModdedFilePath = _gameFolderPath + resAssetsFileName + ".modded";
	std::vector<AssetsReplacer*> resReplacors;
	std::vector<AssetFileInfoEx*> resAssetsFileInfos;

	for (unsigned int i = 0; i < resPatchFileName.size(); i++)
	{
		int tempPathID = FindPathID(resPatchFileName[i]);
		resAssetsFileInfos.push_back(resAssetsFileTable->getAssetInfo(tempPathID)); // I know the ID - no need to search
		resReplacors.push_back(MakeAssetModifierFromFile(0, (*resAssetsFileInfos[i]).index, (*resAssetsFileInfos[i]).curFileType, (*resAssetsFileInfos[i]).inheritedUnityClass,
			pResPatchFile[i], 0, (QWORD)filesize(pResPatchFile[i]))); // I expect that the size parameter refers to the file size but I couldn't check this until now
	}
	FILE *pModdedResFile = fopen((resModdedFilePath).c_str(), "wb");
	sharedAssetsFile->Write(AssetsWriterToFile, (LPARAM)pModdedResFile, 0, resReplacors.data(), resReplacors.size(), 0);

	for (unsigned int i = 0; i < pResPatchFile.size(); i++)
	{
		if (pResPatchFile[i])
		{
			fclose(pResPatchFile[i]);
			pResPatchFile[i] = NULL;
		}
	}
	if (pModdedResFile)
	{
		fclose(pModdedResFile);
		pModdedResFile = NULL;
	}


#ifdef MY_DEBUG
	cout << "Slime Rancher Korean Translation Patch Complete. Exit" << endl;
#endif
}

UnmanagedAssetInfo::UnmanagedAssetInfo()
{
	
}
UnmanagedAssetInfo::UnmanagedAssetInfo(int pathID, string name, int offset, int size) 
{
	this->pathID = pathID;
	this->name = name;
	this->offset = offset;
	this->size = size;
}