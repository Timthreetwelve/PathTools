The PathTools ReadMe file


About PathTools
=================
In Windows, PATH is an environment variable that specifies a set of directories, separated with
semicolons (;), where executable programs are located. Some applications add their directory to
the PATH variable when they are installed. Some ill-behaved applications don't ask permission
to add a directory to the PATH or don't remove their directory from the PATH when they are
uninstalled. That's where PathTools comes in. It lets you easily check the PATH variable for
changes.


First Run
=========
The first time PathTools is executed, it will create a file that will be used to compare the
current PATH to what it is in subsequent executions. This file is seeded with C:\Windows\System32
and C:\Windows. These directories will show as "Unchanged" and every additional directory in the
PATH will show as "Added". This is normal and will only happen the first time.

Additionally, when starting up for the first time, PathTools will ask if you want to keep a log
file. In the dialog that pops up, check the box next to Keep a log file and enter a complete path
to the log file. This log file is only written to when a change to the PATH has been detected. If
at some point, you no longer want a log file or want to change the filename, select Log File Setup
from the Options menu and make changes there.  To view the log, select View Log File from the
Options menu.


The PathTools Display
=======================
After the first execution, PathTools will show all directories, marking them as "Unchanged",
"Duplicate", "Added", or "Removed". Duplicate directories are shown in blue, those that are added
will be shown in green, removed directories will be shown in red.

PathTools also shows the directory type. "Machine" directories belong to the system. Those labeled
"User" belong to the user currently logged on. Together they combine to make up the PATH. It's
possible for the same directory to exist in both the machine and user paths, in which case you
may want to remove one.


Automating PathTools
====================
PathTools can alert you when there has been a change your computer's PATH variable.To make this
happen on a regular basis, set up a new task in Task Scheduler as you normally would. In the
Trigger tab, set it to recur at an interval you feel is appropriate, perhaps once a day. In the
Action tab, select New action, enter the full path to PathTools as the program to execute. Then
specify "/hide" as an argument. This will keep the PathTools window from showing unless it
detects a change. If there are no changes to report, it will stay out of your way. If there are
changes, the window will appear. There are two settings on the Options menu that can help it be
more noticeable if needed, Keep on Top and Sounds.

By default PathTools is not case sensitive. This means that C:\Windows and c:\windows are considered
the same. Microsoft added the option to enable case sensitivity on a per-directory basis in the Windows
10 April 2018 update. If you have enabled this option for any directory in the PATH or would just like
case sensitive comparisons you can enable them in the Options menu.

Because PathTools saves PATH information after every execution, it will only alert you to a change
the first time it detects a change in the PATH.


Additional Actions
==================
Other selections on the Action menu will let you open Environment Variables to make changes if needed,
show the PATH in the command prompt, verify the PATH in a window or with a PowerShell script, and view
the Saved Path file. Additionally, you can copy the contents of the PathTools window to the clipboard,
check the status of the Windows LongPathEnabled setting, and see the total length of the PATH variable
in bytes.


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

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
