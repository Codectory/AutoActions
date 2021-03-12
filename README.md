# HDR Profile
 
You can add applications to HDR Profile to automatically activate HDR in Windows when one of hte added processes is running.

~~The tool is using NVApi (copied some code from https://github.com/bradgearon/hdr-switch, thank you!), so I think this will only work with NVidia graphic cards.~~

The tool is using fandangos approach to toggle HDR mode (https://github.com/fandangos/AutoHDR)

If you choose "Focused" HDR will be activated when one of the added application is in focus and will deactivate HDR, when none of the added app is in focus.

Running mode works the same, but as the name says, it is not neccessary for the application to be in focus.

For the moment, the tool is only looking for the process name and not the location of the file.

**Features:**

**Auto-Start:** This setting will create a scheduled task to run this application when the currently logged on user gets logged on.  
**HDR-Mode** Select at which condition of one of the added applications HDR should be activated automatically.  
**Compatibility mode:** In some games, e.g. Cyberpunk 2077,  you don't see any HDR options in settings, when HDR was not enabled in Windows before the game was launched. Therefore, the application kills the process on the first occurence, activates hdr and restarts the process.  
**Start in HDR-Mode:** You can use HDRProfile as a launcher by selecting the application and clicking Start in HDR-Mode. This will enable HDR before starting the application, which is necessary for some applications, e.g. Cyberpunk 2077.  

Screenshots:

![ScreenShot](https://raw.github.com/Codectory/HDR-Profile/main/Screenshots/Status_1-5-0.png)

![ScreenShot](https://raw.github.com/Codectory/HDR-Profile/main/Screenshots/Applications_1-5-0.png)

![ScreenShot](https://raw.github.com/Codectory/HDR-Profile/main/Screenshots/Settings_1-5-0.png)
