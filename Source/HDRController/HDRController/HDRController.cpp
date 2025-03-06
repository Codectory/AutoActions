//source https://github.com/thexai/xbmc
/// https://github.com/thexai/xbmc/blob/a4bfd1332e8e363c58842dffbb81b0fca8d29cc8/xbmc/platform/win32/DisplayUtilsWin32.cpp

#define WIN32_LEAN_AND_MEAN      // Exclude rarely-used stuff from Windows headers

#include <windows.h>


#include "HDRController.h"
#include <iostream>
#include "WinUser.h"
#include <stdio.h>
#include <winerror.h>
#include <wingdi.h>
#include <stdexcept>
#include <string> 
#include <VersionHelpers.h>
#include <dwmapi.h>
#include <stdint.h>
#include <cstdlib>
#include <cstring>
#include <conio.h>
#include <vector>

// @todo: Remove this and "SDK_26100.h" when Windows SDK updated to 10.0.26100.0 in builders
#ifndef NTDDI_WIN11_GE // Windows SDK 10.0.26100.0 or newer
#include "SDK_26100.h"
#endif


using namespace core;

static void showError(std::string msg)
{
	LPCWSTR lmsg = (LPCWSTR)msg.c_str();

	MessageBox(NULL, lmsg, L"HDRController Error", MB_OK | MB_ICONWARNING);
}

typedef LONG NTSTATUS;

static bool sysGetVersionExWByRef(OSVERSIONINFOEXW& osVerInfo)
{
	osVerInfo.dwOSVersionInfoSize = sizeof(osVerInfo);
	typedef NTSTATUS(__stdcall* RtlGetVersionPtr)(RTL_OSVERSIONINFOEXW* pOsInfo);
	static HMODULE hNtDll = GetModuleHandleW(L"ntdll.dll");
	if (hNtDll != NULL)
	{
		static RtlGetVersionPtr RtlGetVer = (RtlGetVersionPtr)GetProcAddress(hNtDll, "RtlGetVersion");
		if (RtlGetVer && RtlGetVer(&osVerInfo) == 0)
			return true;
	}
	// failed to get OS information directly from ntdll.dll
	// use GetVersionExW() as fallback
	// note: starting from Windows 8.1 GetVersionExW() may return unfaithful information 
#pragma warning(disable : 4996)
	if (GetVersionExW((OSVERSIONINFOW*)&osVerInfo) != 0)
		return true;

	ZeroMemory(&osVerInfo, sizeof(osVerInfo));
	return false;
}

static bool UseNewApi()
{
	OSVERSIONINFOEXW current = {};
	sysGetVersionExWByRef(current);

	// Initialize the OSVERSIONINFOEX structure.

	if (current.dwMajorVersion > 10)
		return true;
	else if (current.dwMajorVersion == 10)
	{
		if (current.dwMinorVersion > 0)
			return true;
		else if (current.dwMinorVersion == 0)
		{
			if (current.dwBuildNumber >= 2600)
				return true;
		}
	}
	return false;
}

static DISPLAYCONFIG_MODE_INFO* GetAllDisplayIdentifiers()
{
	uint32_t pathCount, modeCount;

	uint8_t set[] = { 0x0A, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x14, 0x81, 0x00, 0x00,
					 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };

	uint8_t request[] = { 0x09, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x7C, 0x6F, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0xDB, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };

	if (ERROR_SUCCESS == GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount))
	{
		DISPLAYCONFIG_PATH_INFO* pathsArray = nullptr;
		DISPLAYCONFIG_MODE_INFO* identifiers = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeIdentifiers = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		identifiers = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeIdentifiers));


		if (pathsArray != nullptr && identifiers != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(identifiers, 0, sizeIdentifiers);

			LONG queryRet = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, identifiers, 0);
			std::free(pathsArray);
			return identifiers;
		}
	}
	

}

static DISPLAYCONFIG_MODE_INFO GetDisplayIdentifier(UINT32 uid)
{
	DISPLAYCONFIG_MODE_INFO* identifiers = GetAllDisplayIdentifiers();
	DISPLAYCONFIG_MODE_INFO identifier = {};
	for (int i = 0; i < sizeof(identifiers); i++)
	{

		if (identifiers[i].id == uid)
			identifier = identifiers[i];
	}
	std::free(identifiers);
	return identifier;

}

