rem %1 is the destination path but with some file name added
rem %2 is the solution name
rem %3 is the source path for z3 library files
rem %4 is the source manifest
rem %5 is the destination manifest

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
set SOURCE_FOLDER_EXTENSION=bin\x64\Debug\net472
rem echo %SOURCE_FOLDER_EXTENSION%

if not exist %TARGET_FOLDER% goto end

echo Copying files to: %TARGET_FOLDER%
copy /Y %SOURCE_FOLDER%\libz3.dll %TARGET_FOLDER%
copy /Y %SOURCE_FOLDER%\Microsoft.Z3.dll %TARGET_FOLDER%
copy /Y %4 %TARGET_FOLDER%\%5
copy /Y %SOURCE_FOLDER_EXTENSION%\Microsoft.Extensions.Logging.Abstractions.dll %TARGET_FOLDER%

:end
