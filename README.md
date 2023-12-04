# PowerOutageNotifier

A Windows service, written in .NET Framework 4.8 to send Telegram notifications when:
1. a planned power outage is scheduled in Belgrade, Serbia
2. a planned water outage is scheduled in Belgrade, Serbia
3. an unplanned water outage ocurrs in Belgrade, Serbia
4. a parking fine is recieved in Belgrade, Serbia

## Install

1. Edit userdata.example.csv as needed and rename it to userdata.csv
2. Install it by running install.bat  
Note: install.bat uses installutil, which needs to be installed beforehand.
