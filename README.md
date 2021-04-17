<h1>ConsoleFileManager</h1>
<p>Release candidate, March 22, 2021.</p>
<p>C#, Console Application, .Net Core 3.1, Any CPU</p>
<p>Developer: Fakhrudinov Alexander, asbuka@gmail.com </p>
<h2>Description</h2>
<p>An entry-level console file manager with two panels and a command line in the Windows environment.
<br>
<img alt="Program view" title="Program view" src="overView.png" />
</p>
<p>Supported copying, deleting, moving through the directory structure, renaming, running files, creating new directories.</p>
<p>The work with the resizing of the console by the user has been implemented - to change it, you just need to stretch the window to the desired size.</p>
<p>Changes to the dimensions opened in the directory panels are saved and will be applied the next time the program is started.</p>
<p>The history of the commands entered by the user is saved, including the commands made in the file manager from the "F" buttons, such as F5, F6, etc. </p>
<h2>Main areas of the program </h2>
<h3>Panel.</h3>
<p><img alt="Panel" title="Panel" src="onePanel.png" /></p>
<p>Above - an information panel about the current directory and disk.<br>
Below is the line with directories and files in the current directory.
The highlighted line shows the currently selected object.<br>
Bottom line - information about pagination on the panel and the total number of directories and files.</p>
<h3>F buttons information</h3>
<p><img alt="F bar" title="F bar" src="FBar.png" /></p>
<p>The available 'F' keys are shown here with a brief description of the action to be performed.</p>
<h3>Command panel</h3>
<p><img alt="Command bar" title="Command bar" src="CommandBar.png" /></p>
<p>On the command panel there are 2 lines - at the bottom, the user enters the command and its arguments. The top line shows a hint for the entered command and an estimate of the entered path.</p>
<h2>User manual</h2>
<h3>Panel operations</h3>
<p><strong>Basic navigation.</strong> To navigate within the panel, use the up and down arrows, pgUp, pgDown, Home, End buttons.<br>
Press Enter to start and open the file, as well as to enter the directory and move to the parent directory "..".<br>
Tab to change the active panel.<br>
To change the disk, press enter on ".." while in the root directory of the disk.</p>
<p><strong>F1 Help</strong> Will show a panel with a list of available commands. Press Enter to close the information window.</p>
<p><strong>F3 Info</strong> Shows information about the selected object. Press Enter to close the information window.</p>
<p><strong>F5 Copy</strong> Copies selected object to an inactive panel. A confirmation dialog will be shown, where 'Y' or 'y' is confirmation, any other input will cancel the action.</p>
<p><strong>F6 Move</strong> Moves selected object to an inactive panel.  A confirmation dialog will be shown, where 'Y' or 'y' is confirmation, any other input will cancel the action.</p>
<p><strong>F7 NewDir</strong> Create a new directory in the current directory. New name request will be displayed. Enter a name to create or leave blank to cancel the action.</p>
<p><strong>F8 Del</strong> Delete selected object in active panel. A confirmation dialog will be shown, where 'Y' or 'y' is confirmation, any other input will cancel the action.</p>
<p><strong>F9 Rename</strong> Rename selected object in active panel. New name request will be displayed. Enter a name to create or leave blank to cancel the action.</p>
<p><strong>Alt + F4 Exit.</strong> Close this program.</p>
<h3>Command line operations</h3>
<p><strong>Ctrl + Enter</strong> Paste the selected in active panel object into the command line </p>
<p><strong>Ctrl + E</strong> Show a list of the last used commands. Use the up and down arrows to navigate through the list. Press Enter to select </p>
<p><strong>equal</strong> Changes the working directory of the inactive panel to the working directory of the active panel. All panels show the same directory. </p>
<p><strong>cd argument</strong> Change directory. The full path, the name of the directory in the active panel, or a relative path are accepted as an argument.<br>
Examples:
<ul>
<li>cd d: (change drive)
<li>cd .. (go to the to parent directory)
<li>cd folderName (go to the folder 'folderName', located in the current directory of the active panel)
<li>cd c:\temp\333\ (go to the absolute path) 
</ul></p>
<p><strong>cp argument</strong> Copy, one argument. The object will be copied to the directory on the inactive panel. The full path, the name of the directory or file in the active panel, or a relative path are accepted as an argument.<br>
Examples:
<ul>
<li>cp folderName (copy the folderName located in the current directory of the active panel to the directory on the inactive panel) </ul></p>
<p><strong>cp argument1, argument2</strong>  Copy, two arguments. Arguments delimiter is ', '. The full path, the name of the directory or file in the active panel, or a relative path are accepted as an arguments<br>
Examples:
<ul>
<li>cp folderName, NewFolderName (copy the folderName located in the current directory of the active panel to the directory with new name on the inactive panel)
<li>cp c:\temp\111\folderName, c:\222\333\folderName (copy from absolute path to the absolute path) 
</ul></p>
<p><strong>mv argument</strong>  Move, one argument. The object will be movied to the directory on the inactive panel. The full path, the name of the directory or file in the active panel, or a relative path are accepted as an argument.<br>
Examples:
<ul>
<li>mv folderName (move the folderName located in the current directory of the active panel to the directory on the inactive panel) </ul></p>
<p><strong>mv argument1, argument2</strong>  Move, two arguments. Arguments delimiter is ', '. The full path, the name of the directory or file in the active panel, or a relative path are accepted as an arguments<br>
Examples:
<ul>
<li>mv folderName, NewFolderName (move the folderName located in the current directory of the active panel to the directory with new name on the inactive panel)
<li>mv c:\temp\111\folderName, c:\222\333\folderName (move from absolute path to the absolute path) 
</ul></p>
<p><strong>rm argument</strong> Remove object. The full path, the name of the directory or file in the active panel, or a relative path are accepted as an argument.<br>
Examples:
<ul>
<li>rm folderName (Remove folder 'folderName', located in the current directory of the active panel)
<li>rm c:\temp\333 (Remove folder '333' by absolute path)</ul></p>
<p><strong>mkdir argument</strong> Make new directory. The full path, the name of the new directory creatted in the active panel, or a relative path are accepted as an argument.<br>
Examples:
<ul>
<li>mkdir ..\111 (create new directory '111' in the parent directory relatively current directory of the active panel)
<li>mkdir folderName (create new directory 'folderName' in the current directory of the active panel)
<li>mkdir c:\temp\333\111 (create new directory '111' by the absolute path) 
</ul></p>
<p><strong>run argument</strong> Try to execute in the default program. The full path, the name of the directory or file in the active panel, or a relative path are accepted as an argument.<br>
Examples:
<ul>
<li>run 111 (open folder '111' located in the current directory of the active panel in default file manager)
<li>run rtfm.txt (if exist, open file 'rtfm.txt' in the current directory of the active panel with default text editor program)
<li>run c:\temp\333\111\rtfm.txt (if exist, open file 'rtfm.txt' by the absolute path with default text editor program) 
</ul></p>
<p><strong>name argument, argument2</strong> Rename the object. The first argument must exist. The second argument is the new name of the object. The full path, the name of the directory or file in the active panel, or a relative path are accepted as an first argument.<br>
Examples:
<ul>
<li>name folderName, NewFolderName (Rename the folderName located in the current directory of the active panel with new name on the active panel)
<li>name c:\temp\111\folderName, c:\222\333\folderName (copy from absolute path to the absolute path)  ????
</ul></p>