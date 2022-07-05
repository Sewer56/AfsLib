# AfsLib

A simple, relatively fast library for the reading and writing of CRIWare AFS archives.

## Usage

### Previewing an AFS File
You can preview an AFS file by creating an instance of `AfsFileViewer`.
In this mode, the AFS file is directly loaded from memory. No copying is done, allowing you to quickly inspect the structure of the AFS archive..

```csharp
var data = File.ReadAllBytes(afsFilePath);
if (AfsFileViewer.TryFromFile(data, out var afsViewer)) 
{
	// Do stuff.
};
```

### Reading data from a single entry in an AFS File
To get the data from a single entry inside an AFS with minimal memory footprint, use the static method:
```csharp
AfsArchive.SeekToAndLoadDataFromIndex(stream, index)
```


This will seek through the file and return a byte[] containing just the data at that index.
This is useful for large AFS with one-off reads.


### Editing an AFS File
To edit an AFS file, create an instance of `AfsArchive`.
`AfsArchive` reads all of the data from an `AfsFileViewer`, converting it into a format easier to edit for the end user.

```csharp
var data = File.ReadAllBytes(afsFilePath);
if (AfsArchive.TryFromFile(data, out var afsArchive)) 
{
	// Do stuff.
};
```

To convert the file back to bytes, use the `ToBytes` method.

```csharp
afsArchive.ToBytes();
```
