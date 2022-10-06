# TPL Dataflow Examples

<img src="https://user-images.githubusercontent.com/86239515/194308617-5c2b0c70-5077-4547-947f-1a83bfc98db6.png" width="400">

Build a robust concurrent program in CSharp is not so easy, you need to check shared contexts, 
thread-safety classes, implement locks and semaphores.
Inside the TPL namespace we have Dataflow, an official extensions that can reduce some of those 
complexities. 

Dataflow promotes an actor-based programming model and provide blocks that can be chained in complex 
pipelines to stream your data in-process.

<img src="https://user-images.githubusercontent.com/86239515/194308153-dfd66a99-e9e9-45e5-bc3a-2fb0daf355ab.png" width="400">

This is useful when you have multiple operations that must communicate asynchronously 
or when you want to process data as it becomes available.

It also gives you explicit control over how data is buffered and moves in this flow.

A dataflow pipeline is a series of components or blocks that can be joined.

We are going to cover some of them in the examples:
- BufferBlock
- BroadcastBlock
- TransformBlock
- BatchBlock
- JoinBlock
- ActionBlock

<img src="https://user-images.githubusercontent.com/86239515/194307632-1eb81779-7593-46b5-a873-1d225db5a11d.png" width="400">
<img src="https://user-images.githubusercontent.com/86239515/194307662-6eb8d4a7-c950-4d7a-aa93-8342fa87d111.png" width="400">
<img src="https://user-images.githubusercontent.com/86239515/194307647-86e68d8f-ddc8-4b38-87f4-062355a699e8.png" width="400">
<img src="https://user-images.githubusercontent.com/86239515/194307593-08ed96f4-7fe5-4211-8ea8-9f423ae2d3f5.png" width="400">
<img src="https://user-images.githubusercontent.com/86239515/194307575-35903ee4-28b3-4875-9db0-865984842933.png" width="400">

### Links
- [TPL Dataflow](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library)
- [Dataflow pipeline example](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/walkthrough-creating-a-dataflow-pipeline?source=recommendations)
- [Pitfalls of parallelism](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/potential-pitfalls-in-data-and-task-parallelism)
- [Article 1](https://michaelscodingspot.com/pipeline-pattern-tpl-dataflow), 
  [Article 2](https://hamidmosalla.com/2018/08/04/what-is-tpl-dataflow-in-net-and-when-should-we-use-it),
  [Article 3](https://jack-vanlightly.com/blog/2018/4/18/processing-pipelines-series-tpl-dataflow),
  [Article 4](https://blog.wedport.co.uk/2020/06/22/data-processing-pipelines-with-tpl-dataflow-in-c-net-core)
- Book: [TPL Dataflow by Example](https://www.amazon.com/TPL-Dataflow-Example-Reactive-Programming/dp/1499149352)
- Book: [Concurrency in C# Cookbook](https://www.amazon.com/Concurrency-Cookbook-Asynchronous-Multithreaded-Programming/dp/149205450X)
