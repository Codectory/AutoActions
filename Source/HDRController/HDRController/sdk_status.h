#ifndef SDK_STATUS_H
#define SDK_STATUS_H

#include "nvapi_deps.h"
#include "../Include/sdk_status.h"

namespace core
{
    struct SdkStatusImpl : SdkStatus
    {
        SdkStatusImpl(const NvAPI_Status& status);
    };

} // namespace core
#endif