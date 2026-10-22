/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Security.Cryptography;
using System.Text.Json;
using Dev2.Common;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Gives a run-from-package engine a writable workflows directory.
///
/// With <c>WEBSITE_RUN_FROM_PACKAGE</c> set, <c>wwwroot</c> is a read-only mount of the zip, so
/// <c>{AppContext.BaseDirectory}/Resources</c> can be read but never written: create_workflow,
/// edit_workflow, delete_workflow, add_source and the rest failed with
/// <c>FileNotFoundException</c>. When the app also has a persistent content share
/// (<c>WEBSITE_CONTENTSHARE</c>, mounted as <c>%HOME%</c> and shared by every instance), workflows
/// live in <c>%HOME%/data/Warewolf/Resources</c> instead, seeded from the package's Resources.
///
/// <see cref="SeedFileName"/>, beside that directory, records the hash of every file seeded, so a later package can
/// update a file nobody has changed since, while a file someone changed or deleted is left
/// alone. Without a content share (Flex Consumption, Linux Consumption) <c>%HOME%</c> isn't
/// persistent or shared, so the package directory is used as before.
/// </summary>
internal static class PackagedResources
{
    internal const string SeedFileName = ".package-seed.json";
    const string LockFileName = ".package-seed.lock";
    static readonly TimeSpan LockWait = TimeSpan.FromSeconds(60);

    /// <summary>
    /// The writable directory to use instead of <paramref name="packagedDirectory"/>, seeded and
    /// ready, or <paramref name="packagedDirectory"/> itself when there is no need or no way.
    /// </summary>
    internal static string ResolveWorkflowsDirectory(string packagedDirectory, Func<string, string?> getEnv)
    {
        var writable = WritableDirectoryFor(getEnv);
        if (writable is null)
        {
            return packagedDirectory;
        }

        try
        {
            Seed(packagedDirectory, writable);
            Dev2Logger.Info($"PackagedResources: running from a package, so workflows live in {writable} (seeded from {packagedDirectory})", nameof(PackagedResources));
            return writable;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Dev2Logger.Warn($"PackagedResources: couldn't seed {writable}, so workflows stay in the read-only {packagedDirectory}. ExceptionType={ex.GetType().Name}", ex, nameof(PackagedResources));
            return packagedDirectory;
        }
    }

    /// <summary>
    /// <c>WAREWOLF_DATA_DIR/Resources</c> when that's set (a self-hosted engine, see
    /// <c>SelfHost.SelfHostedEngine</c>, whose install folder isn't the place for data);
    /// <c>%HOME%/data/Warewolf/Resources</c> when the app runs from a package and has a
    /// persistent content share; else <c>null</c>.
    /// </summary>
    internal static string? WritableDirectoryFor(Func<string, string?> getEnv)
    {
        var dataDirectory = getEnv("WAREWOLF_DATA_DIR");
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            return Path.Combine(dataDirectory, "Resources");
        }

        var package = getEnv("WEBSITE_RUN_FROM_PACKAGE");
        if (string.IsNullOrWhiteSpace(package) || package.Trim() == "0")
        {
            return null;
        }

        var home = getEnv("HOME");
        if (string.IsNullOrWhiteSpace(home) || string.IsNullOrWhiteSpace(getEnv("WEBSITE_CONTENTSHARE")))
        {
            return null;
        }

        return Path.Combine(home, "data", "Warewolf", "Resources");
    }

    /// <summary>
    /// Copies the package's files into <paramref name="writable"/>: new ones, and updated ones
    /// whose writable copy is still what was last seeded. Instances starting together take turns
    /// through a lock file on the share.
    /// </summary>
    internal static void Seed(string packagedDirectory, string writable)
    {
        Directory.CreateDirectory(writable);
        // Beside the directory, not in it: everything in it is read as workflows and sources.
        var parent = Path.GetDirectoryName(Path.GetFullPath(writable))!;
        using var _ = AcquireLock(Path.Combine(parent, LockFileName));

        var seedPath = Path.Combine(parent, SeedFileName);
        var seeded = File.Exists(seedPath)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(seedPath)) ?? new()
            : new Dictionary<string, string>();
        seeded = new Dictionary<string, string>(seeded, StringComparer.OrdinalIgnoreCase);

        if (Directory.Exists(packagedDirectory))
        {
            foreach (var source in Directory.EnumerateFiles(packagedDirectory, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(packagedDirectory, source).Replace('\\', '/');
                var packageHash = Hash(source);
                var target = Path.Combine(writable, relative);
                var known = seeded.TryGetValue(relative, out var lastSeeded);

                if (known && lastSeeded == packageHash)
                {
                    continue; // nothing new in the package; the copy may have been edited or deleted since
                }

                if (File.Exists(target))
                {
                    var current = Hash(target);
                    if (current == packageHash)
                    {
                        seeded[relative] = packageHash;
                    }
                    else if (known && current == lastSeeded)
                    {
                        Copy(source, target);
                        seeded[relative] = packageHash;
                    }
                    // else: changed here since it was seeded, or never seeded; the writable copy wins
                }
                else if (!known)
                {
                    Copy(source, target);
                    seeded[relative] = packageHash;
                }
                // else: seeded once and deleted since; stays deleted
            }
        }

        var temp = seedPath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(new SortedDictionary<string, string>(seeded, StringComparer.Ordinal), new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, seedPath, overwrite: true);
    }

    static void Copy(string source, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, overwrite: true);
    }

    static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    static FileStream AcquireLock(string path)
    {
        var deadline = DateTime.UtcNow + LockWait;
        while (true)
        {
            try
            {
                return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (DateTime.UtcNow < deadline)
            {
                Thread.Sleep(250);
            }
        }
    }
}
