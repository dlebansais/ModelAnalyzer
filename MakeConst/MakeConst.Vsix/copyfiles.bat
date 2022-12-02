rem %1 is the destination path but with some file name added
rem %2 is the solution name
rem %3 is the source path for z3 library files
rem %4 is the source manifest

setlocal
set INPUT_FOLDER_WITH_BACKSLASH=%~dp1
rem echo %INPUT_FOLDER_WITH_BACKSLASH%
set INPUT_FOLDER_WITHOUT_BACKSLASH=%INPUT_FOLDER_WITH_BACKSLASH:~0,-1%
rem echo %INPUT_FOLDER_WITHOUT_BACKSLASH%
set TARGET_FOLDER=%INPUT_FOLDER_WITHOUT_BACKSLASH%Roslyn\Extensions\%USERNAME%\%2\1.0
rem echo Output: %TARGET_FOLDER%
set SOURCE_FOLDER=%3
rem echo Input: %SOURCE_FOLDER%
rem echo File: %4

copy /Y %SOURCE_FOLDER%\libz3.dll %TARGET_FOLDER%
copy /Y %SOURCE_FOLDER%\Microsoft.Z3.dll %TARGET_FOLDER%
copy /Y %4 %TARGET_FOLDER%
