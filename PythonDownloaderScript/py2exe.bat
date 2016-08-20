echo off

:: Add to post-build: call "$(SolutionDir)PythonDownloaderScript\py2exe.bat" "$(SolutionDir)PythonDownloaderScript"

set dir="%~1"

pushd %dir%

delete "build"
md "build"
copy "GetImageFromGoogle.py" "build\GetImageFromGoogle.py"
copy "setup.py" "build\setup.py"

pushd "build"
C:\Python27\python.exe setup.py install
C:\Python27\python.exe setup.py py2exe
popd

popd