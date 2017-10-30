// BomberCrewKoreanPatcherManagedCpp.h

#pragma once
#include "..\BomberCrewKoreanPatcherCpp\unmanagedPatcher.h"
using namespace System;

namespace BomberCrewKoreanPatcherManagedCpp {

	public ref class AssetInfo {
	public:
		int pathID;
		String^ name;
		int offset;
		int size;
	};

	public ref class ManagedPatcher
	{
	private:

	protected:
		unmanagedPatcher *m_pUnmanagedPatcher;
		// TODO: 여기에 이 클래스에 대한 메서드를 추가합니다.
	public:
		ManagedPatcher(String^ gameFolderPath, String^ currentDirectory);
		~ManagedPatcher();
		array<AssetInfo^>^ GetAssetInfos();
		void MakeModdedAssets();
	};


}
