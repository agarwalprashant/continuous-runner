﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Magnum.Extensions;
using NLog;

namespace ContinuousRunner.Console
{
    public class Application
    {
        public static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                var options = CommandLineOptions.FromArgs(args);

                using (var container = Container.Build(options))
                {
                    var collection = container.Resolve<IScriptCollection>();

                    var queue = container.Resolve<IRunQueue>();

                    logger.Info("Loading scripts");

                    var loader = container.Resolve<IScriptLoader>();

                    foreach (var script in collection.GetScripts(fi => loader.Load(fi)))
                    {
                        logger.Info("Loaded: {0}", script.File.Name);

                        queue.Push(script);
                    }

                    var watcher = container.Resolve<IWatcher>();

                    var watchHandle = watcher.Watch();

                    logger.Info("Entering run loop; press any key to abort");

                    var input = System.Console.OpenStandardInput();

                    while (input.Length == 0)
                    {
                        var queued = queue.RunAsync().ToArray();
                        if (queued.Any())
                        {
                            var results = Task.WhenAll(queued);

                            results.Wait();

                            foreach (var tr in results.Result)
                            {
                                logger.Info(tr.ToString());
                            }
                        }
                        else
                        {
                            logger.Info("No tests in queue");
                        }

                        Thread.Sleep(500.Milliseconds());
                    }

                    watchHandle.Cancel();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Bootstrap of continuous runner failed: {ex.Message}");
            }
        }
    }
}