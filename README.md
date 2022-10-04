# TPL Dataflow Examples

Build a robust concurrent program in CSharp is not so easy, you need to check shared contexts, 
thread-safety classes, implement locks and semaphores.
Inside the TPL namespace we have Dataflow, an official extensions that can reduce some of those 
complexities. 

Dataflow promotes an actor-based programming model and provide blocks that can be chained in complex 
pipelines to stream your data in-process.

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