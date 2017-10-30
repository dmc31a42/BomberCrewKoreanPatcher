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
		// TODO: ���⿡ �� Ŭ������ ���� �޼��带 �߰��մϴ�.
	public:
		ManagedPatcher(String^ gameFolderPath, String^ currentDirectory);
		~ManagedPatcher();
		array<AssetInfo^>^ GetAssetInfos();
		void MakeModdedAssets();
	};


}
