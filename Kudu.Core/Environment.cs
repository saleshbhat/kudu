﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using Kudu.Core.Helpers;
using Kudu.Core.Infrastructure;

namespace Kudu.Core
{
    public class Environment : IEnvironment
    {
        private readonly string _webRootPath;
        private readonly string _deploymentsPath;
        private readonly string _deploymentToolsPath;
        private readonly string _siteExtensionSettingsPath;
        private readonly string _diagnosticsPath;
        private readonly string _locksPath;
        private readonly string _sshKeyPath;
        private readonly string _tempPath;
        private readonly string _scriptPath;
        private readonly string _nodeModulesPath;
        private string _repositoryPath;
        private readonly string _logFilesPath;
        private readonly string _applicationLogFilesPath;
        private readonly string _tracePath;
        private readonly string _analyticsPath;
        private readonly string _deploymentTracePath;
        private readonly string _dataPath;
        private readonly string _jobsDataPath;
        private readonly string _jobsBinariesPath;

        // This ctor is used only in unit tests
        public Environment(
                string rootPath,
                string siteRootPath,
                string tempPath,
                string repositoryPath,
                string webRootPath,
                string deploymentsPath,
                string diagnosticsPath,
                string locksPath,
                string sshKeyPath,
                string scriptPath,
                string nodeModulesPath,
                string dataPath,
                string siteExtensionSettingsPath)
        {
            if (repositoryPath == null)
            {
                throw new ArgumentNullException("repositoryPath");
            }

            RootPath = rootPath;
            SiteRootPath = siteRootPath;
            _tempPath = tempPath;
            _repositoryPath = repositoryPath;
            _webRootPath = webRootPath;
            _deploymentsPath = deploymentsPath;
            _deploymentToolsPath = Path.Combine(_deploymentsPath, Constants.DeploymentToolsPath);
            _siteExtensionSettingsPath = siteExtensionSettingsPath;
            _diagnosticsPath = diagnosticsPath;
            _locksPath = locksPath;
            _sshKeyPath = sshKeyPath;
            _scriptPath = scriptPath;
            _nodeModulesPath = nodeModulesPath;

            _dataPath = dataPath;

            _jobsDataPath = Path.Combine(_dataPath, Constants.JobsPath);
            _jobsBinariesPath = _jobsDataPath;

            _logFilesPath = Path.Combine(rootPath, Constants.LogFilesPath);
            _applicationLogFilesPath = Path.Combine(_logFilesPath, Constants.ApplicationLogFilesDirectory);
            _tracePath = Path.Combine(rootPath, Constants.TracePath);
            _analyticsPath = Path.Combine(tempPath ?? _logFilesPath, Constants.SiteExtensionLogsDirectory);
            _deploymentTracePath = Path.Combine(rootPath, Constants.DeploymentTracePath);
        }

        public Environment(
                string rootPath,
                string binPath,
                string repositoryPath)
        {
            RootPath = rootPath;

            SiteRootPath = Path.Combine(rootPath, Constants.SiteFolder);

            _tempPath = Path.GetTempPath();
            _repositoryPath = repositoryPath;
            _webRootPath = Path.Combine(SiteRootPath, Constants.WebRoot);
            _deploymentsPath = Path.Combine(SiteRootPath, Constants.DeploymentCachePath);
            _deploymentToolsPath = Path.Combine(_deploymentsPath, Constants.DeploymentToolsPath);
            _siteExtensionSettingsPath = Path.Combine(SiteRootPath, Constants.SiteExtensionsCachePath);
            _diagnosticsPath = Path.Combine(SiteRootPath, Constants.DiagnosticsPath);
            _locksPath = Path.Combine(SiteRootPath, Constants.LocksPath);

            string siteName = System.Environment.GetEnvironmentVariable("APP_POOL_ID");
            siteName = NormalizeSiteName(siteName);

            if (OSDetector.IsOnWindows())
            {
                _sshKeyPath = Path.Combine(rootPath, Constants.SSHKeyPath);
            }
            else
            {
                // in linux, rootPath is "/home", while .ssh folder need to under "/home/{user}", 
                // and username and site name is always the same in actual deployment
                _sshKeyPath = Path.Combine(rootPath, siteName, Constants.SSHKeyPath);
            }
            _scriptPath = Path.Combine(binPath, Constants.ScriptsPath);
            _nodeModulesPath = Path.Combine(binPath, Constants.NodeModulesPath);
            _logFilesPath = Path.Combine(rootPath, Constants.LogFilesPath);
            _applicationLogFilesPath = Path.Combine(_logFilesPath, Constants.ApplicationLogFilesDirectory);
            _tracePath = Path.Combine(rootPath, Constants.TracePath);
            _analyticsPath = Path.Combine(_tempPath ?? _logFilesPath, Constants.SiteExtensionLogsDirectory);
            _deploymentTracePath = Path.Combine(rootPath, Constants.DeploymentTracePath);
            _dataPath = Path.Combine(rootPath, Constants.DataPath);
            _jobsDataPath = Path.Combine(_dataPath, Constants.JobsPath);
            _jobsBinariesPath = Path.Combine(_webRootPath, Constants.AppDataPath, Constants.JobsPath);
        }

