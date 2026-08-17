# =============================================================================
# Warewolf.Execution.QueueProcessor - Linux container worker
#
# BUILD CONTEXT IS A STAGED COPY OF THE PUBLISH OUTPUT, not the repo. It is produced by
# Deploy-WwQueueProcessor.ps1, which copies -PublishPath to a temp directory and then populates
#   Settings/triggers/<triggerId>.bite   one per deployed trigger
#   Settings/sources/<sourceId>.bite     every source those triggers reference
# (WFAES-encrypted under the engine's Key Vault key when -EncryptStagedSettings is passed), so this
# Dockerfile only has to copy that tree. Your publish output is never mutated.
#
#   Manual build:  dotnet publish Dev/Warewolf.Execution.QueueProcessor -c Release -o /tmp/pub
#                  cp -r <settings> /tmp/pub/Settings
#                  docker build -f Dev/Warewolf.Execution.QueueProcessor/Dockerfile -t wwqp:dev /tmp/pub
#
# Why not a two-stage build from source:
#   * A `dotnet publish` inside the image compiles ~133 projects on every deploy (~6 min) purely to
#     reproduce output the deploy machine already has.
#   * That compile happens on a CASE-SENSITIVE filesystem, so any csproj whose <Compile Include>
#     casing differs from the file on disk fails there and nowhere else - it cost a full build cycle
#     to find one (Dev2.Common: 'JsonUtils.cs' vs 'JSONUtils.cs') and six more remain in the repo.
#   * The repo-root context also had to tar ~gigabytes of bin/obj and locked .vs files.
#
# ACA is Linux-only, which is why the worker is a new project rather than a containerised
# QueueWorker.exe (a WinExe with DPAPI + Win32 dependencies).
# =============================================================================

# aspnet, NOT runtime: the worker has no ingress, but its publish output requires the
# Microsoft.AspNetCore.App shared framework as well as Microsoft.NETCore.App. The requirement
# arrives transitively through Dev2.Common (Common\ExtensionMethods.cs uses Microsoft.AspNetCore.Mvc,
# and Boxed.AspNetCore / Microsoft.Extensions.Logging.Log4Net.AspNetCore carry the FrameworkReference),
# so it appears in Warewolf.Execution.QueueProcessor.runtimeconfig.json. On the plain runtime image the
# host cannot resolve it and the container exits 150 -- "No frameworks were found." -- before any
# RabbitMQ connection is attempted, which surfaces in ACA as CrashLoopBackOff.
#
# MUST match the TargetFramework this project (and Dev2.Common) actually build against - net10.0
# (see the pipeline's 'Install .NET 10 SDK' step and the NETSDK1045 comment next to it). An 8.0
# base image starts, but its dotnet muxer only has Microsoft.NETCore.App 8.0.x, which cannot
# resolve the runtimeconfig.json's "version": "10.0.0" requirement - the process exits immediately
# with "You must install or update .NET to run this application.", which ACA also reports as
# CrashLoopBackOff, and the container never gets far enough to open a RabbitMQ connection.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Non-root by default; the worker needs no privileged operation and no inbound port.
RUN useradd --uid 10001 --create-home --shell /usr/sbin/nologin warewolf
WORKDIR /app

# Create the Settings tree FIRST so that a hand-built image with an empty context still starts and
# fails with the clear "no trigger files" error rather than a missing-directory exception.
RUN mkdir -p /app/Settings/triggers /app/Settings/sources

# The staged publish tree, including Settings/. Config is BAKED IN, not mounted or fetched: this
# worker scales to zero, so any startup round-trip to a share or blob would be paid on every
# 0->1 scale.
COPY . .

RUN chown -R warewolf:warewolf /app
USER warewolf

# Engine-parity logging switches (see LoggingConfiguration.FromEnvironment()).
# Deploy-WwQueueProcessor.ps1 overrides these per Container App via --set-env-vars.
ENV DOTNET_ENVIRONMENT=Production \
    ENABLECONSOLELOGGING=true \
    ENABLEAPPLICATIONINSIGHTS=false \
    ENABLEELASTICSEARCHLOGGING=false \
    EXECUTIONLOGLEVEL=INFO \
    STRUCTURED_LOGS=true \
    QUEUE__SETTINGSPATH=/app/Settings \
    WORKER__MAXCONCURRENCY=1 \
    WORKER__SHUTDOWNGRACESECONDS=60 \
    ENGINE__TIMEOUTSECONDS=45

# No EXPOSE: this worker has no ingress. ACA sends SIGTERM on scale-in/rollout and the
# host drains in-flight messages before exiting (see QueueConsumerService.StopAsync).
STOPSIGNAL SIGTERM

# Invoke the DLL through the `dotnet` muxer rather than an apphost. The publish output is produced
# on the DEPLOY MACHINE, which is Windows, so its apphost is Warewolf.Execution.QueueProcessor.EXE -
# the extensionless Linux apphost does not exist and "./Warewolf.Execution.QueueProcessor" would fail
# with "no such file or directory". The .dll is always present in a framework-dependent publish, so
# this entrypoint works whichever OS produced it and needs no -r linux-x64 publish.
ENTRYPOINT ["dotnet", "Warewolf.Execution.QueueProcessor.dll"]
