# PowerOutageNotifier

A Windows service, written in .NET Framework 4.8 to send Telegram notifications when:
1. a planned power outage is scheduled in Belgrade, Serbia
2. a planned water outage is scheduled in Belgrade, Serbia
3. an unplanned water outage occurs in Belgrade, Serbia
4. a parking fine is received in Belgrade, Serbia

## Install

1. Say "hi" to https://t.me/PowerOutageNotifierSrbijaBot
2. Send "/my_id" to https://t.me/get_id_bot  
Note down you Chat ID which the bot will tell you  
Example: Your Chat ID = 123456
4. In userdata.example.csv, replace 123456 with your Chat ID
5. Replace the "Палилула" and "САВЕ МРКАЉА" with your own District Name and Street Name
6. Rename userdata.example.csv to userdata.csv
7. Build the project
8. From an admin console run install.bat  
Note: install.bat uses installutil, which needs to be installed beforehand. This tool is automatically installed with Visual Studio.
9. Double-check in Services that the PowerOutageNotifier is running and that the startup is set to "automatic"
