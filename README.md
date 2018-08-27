# Ortelius
A Unity Engine visualiser for Everett

# Setup

1. Download gRPC for Unity from Experimental gRPC C# page and extract to `Assets` dir (it should put its stuff in `Plugins`)
2. Install gRPC, gRPC.Core, and gRPC.Tools via NuGet
3. Clone the `ebstorf` repository into e.g. `Assets/proto/ebstorf` (TODO: git submodule this)
4. `cd Assets`
5. `..\Packages\Grpc.Tools.1.14.1\tools\windows_x64\protoc.exe -Iproto --csharp_out=Ebstorf --grpc_out=Ebstorf --plugin=protoc-gen-grpc=..\Packages\Grpc.Tools.1.14.1\tools\windows_x64\grpc_csharp_plugin.exe proto\ebstorf\world.proto proto\ebstorf\world_traverser.proto proto\ebstorf\world_streamer.proto`
6. Done
