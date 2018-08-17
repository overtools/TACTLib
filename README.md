# TACTLib 

### License: MIT
[![Build status](https://ci.appveyor.com/api/projects/status/7341i7g2qupdle6l?svg=true)](https://ci.appveyor.com/project/yukimono/tactlib)

----

### Usage:
* Will add properly whenever i can get this thing to work

#### Creating a ClientHandler
The ClientHandler is the base object that controls the CAS(C). 

The path passed to ClientHandler should be the *base path* of the game install. E.g. where the game executables are. Upon creation, the client handler will load everything required for CASC operation.
```c#
string path = @"C:\ow\game\Overwatch";
ClientHandler clientHandler = new ClientHandler(path);
```

#### Product specific:
(none of this is true yet)
 
##### VFS: (Black Ops 4)
```c#
ClientHandler client = new ClientHandler(path);
if (client.VFS == null) {
    // invalid install
    return; // or whatever
}
VFSFileTree vfs = client.VFS;
using (Stream stream = vfs.Open(@"zone\\base.xpak")) {
    // do whatever
}
foreach (string folder in vfs.Root.GetFiles("zone\\")) {
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
    return; // or whatever
}
```