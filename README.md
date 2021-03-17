# RaftCore

[![NuGet version](https://img.shields.io/nuget/v/RaftCore.svg?style=flat-square)](https://www.nuget.org/packages/RaftCore/)
[![Build Status](https://github.com/guille/RaftCore/actions/workflows/dotnet.yml/badge.svg)

Implementation of the Raft algorithm for the .NET Core Platform. This project is part of my Bachelor Final Project for the University of Oviedo. You can find a visualization of the implementation using D3js and ASP.NET [here](https://github.com/guille/raftcoreweb)

## 1. Content

**RaftCore**: Library project consisting of an extensible implementation of the Raft algorithm.

**RaftCoreTest**: Tests for RaftCore.

## 2. Using the library

The library is published on the NuGet package manager under the name "RaftCore". Once the library has been obtained, it must be included into the user’s project. Different steps will be taken depending on how it was obtained. The Microsoft [documentation page for NuGet packages](https://docs.microsoft.com/en-us/nuget/consume-packages/ways-to-install-a-package.) goes into detail on these options.

When using Visual Studio to add the NuGet package to a project, the dependency is created automatically.

Once RaftCore has been added to the project, the next step is to choose a state machine and a connector. It is recommended at this point to either generate the API reference or visit the documentation page online ([link](https://guille.github.io/RaftCore/)) to consult.

### 2.1. Choosing a state machine

The project offers two state machines out-of-the-box. However, these implementations are very basic and were mostly developed to test the application, so most users will need to develop their own implementation.

To do that, the programmer must extend the class *IRaftStateMachine*, included in the namespace *RaftCore.StateMachine*. The documentation pertinent to this interface, as well as the methods to implement can be found in the documentation page. It is important to implement the method *TestConnection* in a way that will later allow the library to determine the time it would take to make a request to the state machine.

Another important note is the handling of bad or unrecognizable requests to the state machine. The recommended way to tackle this issue is to ignore the requests. The error can be logged, but by ignoring the request all the state machines are ensured to have the same state without errors or exceptions.

The other methods must be implemented to parse the expected user commands and status requests. For example, a possible implementation of this interface could connect to a relational database in the host machine and send the user commands as SQL strings to it. If the database is configured in the same way in all the machines, the same entries will be executed on each and it will be replicated.

### 2.2. Choosing a connector

The project also offers two basic connectors. The first one connects to nodes in memory and is mainly used for testing or simulations. The second one, *APIRaftConnector*, takes a base URL and will make HTTP requests to a set of pre-defined endpoints. The parameters will be sent via JSON, and the result is also expected to be in JSON. These pre-defined endpoints are listed in the class’ documentation.

However, most users will prefer to configure their own endpoints, request and response structures, and encodings when communicating. To do that, the user must implement the interface *IRaftConnector*, available in the namespace *RaftCore.Connections*. The resulting class must then map the methods defined to the node matching the connector’s id.

When creating the connector, the user must also create the back-end responsible for serving these requests, if necessary. For example, if the user was using the given *APIRaftConnector*, he would still have to configure a server that listened on the given URL and properly interpreted the received requests at the correct routes. For example, the server must serve a route with the given base URL and the route “/requestvote”. That route will receive a POST request from other nodes whenever other nodes want to request the machine’s node’s vote. The user-configured server must parse the received JSON object and call the RequestVote method on its node with the appropriate parameters. Afterwards, the user would have to return the Result object as JSON back to the other node.

### 2.3. Configuring the cluster and nodes

The *RaftCluster* class’ functionality is based on having a list of properly configured connectors. The following code snippet is an example of configuring a cluster with a set of user-defined connectors that store each node’s id and an IP address to connect to them:

```cs
var cluster = new RaftCluster();
cluster.AddNode(new MyIPConnector(1, “192.168.1.1”));
cluster.AddNode(new MyIPConnector(2, “192.168.1.2”));
cluster.AddNode(new MyIPConnector(3, “192.168.1.3”));
```

The cluster must be configured on each machine/node. After creating the cluster, each machine must create as many *RaftNode* instances as it will have running on it, usually one. Finally, the nodes must be instantiated, configured and started. The following examples are using the provided *NumeralStateMachine* class as the chosen state machine.

Code in machine 1:

```cs
var node = new RaftNode(1, new NumeralStateMachine());
node.Configure(cluster);
node.Run();
```

Code in machine 2:

```cs
var node = new RaftNode(2, new NumeralStateMachine());
node.Configure(cluster);
node.Run();
```

Code in machine 3:

```cs
var node = new RaftNode(3, new NumeralStateMachine());
node.Configure(cluster);
node.Run();
```

The execution will fail if the node is not configured, and the node will not respond to RPCs until
it is running.

Once that code has been executed on each machine, all the nodes will be running, and the user can start adding entries to the nodes. To do this, the user must call the method *MakeRequest* on its machine’s node instance. The node will then append the request to its log if it is a leader or redirect it to a leader.

```cs
node.MakeRequest(“+20”);
```

A node can be forcefully stopped by calling the Stop method on it. This action can be reversed by calling the method Restart. The Raft algorithm the library is built upon ensures the entries will be replicated for as long as half plus one nodes are still running.

## 3. Contributing

Contributions for any developer willing to extend RaftCore's functionality are welcome and encouraged. For example, a developer might wish to add a connector or state machine implementation he has developed to the project, fix a bug or add some of the missing Raft functionality, such as log compaction. As a rule of thumb, changes in the core functionality of the algorithm will require changes in the classes *RaftNode*, *RaftCluster* and *IRaftConnector*, as well as all the connector implementations.

Adding new implementations should be as simple as adding the implemented class to the
*RaftCore.StateMachine.Implementations* or the *RaftCore.Connections.Implementations* namespaces and their corresponding folders.

It is mandatory to submit comprehensive automated tests along with any extensions or changes proposed to the main code repository. These tests must be appropriately named and organized inside the *RaftCoreTest* project. The tests can be executed by calling the command:

`dotnet test RaftCoreTest`

Or from the Visual Studio test runner interface.

Finally, any classes changed must be documented using Visual Studio’s XML comments to re-generate the documentation.

Feel free to open an issue if you want to discuss what to add or if you have any questions. If you want to contribute but you don't know what to work on, check out the open Issues.

## 4. Useful links

[API Reference](https://guille.github.io/RaftCore)

[NUGet package homepage](https://www.nuget.org/packages/RaftCore/)

[Visualization](https://github.com/guille/RaftCoreWeb)
