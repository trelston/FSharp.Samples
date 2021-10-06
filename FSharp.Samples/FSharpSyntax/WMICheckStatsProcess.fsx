// sets the current directory to be same as the script directory
System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

#r @"../../packages/FSharp.Management/lib/net40/System.Management.Automation.dll"
#r @"../../packages/FSharp.Management/lib/net40/FSharp.Management.dll"
#r @"../../packages/FSharp.Management/lib/net40/FSharp.Management.WMI.dll"

open FSharp.Management


// get data for the local machine
type Local = WmiProvider<"localhost">
let data = Local.GetDataContext()

