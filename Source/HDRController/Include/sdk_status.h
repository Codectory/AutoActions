#ifndef SDK_STATUS_PUB_H
#define SDK_STATUS_PUB_H

#include <string>

namespace core
{
    struct SdkStatus
    {
        std::string Message = "";
        bool IsSuccessful = false;
    };

} // namespace core
#endif