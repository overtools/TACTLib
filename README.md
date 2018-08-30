# TACTLib 

### License: MIT
[![Build status](https://ci.appveyor.com/api/projects/status/7341i7g2qupdle6l?svg=true)](https://ci.appveyor.com/project/yukimono/tactlib)

----

### Usage:
#### Creating a ClientHandler
The ClientHandler is the base object that controls the CAS(C). 

The path passed to ClientHandler should be the *base path* of the game install. E.g. where the game executables are. Upon creation, the client handler will load everything required for CASC operation.
```c#
string path = @"C:\ow\game\Overwatch";
ClientHandler clientHandler = new ClientHandler(path);
```

### Logging:
Logging is handled through the TACTLib.Logger class. It has events that are triggered by TACTLib during runtime.
Basic logging can be enabled by using TACTLib.Logger.RegisterBasic. That method also serves as an example of how to do custom logging. (see [Logger.cs](https://github.com/overtools/TACTLib/blob/master/TACTLib/Logger.cs))

```c#
// enables the default basic logger. should be called *before* creating the client
Logger.RegisterBasic();
```

#### Product specific:
(none of this is true yet)
 
##### VFS: (Black Ops 4)
Not all features are implemented yet. GetFiles(string subDir) is just an idea for now.
```c#
ClientHandler client = new ClientHandler(path);
if (client.VFS == null) {
    // invalid install
    return;
}
VFSFileTree vfs = client.VFS;
using (Stream stream = vfs.Open(@"zone\base.xpak")) {
    // do whatever
}
foreach (string folder in vfs.Root.GetFiles(@"zone\")) {
    // could maybe do this too
}
```
##### Tank: (Overwatch)
TACTLib is used internally in TankLib/DataTool.
```c#
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