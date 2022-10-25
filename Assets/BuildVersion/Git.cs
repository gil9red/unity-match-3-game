/* MIT License
Copyright (c) 2016 RedBlueGames
Code written by Doug Cox

Copyright (c) 2022 Ilya Petrash (aka gil9red)
Code modified by Ilya Petrash (aka gil9red)
*/

using System;
using System.Text.RegularExpressions;

using UnityEngine;


/// <summary>
/// GitException includes the error output from a Git.Run() command as well as the
/// ExitCode it returned.
/// </summary>
public class GitException : InvalidOperationException
{
    public GitException(int exitCode, string errors) : base(errors) =>
        this.ExitCode = exitCode;

    /// <summary>
    /// The exit code returned when running the Git command.
    /// </summary>
    public readonly int ExitCode;
}

public static class Git
{
    /* Properties ============================================================================================================= */

    /// <summary>
    /// Example: "v0.0-2-g7c18d46"
    /// </summary>
    public static string BuildVersionRaw => Run(@"describe --tags --long --match ""v[0-9]*""");

    /// <summary>
    /// Example: "v0.0-2-g7c18d46" -> "0.0.2 (7c18d46)"
    /// </summary>
    public static string BuildVersionWithHash
    {
        get
        {
            var version = BuildVersionRaw;

            var regex = new Regex(@"^v(\d+)\.(\d+)-(\d+)-g(\w+)$");
            var m = regex.Match(version);
            if (!m.Success)
            {
                throw new Exception($"Invalid format: {version}!");
            }
            var g = m.Groups;
            return $"{g[1]}.{g[2]}.{g[3]} ({g[4]})";
        }
    }

    /// <summary>
    /// Retrieves the build version from git based on the most recent matching tag and
    /// commit history. This returns the version as: {major.minor.build} where 'build'
    /// represents the nth commit after the tagged commit.
    /// Note: The initial 'v' and the commit hash code are removed.
    /// Example: "v0.0-2-g7c18d46" -> "0.0.2"
    /// </summary>
    public static string BuildVersion
    {
        get
        {
            var version = BuildVersionRaw;
            // Remove initial 'v' and ending git commit hash.
            version = version.Replace('-', '.');
            version = version[1..version.LastIndexOf('.')];
            return version;
        }
    }

    /// <summary>
    /// The currently active branch.
    /// </summary>
    public static string Branch => Run(@"rev-parse --abbrev-ref HEAD");

    /// <summary>
    /// Returns a listing of all uncommitted or untracked (added) files.
    /// </summary>
    public static string Status => Run(@"status --porcelain");


    /* Methods ================================================================================================================ */

    /// <summary>
    /// Runs git.exe with the specified arguments and returns the output.
    /// </summary>
    public static string Run(string arguments)
    {
        using var process = new System.Diagnostics.Process();
        var exitCode = process.Run(
            @"git", arguments, Application.dataPath,
            out var output, out var errors
        );
        if (exitCode == 0)
        {
            return output;
        }
        
        throw new GitException(exitCode, errors);
    }
}
