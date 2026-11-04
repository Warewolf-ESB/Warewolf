/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common;
using Dev2.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.SelfHost;

/// <summary>
/// Runs one workflow and exits, with nothing listening: <c>--run &lt;file.bite&gt;</c>. This is what the npm
/// package's <c>warewolf run</c> calls. The host is built (so the same services, catalog and logging are
/// there) but never started, so no port is opened and no service manager is involved.
/// <para>
/// The workflow's output goes to stdout; errors and logs go to stderr, so the output can be piped. The exit
/// code is 0 when the workflow succeeded, 1 when it failed and 2 for a usage error.
/// </para>
/// </summary>
internal static class RunOnceCommand
{
    internal const int Succeeded = 0;
    internal const int Failed = 1;
    internal const int UsageError = 2;

    internal const string Usage = @"Usage: --run <workflow.bite> [options]

  --input <json>        the workflow's inputs, as JSON (e.g. '{""name"":""Wolf""}')
  --input-file <path>   read the inputs from a file; '-' reads them from stdin
  --resources <dir>     the folder holding the sources and workflows it calls (default: the workflow's folder)
  --xml                 write the output as XML instead of JSON
  --debug               write the debug tree instead of the output
";

    /// <summary>What <c>--run</c> was asked to do.</summary>
    internal sealed record Options(string WorkflowPath, string? Input, string? InputFile, string? ResourcesDirectory, bool Xml, bool Debug);

    internal static bool IsRequested(string[] args) =>
        args.Contains("--run", StringComparer.OrdinalIgnoreCase);

    /// <summary>Parses the arguments, or returns null with <paramref name="error"/> set.</summary>
    internal static Options? Parse(string[] args, out string? error)
    {
        error = null;
        string? workflow = null, input = null, inputFile = null, resources = null;
        bool xml = false, debug = false;

        for (var i = 0; i < args.Length; i++)
        {
            string Value(string name)
            {
                if (i + 1 >= args.Length) throw new ArgumentException($"{name} needs a value.");
                return args[++i];
            }

            try
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--run": workflow = Value("--run"); break;
                    case "--input": input = Value("--input"); break;
                    case "--input-file": inputFile = Value("--input-file"); break;
                    case "--resources": resources = Value("--resources"); break;
                    case "--xml": xml = true; break;
                    case "--debug": debug = true; break;
                    default: throw new ArgumentException($"Unknown option '{args[i]}'.");
                }
            }
            catch (ArgumentException e)
            {
                error = e.Message;
                return null;
            }
        }

        if (string.IsNullOrWhiteSpace(workflow)) error = "--run needs a workflow file.";
        else if (input != null && inputFile != null) error = "Use --input or --input-file, not both.";
        else return new Options(workflow, input, inputFile, resources, xml, debug);
        return null;
    }

    /// <summary>The defaults for a run, applied before the engine reads its configuration. Anything already set wins.</summary>
    internal static void ApplyDefaults(Func<string, string?> getEnv, Action<string, string> setEnv)
    {
        void Default(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(getEnv(name))) setEnv(name, value);
        }

        // A locally run engine is free, like a self-hosted one; nothing here is licensed or metered.
        Default("WAREWOLF_LICENSE_CHECK_ENABLED", "false");
        Default("WAREWOLF_USAGE_TRACKING_ENABLED", "false");
        // Logs would share the terminal with the output, and a failure is already reported as the run's result
        // (stderr, exit code 1), so a run logs nothing by default. WAREWOLF_LOGGING_CONFIG='{"console":true}'
        // with EXECUTIONLOGLEVEL turns it on, to stderr, when diagnosing.
        Default("EXECUTIONLOGLEVEL", "ERROR");
        Default("WAREWOLF_LOGGING_CONFIG", "{}");
    }

    internal static async Task<int> RunAsync(string[] args, TextWriter stdout, TextWriter stderr, Func<TextReader> stdin)
    {
        var options = Parse(args, out var error);
        if (options is null)
        {
            await stderr.WriteLineAsync($"{error}\n\n{Usage}");
            return UsageError;
        }

        var workflowPath = Path.GetFullPath(options.WorkflowPath);
        if (!File.Exists(workflowPath))
        {
            await stderr.WriteLineAsync($"Workflow not found: {workflowPath}");
            return UsageError;
        }

        string? input;
        try
        {
            input = options.InputFile switch
            {
                null => options.Input,
                "-" => await stdin().ReadToEndAsync(),
                var path => await File.ReadAllTextAsync(path),
            };
        }
        catch (IOException e)
        {
            await stderr.WriteLineAsync($"Couldn't read the input: {e.Message}");
            return UsageError;
        }

        ApplyDefaults(Environment.GetEnvironmentVariable, Environment.SetEnvironmentVariable);
        var config = HostEnvironmentConfig.Load();
        var loggingConfig = LoggingConfiguration.FromEnvironment();

        using var host = Build(config, loggingConfig);
        await StartupOrchestrator.RunStartupAsync(host, config);
        Dev2Logger.ExternalSink = new Dev2LoggerSinkAdapter(host.Services.GetRequiredService<IExecutionLogger>());

        var request = new WorkflowExecutionRequest
        {
            WorkflowFilePath = workflowPath,
            ReturnType = options.Xml ? EmitionTypes.XML : EmitionTypes.JSON,
            IsDebug = options.Debug,
        };
        if (options.ResourcesDirectory != null) request.WorkflowsDirectory = Path.GetFullPath(options.ResourcesDirectory);
        if (!string.IsNullOrWhiteSpace(input)) request.RawInputPayload = input;

        var result = host.Services.GetRequiredService<IWorkflowExecutor>().Execute(request);

        var output = await result.ReadPayloadAsync();
        if (!string.IsNullOrEmpty(output)) await stdout.WriteLineAsync(output);
        foreach (var message in result.Errors) await stderr.WriteLineAsync(message);
        return result.IsSuccess ? Succeeded : Failed;
    }

    /// <summary>The engine's services without a server: built, never run, so nothing listens.</summary>
    internal static IHost Build(HostEnvironmentConfig config, LoggingConfiguration loggingConfig)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(loggingConfig.MelMinimumLevel);
        if (loggingConfig.EnableConsoleLogging)
            builder.Logging.AddConsole(console => console.LogToStandardErrorThreshold = Microsoft.Extensions.Logging.LogLevel.Trace);
        builder.Logging.AddFilter("Warewolf.Execution.Lightweight", loggingConfig.MelMinimumLevel);

        builder.Services.AddExecutionLogging(loggingConfig);
        builder.Services.AddCoreServices(config, serviceBusTrigger: null);
        if (config.EncryptionEnabled) builder.Services.AddKeyVaultEncryption(config);
        return builder.Build();
    }
}