static UINT32 _GetUID(UINT32 id)
{

	SIZE resolution = SIZE();

	uint32_t pathCount, modeCount;

	if (ERROR_SUCCESS == GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount))
	{
		DISPLAYCONFIG_PATH_INFO* pathsArray = nullptr;
		DISPLAYCONFIG_MODE_INFO* identifiers = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeidentifiers = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		identifiers = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeidentifiers));

		if (pathsArray != nullptr && identifiers != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(identifiers, 0, sizeidentifiers);

			if (ERROR_SUCCESS == QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, identifiers, 0))
			{
				for (int i = 0; i < pathCount; i++)
				{
					try
					{
						if (pathsArray[i].sourceInfo.id == id)
						{
							return pathsArray[i].targetInfo.id;
						}
					}
					catch (const std::exception&)
					{

					}
				}
			}
			std::free(pathsArray);
			std::free(identifiers);
			return 0;
		}
	}
}

static void SetHDR(UINT32 uid, bool enabled)
{
	LONG result{ ERROR_SUCCESS };
	DISPLAYCONFIG_MODE_INFO identifier = GetDisplayIdentifier(uid);
	// Windows 11 24H2 or newer (SDK 10.0.26100.0)
	if (UseNewApi)
	{
		DISPLAYCONFIG_SET_HDR_STATE setHdrState = {};
		setHdrState.header.type =
			static_cast<DISPLAYCONFIG_DEVICE_INFO_TYPE>(DISPLAYCONFIG_DEVICE_INFO_SET_HDR_STATE);
		setHdrState.header.size = sizeof(setHdrState);
		setHdrState.header.adapterId = identifier.adapterId;
		setHdrState.header.id = identifier.id;
		setHdrState.enableHdr = enabled ? TRUE : FALSE;

		result = DisplayConfigSetDeviceInfo(&setHdrState.header);
	}
	else // older than Windows 11 24H2
	{
		DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE setColorState = {};
		setColorState.header.type = DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
		setColorState.header.size = sizeof(setColorState);
		setColorState.header.adapterId = identifier.adapterId;
		setColorState.header.id = identifier.id;
		setColorState.enableAdvancedColor = enabled ? TRUE : FALSE;

		result = DisplayConfigSetDeviceInfo(&setColorState.header);
	}
}

static bool GetHDRStatus(UINT32 uid)
{
	bool hdrSupported{ false };
	bool hdrEnabled{ false };
	DISPLAYCONFIG_MODE_INFO identifier = GetDisplayIdentifier(uid);
	// Windows 11 24H2 or newer (SDK 10.0.26100.0)
	if (UseNewApi)
	{
		DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2 getColorInfo2 = {};
		getColorInfo2.header.type = static_cast<DISPLAYCONFIG_DEVICE_INFO_TYPE>(
			DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO_2);
		getColorInfo2.header.size = sizeof(getColorInfo2);
		getColorInfo2.header.adapterId = identifier.adapterId;
		getColorInfo2.header.id = identifier.id;

		if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(&getColorInfo2.header))
		{
			if (getColorInfo2.activeColorMode == DISPLAYCONFIG_ADVANCED_COLOR_MODE_HDR)
				hdrEnabled = true;

			if (getColorInfo2.highDynamicRangeSupported == TRUE)
				hdrSupported = true;
		}
	}
	else // older than Windows 11 24H2
	{
		DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO getColorInfo = {};
		getColorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
		getColorInfo.header.size = sizeof(getColorInfo);
		getColorInfo.header.adapterId = identifier.adapterId;
		getColorInfo.header.id = identifier.id;

		if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(&getColorInfo.header))
		{
			// DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO documentation is lacking and field
			// names are confusing. Through experimentation and deductions from equivalent WinRT API:
			//
			// SDR screen, advanced color not supported (Win 10, Win 11 < 22H2)
			// > advancedColorSupported = 0 and wideColorEnforced = 0
			// SDR screen, advanced color is supported (Win 11 >= 22H2)
			// > advancedColorSupported = 1 and wideColorEnforced = 1
			// HDR screen
			// > advancedColorSupported = 1 and wideColorEnforced = 0
			//
			// advancedColorForceDisabled: maybe equivalent of advancedColorLimitedByPolicy?
			//
			// advancedColorEnabled = 1:
			// For HDR screens means HDR is on
			// For SDR screens means ACM (Automatic Color Management, Win 11 >= 22H2) is on

			//if (getColorInfo.advancedColorSupported && !getColorInfo.wideColorEnforced)
			//	hdrSupported = true;

			if (hdrSupported && getColorInfo.advancedColorEnabled)
				hdrEnabled = true;
		}
	}
	return hdrEnabled;
}


