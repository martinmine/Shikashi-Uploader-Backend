dotnet publish -r linux-x64 -c Release --self-contained false .\ShikashiAPI\ShikashiAPI.csproj
scp ShikashiAPI/bin/Release/net6.0/linux-x64/publish/ShikashiAPI root@api.shikashi.me:/home/shikashi/shikashinet
