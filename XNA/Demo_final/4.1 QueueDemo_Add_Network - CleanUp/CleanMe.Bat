
# Delete temporaries
# Run before every submission

del /F /S /A:H *.suo
rmdir /S /Q PeerToPeer\bin
rmdir /S /Q PeerToPeer\obj

del /F /S PeerToPeer\QueueDemo.csproj.Debug.cachefile

del /F /S PeerToPeer\QueueDemo.csproj.Release.cachefile

rmdir /S /Q PeerToPeer\Content\bin
rmdir /S /Q PeerToPeer\Content\obj


# Magic secret sauce - updates the date and time
# Force Perforce to check in new file and version number

cd .\PeerToPeer\Properties\
copy AssemblyInfo.cs+Empty.cs AssemblyInfo.cs