static void  SetGlobalHDR(bool enabled)
{
	DISPLAYCONFIG_MODE_INFO* identifiers = GetAllDisplayIdentifiers();

	for (int i = 0; i < sizeof(identifiers); i++)
	{
		try
		{
			SetHDR(identifiers[i].id, enabled);
		}
		catch (const std::exception&)
		{

		}
	}

	std::free(identifiers);
}


static UINT32 _GetColorDepth(UINT32 uid)
{

	DISPLAYCONFIG_MODE_INFO identifier = GetDisplayIdentifier(uid);
	uint8_t request[] = { 0x09, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x7C, 0x6F, 0x00,
				 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0xDB, 0x00,
				 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };
	DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacket =
		reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(request);


	try
	{
		if (identifier.infoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
		{
			requestPacket->adapterId.HighPart = identifier.adapterId.HighPart;
			requestPacket->adapterId.LowPart = identifier.adapterId.LowPart;
			requestPacket->id = identifier.id;
		}
		if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(requestPacket))
		{
			UINT32 number = request[28];
			return number;
		}
	}
	catch (const std::exception&)
	{

	}
	return 0;
}

static void _SetColorDepth(UINT32 uid, UINT32 colorDepth)
{
	DISPLAYCONFIG_MODE_INFO identifier = GetDisplayIdentifier(uid);


	uint8_t set[] = { 0x0A, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x14, 0x81, 0x00,
					  0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x01, 0x00,
					  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };

	uint8_t request[] = { 0x09, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x7C, 0x6F, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0xDB, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };


	DISPLAYCONFIG_DEVICE_INFO_HEADER* setPacket =
		reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(set);
	DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacket =
		reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(request);


	if (identifier.infoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
	{
		setPacket->adapterId.HighPart = identifier.adapterId.HighPart;
		setPacket->adapterId.LowPart = identifier.adapterId.LowPart;
		setPacket->id = identifier.id;

		requestPacket->adapterId.HighPart = identifier.adapterId.HighPart;
		requestPacket->adapterId.LowPart = identifier.adapterId.LowPart;
		requestPacket->id = identifier.id;

		if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(requestPacket))
		{
			set[28] = colorDepth;
			DisplayConfigSetDeviceInfo(setPacket);
		}
	}
}

static bool GetHDRStatus()
{
	bool returnValue = false;
	DISPLAYCONFIG_MODE_INFO* identifiers = GetAllDisplayIdentifiers();

	for (int i = 0; i < sizeof(identifiers); i++)
	{
		try
		{
			returnValue = GetHDRStatus(identifiers[i].id);
			if (returnValue)
			{
				break;
			}
		}
		catch (const std::exception&)
		{
		}
	}
	std::free(identifiers);
	return returnValue;
}

extern "C"
{
	__declspec(dllexport) void SetGlobalHDRState(bool enabled)
	{
		SetGlobalHDR(enabled);
	}

	__declspec(dllexport) bool GetGlobalHDRState()
	{
		return GetHDRStatus();
	}

	__declspec(dllexport) void SetHDRState(UINT32 uid, bool enabled)
	{
		SetHDR(uid, enabled);
	}

	__declspec(dllexport) bool GetHDRState(UINT32 uid)
	{
		return GetHDRStatus(uid);
	}

	__declspec(dllexport) UINT32 GetUID(UINT32 id)
	{
		return _GetUID(id);
	}

	__declspec(dllexport) UINT32 GetColorDepth(UINT32 uid)
	{
		return _GetColorDepth(uid);
	}

	__declspec(dllexport) void SetColorDepth(UINT32 uid, UINT32 colorDepth)
	{
		return _SetColorDepth(uid, colorDepth);
	}
}
