// stdafx.h : 자주 사용하지만 자주 변경되지는 않는
// 표준 시스템 포함 파일 또는 프로젝트 관련 포함 파일이
// 들어 있는 포함 파일입니다.
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // 거의 사용되지 않는 내용은 Windows 헤더에서 제외합니다.
#define __assert(exp,msg) if(exp){MessageBoxA(NULL,msg,"Error",16);ExitProcess(E_OUTOFMEMORY);}
#define __checkoutofmemory(exp) __assert(exp,"Out of memory!")


// TODO: 프로그램에 필요한 추가 헤더는 여기에서 참조합니다.
