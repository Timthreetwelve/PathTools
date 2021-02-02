The PathTools ReadMe file


About PathTools
=================
In Windows, PATH is an environment variable that specifies a set of directories, separated with semicolons (;), where executable programs are located. Some applications add their directory to the PATH variable when they are installed. Some ill-behaved applications do not ask permission to add a directory to the PATH or do not remove their directory from the PATH when they are uninstalled. That is where PathTools comes in. It lets you easily check the PATH variable for changes.


First Run
=========
The first time PathTools is executed, it will create a file that will be used to compare the current PATH to what it is in subsequent executions. This file is seeded with C:\Windows\System32 and C:\Windows. These directories will show as "Unchanged" and every additional directory in the PATH will show as "Added". This is normal and will only happen the first time.

Additionally, when starting up for the first time, PathTools will ask if you want to keep a log file. In the dialog that pops up, check the box next to Keep a log file and enter a complete path to the log file. This log file is only written to when a change to the PATH has been detected. If at some point, you no longer want a log file or want to change the filename, select Log File Setup from the Options menu, and make changes there.  To view the log, select View Log File from the Options menu.


The PathTools Display
=====================
After the first execution, PathTools will show all directories in the PATH, marking them as "Unchanged", "Duplicate", "Added", or "Removed". Duplicate directories are shown in blue, those that are added will be shown in green, removed directories will be shown in red. PathTools also shows the directory type. "Machine" directories belong to the system. Those labeled "User" belong to the current user. Together they combine to make up the PATH. It is possible for the same directory to exist in both the machine and user paths, in which case you may want to remove one.


Additional Tools
================
Other selections in the Toolbox are, Check for Duplicates which will check for duplicates in a new window. Find on PATH will open a window that where you can enter the name of a file to determine if the file is in one or more directories in the path. Verify path will verify that the directories in the path are valid. Verify with PowerShell will perform the same verification only using PowerShell. Check LonPathEnabled will check the status of the Windows LongPathEnabled registry setting. Display PATH Length will show the total length of the PATH variable in bytes. Show PATH in CMD.exe will display the PATH at the Command prompt. Environmental Variables will let you open the Environment Variables dialog to make changes if needed.


Case Sensitivity
================
By default, PathTools is not case sensitive. This means that C:\Windows and c:\windows are considered the same. Microsoft added the option to enable case sensitivity on a per-directory basis in the Windows 10 April 2018 update. If you have enabled this option for any directory in the PATH or would just like case sensitive comparisons you can enable them in the Options menu.


Options
=======
The Options menu has items that allow you to toggle showing alternating colors and grid lines in the data grid. The Show File & Directory Counts option allows you to toggle the display of those counts on the right side of the status bar. Show Toolbox Panel determines whether the panel and buttons on the left side of the window are shown. Keep on Top will allow the window to remain on top of other windows. The remaining items on the Options menu have been discussed above.


Automating PathTools
====================
PathTools can alert you when there has been a change your computer's PATH variable. To make this happen on a regular basis, set up a new task in Task Scheduler as you normally would. In the Trigger tab, set it to recur at an interval you feel is appropriate, perhaps once a day. In the Action tab, select New action, enter the full path to PathTools as the program to execute. Then specify "/hide" as an argument. This will keep the PathTools window from showing unless it detects a change. If there are no changes to report, it will stay out of your way. If there are changes, a small alert window will appear in the lower left corner of the screen. When the Play Sounds item on the Options menu is enabled a sound will also be played. Because PathTools saves PATH information after every execution, it will only alert you to a change the first time it detects a change in the PATH.


Uninstall
=========
To uninstall PathTools, use the regular Windows add/remove programs feature.


Notices and License
===================
PathTools was written in C# by Tim Kennedy and requires .NET 5.

PathTools uses the following icons & packages:

Fugue Icons set https://p.yusukekamiyamane.com/

Json.net v12.0.3 from Newtonsoft https://www.newtonsoft.com/json

NLog v4.7.7 https://nlog-project.org/


MIT License
===========
Copyright (c) 2021 Tim Kennedy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

