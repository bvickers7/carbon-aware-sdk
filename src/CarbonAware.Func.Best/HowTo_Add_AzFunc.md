# Add dotnet func to dev container

```sh
apt-get install gpg
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-debian-bullseye-prod bullseye main" > /etc/apt/sources.list.d/dotnetdev.list'
apt-get update
apt-get install azure-functions-core-tools-4
```

Run `func` command
```sh
func
```

## Reference

https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Clinux%2Ccsharp%2Cportal%2Cbash#v2
https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
https://github.com/Azure/azure-functions-core-tools
