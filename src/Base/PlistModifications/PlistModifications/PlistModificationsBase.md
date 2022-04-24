# DDG Prime Plist Modifications Base
### Rev 2/23/2022

## Introduction
DDG Prime PlistModificationsBase test class is an utility method to restore or clean plist modifications cache.<br>
Intended use is to Restore Plist contents every unit while using ConcurrentTraceDecoder.


## Parameters
#### Patlist
Optional parameter. User can enter comma-separated list of patlist to clean/restore.
#### OperationMode
##### Restore
Restore Patlists with DirtyFlag set to their original condition. Restore runs recursively in case of nested plists.
##### Clean
Deletes Patlist from Plist tree repository. **IMPORTANT: Clean does not Restore!**
