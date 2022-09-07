# TACTLib [![TACTLib](https://github.com/overtools/TACTLib/actions/workflows/dotnet.yml/badge.svg)](https://github.com/overtools/TACTLib/actions/workflows/dotnet.yml)

### License: MIT

----

### Usage:
#### Creating a ClientHandler
The ClientHandler is the base object that controls the CAS(C). 

The path passed to ClientHandler should be the *base path* of the game install. E.g. where the game executables are. Upon creation, the client handler will load everything required for CASC operation.
```cs
string path = @"C:\ow\game\Overwatch";
ClientHandler clientHandler = new ClientHandler(path);
```

### Logging:
Logging is handled through the TACTLib.Logger class. It has events that are triggered by TACTLib during runtime.
Basic logging can be enabled by using TACTLib.Logger.RegisterBasic. That method also serves as an example of how to do custom logging. (see [Logger.cs](https://github.com/overtools/TACTLib/blob/master/TACTLib/Logger.cs))

```cs
// enables the default basic logger. should be called *before* creating the client
Logger.RegisterBasic();
```

#### Product specific:
(none of this is true yet)
 
##### VFS: (Black Ops 4)
```cs
ClientHandler client = new ClientHandler(path);
if (client.VFS == null) {
    // invalid install
    return;
}
VFSFileTree vfs = client.VFS;
using (Stream stream = vfs.Open(@"zone\base.xpak")) {
    // do whatever
}
foreach (string fileName in vfs.Files.Where(x => x.StartsWith(@"zone\"))) {
    using(Stream stream = vfs.Open(fileName)) {
        // could do this too
    }
}
```
##### Tank: (Overwatch)
TACTLib is used internally in TankLib/DataTool.
```cs
ClientHandler client = new ClientHandler(path);
ProductHandler_Tank tankHandler = client.ProductHandler as ProdcuctHandler_Tank;
if (tankHandler == null) {
    // not a valid overwatch install
    return;
}
using (Stream stream = tankHandler.OpenFile(0xE00000000000895)) {  // open any asset you want
    // in this case, parse the material
}
```
##### WorldOfWarcraftV6: (World of Warcraft)
Not all features are implemented yet. Anything below is a concept.
```cs
ClientHandler client = new ClientHandler(path, new ClientCreateArgs {
  HandlerArgs = new ClientCreateArgs_WorldOfWarcraft {
    ListFile = "https://raw.githubusercontent.com/wowdev/wow-listfile/master/listfile.txt"
  }
});
ProductHandler_WorldOfWarcraftV6 wowHandler = client.ProductHandler as ProductHandler_WorldOfWarcraftV6;
if (wowHandler == null) {
    // not a valid warcraft install
    return;
}
using (Stream stream = wowHandler.OpenFile("world/maps/azuremyst isle (7.3 intro)/azuremyst isle (7.3 intro).wdt")) {  // open any asset you want
    // in this case, parse wdt
}
foreach(string file in wowHandler.GetFiles("world")) {
    // will be empty if listfile is invalid
}
foreach(string dir in wowHandler.GetDirectories("world")) {
    // will be empty if listfile is invalid
}
```

## We can't wait to see the amazing things you will do with it
