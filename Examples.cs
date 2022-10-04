using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;
using Timer = System.Timers.Timer;

namespace TPLDataflowExamples
{
    public class Examples
    {
        /*
            BUFFER BLOCK
            
           ┌────────┐
           │ buffer ├──► OUT
           └────────┘
            1+ tasks
            
            - Stores a ConcurrentQueue.
            - Handles concurrency when writing to it.
            - Optionally blocks when reached the capacity.
            - Can be linked to multiple targets, only one receive each message.
        */
        [Test]
        public void BufferBlockExample()
        {
            // We can assign a limit of items to the buffer before stop accepting messages
            var opts = new ExecutionDataflowBlockOptions { 
                BoundedCapacity = 10,
            };
            
            // Create a BufferBlock<int> object.
            var bufferBlock = new BufferBlock<int>(opts);

            // Post several messages to the block.
            for (var i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // Receive the messages back from the block.
            for (var i = 0; i < 3; i++)
            {
                Console.WriteLine(bufferBlock.Receive());
            }

            /* Output:
               0
               1
               2
             */
        }
        
        /*
            ACTION BLOCK
            
                  ┌────────┬──────────────────┐
            IN ───► buffer │ Action<in>       │
                  └────────┴──────────────────┘
                                1+ tasks
   
            - Has 1+ sources.
            - Calls a delegate when receive data.
            - Cannot link targets.
         */
        [Test]
        public void ActionBlockExample()
        {
            // Create an ActionBlock<int> object that prints values to the console.
            var actionBlock = new ActionBlock<int>(n => Console.WriteLine(n));

            // Post several messages to the block.
            for (var i = 0; i < 3; i++)
            {
                actionBlock.Post(i * 10);
            }

            // Set the block to the completed state and wait for all tasks to finish.
            actionBlock.Complete();
            actionBlock.Completion.Wait();

            /* Output:
               0
               10
               20
            */
        }
        
        /*
            TRANSFORM BLOCK
            
                  ┌────────┬──────────────────┐
            IN ───► buffer │ Function<in,out> ├──► OUT
                  └────────┴──────────────────┘
                     size        1+ tasks
                     
            - Similar to the Action block but has both source and targets.
            - Calls a function when receive data.
         */
        [Test]
        public void TransformBlockExample()
        {
            // We can configure a lot of options on each block.
            var opts = new ExecutionDataflowBlockOptions {
                BoundedCapacity = 10,   
                // Can process multiple items in parallel.
                MaxDegreeOfParallelism = 2,
                // Can output items in the same order or not.
                EnsureOrdered = false,
                // Can setup how tasks are assigned.
                // TaskScheduler = ...
                // MaxMessagesPerTask = ...
            };
            
            // Create a TransformBlock<int, double> object that computes the square root of its input.
            var transformBlock = new TransformBlock<int, double>(n => Math.Sqrt(n), opts);

            // Post several messages to the block.
            transformBlock.Post(10);
            transformBlock.Post(20);
            transformBlock.Post(30);

            // Read the output messages from the block.
            for (var i = 0; i < 3; i++)
            {
                Console.WriteLine(transformBlock.Receive());
            }

            /* Output:
               3.16227766016838
               4.47213595499958
               5.47722557505166
             */
        }
        
        /*
            SIMPLE PIPELINE EXAMPLE

            ┌─────────┐   ┌──────────   ┌─────────┐
            │ buffer  ├───► transform───► action  │
            │ block   │   │ block   │   │ block   │
            └─────────┘   └─────────┘   └─────────┘
                          max 2 items   max 2 items
                            2 tasks       2 tasks
              
            - We take words and we return them with their length.
            - We can do that both synchronous and asynchronous.
            - It gives you explicit control over how data is buffered and moves in this flow.
            - We configure a Completion mechanism.
         */
        [Test]
        public async Task SimplePipelineExample()
        {
            var bufferBlock = new BufferBlock<string>();
            
            var opts = new ExecutionDataflowBlockOptions { 
                // Max 2 items in a queue.
                BoundedCapacity = 2,
                // We parallelize.
                MaxDegreeOfParallelism = 2,
                // No ordering needed.
                EnsureOrdered = false
            };
            
            var transformBlock = new TransformBlock<string, Tuple<string,int>>(word => 
                new Tuple<string, int>(word, word.Length), opts);
            
            var actionBlock = new ActionBlock<Tuple<string,int>>(i => 
                Console.WriteLine($"{i.Item1} size: {i.Item2}"), opts);

            var linkOpts = new DataflowLinkOptions {
                // Once a block is marked as completed this information flows in the pipeline.
                PropagateCompletion = true
            };
            
            bufferBlock.LinkTo(transformBlock, linkOpts);
            transformBlock.LinkTo(actionBlock, linkOpts);

            foreach(var word in new[] { "tpl", "dataflow", "examples" })
            {
                await bufferBlock.SendAsync(word);
            }
            // We inform that the first block has no more data to input.
            bufferBlock.Complete();
            // We wait for the completion of the last block.
            actionBlock.Completion.Wait();
            
            /* Output:
                tpl size: 3
                examples size: 8
                dataflow size: 8
             */
        }
        
        /*
            GRAPH EXAMPLE WITCH BATCH BLOCK AND JOIN BLOCK
            
                                         ┌─────────┐
                                         │ packages│
                                         │         │
                                         └─────┬───┘
                                               │
            ┌─────────┐   ┌─────────┐    ┌─────▼───┐   ┌─────────┐
            │ items   ├───► collect ├────►assemble ├───► out     │
            │         │   │    10   │    │         │   │         │
            └─────────┘   └─────────┘    └─────────┘   └─────────┘
        
            - We want to assemble a package/box with at max 10 items.
            - Batch block: collects x items and output a single array.
            - Join block: receive from 2 sources and output an object that contains both elements.
         */
        [Test]
        public void BatchBlockAndJoinBlockPipelineExample()
        {
            // As an example we start adding the items before linking the blocks.
            var packageBuffer = new BufferBlock<string>();
            for (var i = 0; i < 5; i++) { packageBuffer.Post($"package-{i}"); }
            
            var itemBuffer = new BufferBlock<string>();
            for (var i = 0; i < 45; i++) { itemBuffer.Post($"item-{i}"); }

            var batchBlock = new BatchBlock<string>(10);
            var joinBlock = new JoinBlock<string,string[]>();
            var actionBlock = new ActionBlock<Tuple<string,string[]>>(i => Console.WriteLine($"{i.Item1} contains {i.Item2.Length} items"));

            var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
            
            // Immediately items start flowing after we link.
            itemBuffer.LinkTo(batchBlock, linkOpts);
            packageBuffer.LinkTo(joinBlock.Target1, linkOpts);
            batchBlock.LinkTo(joinBlock.Target2, linkOpts);
            joinBlock.LinkTo(actionBlock, linkOpts);
            
            // We mark that we finished to input.
            packageBuffer.Complete();
            itemBuffer.Complete();
            // We wait for the overall completion.
            actionBlock.Completion.Wait();
            
            /* Output:
                package-0 contains 10 items
                package-1 contains 10 items
                package-2 contains 10 items
                package-3 contains 10 items
                package-4 contains 5 items
             */
        }
        
        /*
            GRAPH EXAMPLE WITCH BROADCAST BLOCK AND TIME-WINDOW BATCH BLOCK
         
                           ┌─────────┐
                    ┌──────►   out   │
                    │      │         │
                    │      └─────────┘
                    │
            ┌───────┴─┐    ┌─────────┐    ┌─────────┐
            │  metric ├────► 1 sec   ├────►   out   │
            │         │    │ avg     │    │         │
            └───────┬─┘    └─────────┘    └─────────┘
                    │
                    │      ┌─────────┐    ┌─────────┐
                    │      │ 5 sec   ├────►   out   │
                    └──────► avg     │    │         │
                           └─────────┘    └─────────┘
                           
            - We want to collect data and output it in 3 different sources.
            - We want to collect an avg of the data each 1s and 5s.
            - A broadcast block sends each message to all the targets.
            - A new way to collect items in a time frame with a batch block.         
        */
        [Test]
        public async Task BroadcastBlockAndTimeFrameBatchesPipelineExample()
        {
            const int maxBatchSize = 1000_000_000;
            var opts = new ExecutionDataflowBlockOptions { 
                // Disabled parallelism.
                MaxDegreeOfParallelism = 1,
                EnsureOrdered = true,
                // No limits in each buffer.
                BoundedCapacity = -1
            };
            
            var priceBuffer = new BroadcastBlock<int>(item => item);

            // Time windowed batch block.
            var oneSecondBatch = new BatchBlock<int>(maxBatchSize);
            var intervalTimer = new Timer(1_000);
            intervalTimer.Elapsed += (s, e) => oneSecondBatch.TriggerBatch();
            intervalTimer.AutoReset = true;
            intervalTimer.Enabled = true;
            
            var fiveSecondsBatch = new BatchBlock<int>(maxBatchSize);
            var intervalTimer2 = new Timer(5_000);
            intervalTimer2.Elapsed += (s, e) => fiveSecondsBatch.TriggerBatch();
            intervalTimer2.AutoReset = true;
            intervalTimer2.Enabled = true;
            
            var actionBlock = new ActionBlock<int>(i => Console.WriteLine($"price: {i}"), opts);
            var oneSecondActionBlock = new ActionBlock<int[]>(i => Console.WriteLine($"price avg in 1s: {i.Average()}"), opts);
            var fiveSecondsActionBlock = new ActionBlock<int[]>(i => Console.WriteLine($"price avg in 5s: {i.Average()}"), opts);
            
            var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
            priceBuffer.LinkTo(oneSecondBatch, linkOpts);
            priceBuffer.LinkTo(fiveSecondsBatch, linkOpts);
            priceBuffer.LinkTo(actionBlock, linkOpts);
            oneSecondBatch.LinkTo(oneSecondActionBlock, linkOpts);
            fiveSecondsBatch.LinkTo(fiveSecondsActionBlock, linkOpts);
            
            // We send 10/s metrics for 11 seconds.
            var timer = new CancellationTokenSource(11_000);
            var randomPrice = new Random();
            while (!timer.IsCancellationRequested)
            {
                await priceBuffer.SendAsync(randomPrice.Next(100));
                await Task.Delay(100);
            }
            priceBuffer.Complete();
            await actionBlock.Completion;
            
            /* Output:
                price: 42
                price: 64
                ...
                price avg in 1s: 46.9
                ...
                price: 35
                price: 28
                price avg in 1s: 43.7
                ...
                price: 46
                price: 45
                price avg in 1s: 45.7
                ...
                price: 29
                price: 42
                price avg in 1s: 55.8
                ...
                price: 17
                price: 9
                price avg in 1s: 45.7
                price: 29
                price avg in 5s: 47.2
                ...
                price: 13
                price: 73
                price avg in 1s: 35.7
                price avg in 5s: 35.7
            */
        }
    }
}