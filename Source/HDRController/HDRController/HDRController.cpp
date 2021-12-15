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

#include <stdint.h>
#include <cstdlib>
#include <cstring>
#include <conio.h>
#include <vector>


using namespace core;

static void showError(std::string msg)
{
	LPCWSTR lmsg = (LPCWSTR)msg.c_str();

	MessageBox(NULL, lmsg, L"HDRController Error", MB_OK | MB_ICONWARNING);
}




static void  SetHDR(UINT32 uid, bool enabled)
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
		DISPLAYCONFIG_MODE_INFO* modesArray = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeModesArray = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		modesArray = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeModesArray));


		if (pathsArray != nullptr && modesArray != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(modesArray, 0, sizeModesArray);

			LONG queryRet = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, modesArray, 0);
			if (ERROR_SUCCESS == queryRet)

			{



				DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO getColorInfo = {};
				getColorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
				getColorInfo.header.size = sizeof(getColorInfo);

				DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE setColorState = {};
				setColorState.header.type = DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
				setColorState.header.size = sizeof(setColorState);


				for (int i = 0; i < modeCount; i++)
				{
					try
					{
						if (modesArray[i].id != uid)
							continue;
						if (modesArray[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
						{
							DISPLAYCONFIG_MODE_INFO mode = modesArray[i];
							getColorInfo.header.adapterId.HighPart = mode.adapterId.HighPart;
							getColorInfo.header.adapterId.LowPart = mode.adapterId.LowPart;
							getColorInfo.header.id = mode.id;

							setColorState.header.adapterId.HighPart = mode.adapterId.HighPart;
							setColorState.header.adapterId.LowPart = mode.adapterId.LowPart;
							setColorState.header.id = mode.id;

							if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(&getColorInfo.header))
							{
								UINT32 value = enabled == true ? 1 : 0;
								if (value != getColorInfo.advancedColorEnabled)
								{
									setColorState.enableAdvancedColor = enabled;
									DisplayConfigSetDeviceInfo(&setColorState.header);
									break;
								}
							}

						}
					}
					catch (const std::exception)
					{

					}
				}

			}

			std::free(pathsArray);
			std::free(modesArray);
		}
		else
		{
			throw std::invalid_argument("No monitor found.");
		}


	}
}

static void  SetGlobalHDR(bool enabled)
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
		DISPLAYCONFIG_MODE_INFO* modesArray = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeModesArray = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		modesArray = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeModesArray));


		if (pathsArray != nullptr && modesArray != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(modesArray, 0, sizeModesArray);

			LONG queryRet = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, modesArray, 0);
			if (ERROR_SUCCESS == queryRet)
			{
				DISPLAYCONFIG_DEVICE_INFO_HEADER* setPacket =
					reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(set);
				DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacket =
					reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(request);

				for (int i = 0; i < modeCount; i++)
				{
					try
					{
						SetHDR(modesArray[i].id, enabled);
					}
					catch (const std::exception&)
					{

					}
				}
			}
			std::free(pathsArray);
			std::free(modesArray);
		}
		else
		{
			throw std::invalid_argument("No monitor found.");
		}
	}
}
static UINT32 _GetUID(UINT32 id)
{

	SIZE resolution = SIZE();

	uint32_t pathCount, modeCount;

	if (ERROR_SUCCESS == GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount))
	{
		DISPLAYCONFIG_PATH_INFO* pathsArray = nullptr;
		DISPLAYCONFIG_MODE_INFO* modesArray = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeModesArray = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		modesArray = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeModesArray));

		if (pathsArray != nullptr && modesArray != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(modesArray, 0, sizeModesArray);

			if (ERROR_SUCCESS == QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, modesArray, 0))
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
			std::free(modesArray);
			return 0;
		}
	}
}


static UINT32 _GetColorDepth(UINT32 uid)
{

	uint32_t pathCount, modeCount;

	uint8_t request[] = { 0x09, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x7C, 0x6F, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0xDB, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };

	if (ERROR_SUCCESS == GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount))
	{
		DISPLAYCONFIG_PATH_INFO* pathsArray = nullptr;
		DISPLAYCONFIG_MODE_INFO* modesArray = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeModesArray = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		modesArray = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeModesArray));

		if (pathsArray != nullptr && modesArray != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(modesArray, 0, sizeModesArray);

			if (ERROR_SUCCESS == QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, modesArray, 0))
			{
				DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacket =
					reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(request);

				for (int i = 0; i < modeCount; i++)
				{
					try
					{
						if (modesArray[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
						{
							requestPacket->adapterId.HighPart = modesArray[i].adapterId.HighPart;
							requestPacket->adapterId.LowPart = modesArray[i].adapterId.LowPart;
							requestPacket->id = modesArray[i].id;
						}
						if (modesArray[i].id != uid)
							continue;
						if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(requestPacket))
						{
							UINT32 number = request[28];
							return number;
						}
					}
					catch (const std::exception&)
					{

					}

				}
			}
			std::free(pathsArray);
			std::free(modesArray);
		}
	}
	return 0;

}