        public string RepositoryPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_repositoryPath);
            }
            set
            {
                // normalize the '/' to '\'
                _repositoryPath = Path.GetFullPath(value);
            }
        }

        public string WebRootPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_webRootPath);
            }
        }

        public string DeploymentsPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_deploymentsPath);
            }
        }

        public string DeploymentToolsPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_deploymentToolsPath);
            }
        }

        public string DiagnosticsPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_diagnosticsPath);
            }
        }

        public string LocksPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_locksPath);
            }
        }

        public string SSHKeyPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_sshKeyPath);
            }
        }

        public string RootPath
        {
            get;
            private set;
        }

        public string SiteRootPath
        {
            get;
            private set;
        }

        public string TempPath
        {
            get
            {
                return _tempPath;
            }
        }

        public string ScriptPath
        {
            get
            {
                return _scriptPath;
            }
        }

        public string NodeModulesPath
        {
            get
            {
                return _nodeModulesPath;
            }
        }

        public string LogFilesPath
        {
            get
            {
                return _logFilesPath;
            }
        }

        public string ApplicationLogFilesPath
        {
            get
            {
                return _applicationLogFilesPath;
            }
        }

        public string TracePath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_tracePath);
            }
        }

        public string AnalyticsPath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_analyticsPath);
            }
        }

        public string DeploymentTracePath
        {
            get
            {
                return FileSystemHelpers.EnsureDirectory(_deploymentTracePath);
            }
        }

        public string DataPath
        {
            get
            {
                return _dataPath;
            }
        }

        public string JobsDataPath
        {
            get
            {
                return _jobsDataPath;
            }
        }

        public string JobsBinariesPath
        {
            get { return _jobsBinariesPath; }
        }

        public string SiteExtensionSettingsPath
        {
            get { return _siteExtensionSettingsPath; }
        }

        public string FunctionsPath
        {
            get
            {
                return this.WebRootPath;
            }
        }

        public string AppBaseUrlPrefix
        {
            get
            {
                // GetLeftPart(Authority) returns the https://www.example.com of any Uri
                var url = HttpContext.Current?.Request?.Url?.GetLeftPart(UriPartial.Authority);
                if (string.IsNullOrEmpty(url))
                {
                    // if call is not done in Request context (eg. in BGThread), fall back to %host%
                    var host = System.Environment.GetEnvironmentVariable(Constants.HttpHost);
                    if (!string.IsNullOrEmpty(host))
                    {
                        return $"https://{host}";
                    }

                    throw new InvalidOperationException("There is no request context");
                }
                return url;
            }
        }

        public static bool IsAzureEnvironment()
        {
            return !String.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
        }

        public static string GetFreeSpaceHtml(string path)
        {
            try
            {
                ulong freeBytes;
                ulong totalBytes;
                GetDiskFreeSpace(path, out freeBytes, out totalBytes);

                var usage = Math.Round(((totalBytes - freeBytes) * 100.0) / totalBytes);
                var color = usage > 97 ? "red" : (usage > 90 ? "orange" : "green");
                return String.Format(CultureInfo.InvariantCulture, "<span style='color:{0}'>{1:#,##0} MB total; {2:#,##0} MB free</span>", color, totalBytes / (1024 * 1024), freeBytes / (1024 * 1024));
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        static void GetDiskFreeSpace(string path, out ulong freeBytes, out ulong totalBytes)
        {
            ulong diskFreeBytes;
            if (!EnvironmentNativeMethods.GetDiskFreeSpaceEx(path, out freeBytes, out totalBytes, out diskFreeBytes))
            {
                throw new Win32Exception();
            }
        }

        [SuppressUnmanagedCodeSecurity]
        static class EnvironmentNativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetDiskFreeSpaceEx(string path, out ulong freeBytes, out ulong totalBytes, out ulong diskFreeBytes);
        }

        private static string NormalizeSiteName(string siteName)
        {
            var normalizedSiteName = siteName;
            if (normalizedSiteName.StartsWith("~1", StringComparison.Ordinal))
            {
                normalizedSiteName = normalizedSiteName.Substring(2);
            }

            normalizedSiteName = Regex.Replace(normalizedSiteName, "__[0-9a-f]{4}$", "").Replace("mobile$", "");
            return normalizedSiteName;
        }
    }
}
