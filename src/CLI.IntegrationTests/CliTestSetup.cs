﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using automate;
using automate.Extensions;
using Xunit;

namespace CLI.IntegrationTests
{
    [CollectionDefinition("CLI", DisableParallelization = true)]
    public class AllCliTests : ICollectionFixture<CliTestSetup>
    {
    }

    public class CliTestSetup : IDisposable
    {
        private const string ProcessName = "automate";
        private readonly JsonFilePatternRepository repository;
        private StandardOutput error;
        private StandardOutput output;
        private Process process;

        public CliTestSetup()
        {
            Process.GetProcessesByName(ProcessName).ToList().ForEach(p => p.Kill());
            this.repository = new JsonFilePatternRepository(Environment.CurrentDirectory);
            ResetRepository();
            ResetCommandLineSession();
        }

        public StandardOutput Output
        {
            get
            {
                if (this.process.NotExists())
                {
                    return new StandardOutput(string.Empty);
                }

                if (!this.output.Exists())
                {
                    var standardOutput = this.process.StandardOutput.ReadToEnd();
                    this.output = new StandardOutput(standardOutput);
                }

                return this.output;
            }
        }

        public StandardOutput Error
        {
            get
            {
                if (this.process.NotExists())
                {
                    return new StandardOutput(string.Empty);
                }

                if (!this.error.Exists())
                {
                    var standardError = this.process.StandardError.ReadToEnd();
                    this.error = new StandardOutput(standardError);
                }

                return this.error;
            }
        }

        internal List<PatternMetaModel> Patterns => this.repository.List();

        internal PatternState PatternState => this.repository.GetState();

        public string Location => this.repository.Location;

        public void ResetRepository()
        {
            this.repository.DestroyAll();
        }

        public void RunCommand(string arguments)
        {
            KillProcess();
            ResetCommandLineSession();
            this.process = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.CurrentDirectory, ProcessName) + ".exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            if (this.process.Exists())
            {
                this.process.WaitForExit();
            }
        }

        public void Dispose()
        {
            KillProcess();
        }

        private void ResetCommandLineSession()
        {
            this.output = null;
            this.error = null;
        }

        private void KillProcess()
        {
            if (this.process.Exists())
            {
                if (!this.process.HasExited)
                {
                    this.process.Kill();
                }
            }
        }
    }
}