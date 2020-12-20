#ifndef TOGGLE_PUB_H
#define TOGGLE_PUB_H

#include "color_mode.h"
#include "sdk_status.h"

namespace core
{
    class Toggle
    {
    public:
        Toggle();
        ~Toggle();

        SdkStatus setHdrMode(bool enabled);
        SdkStatus setColorMode(COLOR_MODE mode = COLOR_MODE::YUV444);
    };

} // namespace core

#endif