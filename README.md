# HDR Profile
 
You can add applications to HDR Profile to automatically activate HDR in Windows when one of hte added processes is running.

The tool is using NVApi (copied some code from https://github.com/bradgearon/hdr-switch, thank you!), so I think this will only work with NVidia graphic cards.

If you choose "Focused" HDR will be activated when one of the added application is in focus and will deactivate HDR, when none of the added app is in focus.

Running mode works the same, but as the name says, it is not neccessary for the application to be in focus.

For the moment, the tool is only looking for the process name and not the location of the file.