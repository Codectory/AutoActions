#include "HDRController.h"

#include <iostream>
#include "toggle.h"
#include "WinUser.h"
#include <stdio.h>



using namespace core;

static std::unique_ptr<Toggle> hdrToggle;


static void showError(std::string msg)
{
	LPCWSTR lmsg = (LPCWSTR)msg.c_str();

	MessageBox(NULL, lmsg, L"HDR Switch Error", MB_OK | MB_ICONWARNING);
}

static void setHdrMode(bool enabled)
{
	auto status = hdrToggle->setHdrMode(enabled);
	if (!status.IsSuccessful)
	{
		auto msg = "Error setting hdr mode:" + status.Message;
		showError(msg);
	}
}

extern "C"
{
	__declspec(dllexport) void SetHDR(bool enabled)
	{
		setHdrMode(enabled);
	}
}