static void _SetColorDepth(UINT32 uid, UINT32 colorDepth)
{
	uint32_t pathCount, modeCount;


	uint8_t set[] = { 0x0A, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x14, 0x81, 0x00,
					  0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x01, 0x00,
					  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };

	uint8_t request[] = { 0x09, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x7C, 0x6F, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0xDB, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };

	if (ERROR_SUCCESS == GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount))
	{
		DISPLAYCONFIG_PATH_INFO* pathsArray = nullptr;
		DISPLAYCONFIG_MODE_INFO* modesArray = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeModesArray = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		modesArray = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeModesArray));


		if (pathsArray != nullptr && modesArray != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(modesArray, 0, sizeModesArray);

			LONG queryRet = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, modesArray, 0);
			if (ERROR_SUCCESS == queryRet)
			{
				DISPLAYCONFIG_DEVICE_INFO_HEADER* setPacket =
					reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(set);
				DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacket =
					reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(request);

				for (int i = 0; i < modeCount; i++)
				{
					try
					{
						if (modesArray[i].id != uid)
							continue;

						if (modesArray[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
						{
							setPacket->adapterId.HighPart = modesArray[i].adapterId.HighPart;
							setPacket->adapterId.LowPart = modesArray[i].adapterId.LowPart;
							setPacket->id = modesArray[i].id;

							requestPacket->adapterId.HighPart = modesArray[i].adapterId.HighPart;
							requestPacket->adapterId.LowPart = modesArray[i].adapterId.LowPart;
							requestPacket->id = modesArray[i].id;

							if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(requestPacket))
							{
								set[28] = colorDepth;
								DisplayConfigSetDeviceInfo(setPacket);
							}

						}
					}
					catch (const std::exception&)
					{
					}
				}
			}
			std::free(pathsArray);
			std::free(modesArray);
		}
		else
		{
			throw std::invalid_argument("No monitor found.");
		}
	}
}




static bool HDRIsOn(UINT32 uid)
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
		DISPLAYCONFIG_MODE_INFO* modesArray = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeModesArray = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		modesArray = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeModesArray));


		if (pathsArray != nullptr && modesArray != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(modesArray, 0, sizeModesArray);

			LONG queryRet = QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, modesArray, 0);
			if (ERROR_SUCCESS == queryRet)

			{



				DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO getColorInfo = {};
				getColorInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
				getColorInfo.header.size = sizeof(getColorInfo);

				DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE setColorState = {};
				setColorState.header.type = DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
				setColorState.header.size = sizeof(setColorState);


				for (int i = 0; i < modeCount; i++)
				{
					try
					{
						if (modesArray[i].id != uid)
							continue;
						if (modesArray[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
						{
							DISPLAYCONFIG_MODE_INFO mode = modesArray[i];
							getColorInfo.header.adapterId.HighPart = mode.adapterId.HighPart;
							getColorInfo.header.adapterId.LowPart = mode.adapterId.LowPart;
							getColorInfo.header.id = mode.id;

							setColorState.header.adapterId.HighPart = mode.adapterId.HighPart;
							setColorState.header.adapterId.LowPart = mode.adapterId.LowPart;
							setColorState.header.id = mode.id;

							if (ERROR_SUCCESS == DisplayConfigGetDeviceInfo(&getColorInfo.header))
							{

								return getColorInfo.advancedColorSupported == 1 && getColorInfo.advancedColorEnabled == 1;
							}

						}
					}
					catch (const std::exception)
					{

					}
				}

			}

			std::free(pathsArray);
			std::free(modesArray);
		}
		else
		{
			throw std::invalid_argument("No monitor found.");
		}
	}
	return false;
}


static bool HDRIsOn()
{
	bool returnValue = false;

	uint32_t pathCount, modeCount;

	uint8_t set[] = { 0x0A, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x14, 0x81, 0x00, 0x00,
					 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };

	uint8_t request[] = { 0x09, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x7C, 0x6F, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x00, 0x00, 0xDB, 0x00,
						 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00 };

	if (ERROR_SUCCESS == GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount))
	{
		DISPLAYCONFIG_PATH_INFO* pathsArray = nullptr;
		DISPLAYCONFIG_MODE_INFO* modesArray = nullptr;

		const size_t sizePathsArray = pathCount * sizeof(DISPLAYCONFIG_PATH_INFO);
		const size_t sizeModesArray = modeCount * sizeof(DISPLAYCONFIG_MODE_INFO);

		pathsArray = static_cast<DISPLAYCONFIG_PATH_INFO*>(std::malloc(sizePathsArray));
		modesArray = static_cast<DISPLAYCONFIG_MODE_INFO*>(std::malloc(sizeModesArray));

		if (pathsArray != nullptr && modesArray != nullptr)
		{
			std::memset(pathsArray, 0, sizePathsArray);
			std::memset(modesArray, 0, sizeModesArray);

			if (ERROR_SUCCESS == QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, &pathCount, pathsArray,
				&modeCount, modesArray, 0))
			{
				DISPLAYCONFIG_DEVICE_INFO_HEADER* setPacket =
					reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(set);
				DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacket =
					reinterpret_cast<DISPLAYCONFIG_DEVICE_INFO_HEADER*>(request);

				//HDR is off
				returnValue = false;
				for (int i = 0; i < modeCount; i++)
				{
					try
					{
						returnValue = HDRIsOn(modesArray[i].id);
						if (returnValue)
							return returnValue;
					}
					catch (const std::exception&)
					{

					}

				}
			}
			std::free(pathsArray);
			std::free(modesArray);
			return returnValue;
		}
	}
}

extern "C"
{
	__declspec(dllexport) void SetGlobalHDRState(bool enabled)
	{
		SetGlobalHDR(enabled);
	}

	__declspec(dllexport) bool GetGlobalHDRState()
	{
		return HDRIsOn();
	}

	__declspec(dllexport) void SetHDRState(UINT32 uid, bool enabled)
	{
		SetHDR(uid, enabled);
	}

	__declspec(dllexport) bool GetHDRState(UINT32 uid)
	{
		return HDRIsOn(uid);
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
