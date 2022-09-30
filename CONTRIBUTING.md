# Introduction

Welcome to automate, and thank you for being curious about how we go about doing things around here.

Since doing this kind of work requires a lot of attention and time, we welcome all submissions and feedback, and will do our utmost to involve your valuable contribution to the future of this technology.

The project leaders of this repository are very experienced developers and have been professionals for many decades. You should be in good hands.

Our goal here is to enjoy the creation of useful and effective software that benefits many people in our community, and that puts emphasis on the quality of their experience over the effort to create that experience. If you want to be a contributor to this project, we hope you are similarly motivated.

### What we are looking for

Essentially, we are building high-quality products (tools) for other developers to make their programming lives better.

There is no suggestion or contribution that you could make that would be too minor or meaningless.

We are all human and we all make mistakes every day, so even minor typos and corrections to any part of this product are always more than welcome.

Improving the clarity of the code and of the documentation is very welcome and super important. No one person can get this right on their own. They only have one perspective, and we are all biased and shaped by our own experiences. And none of us is (or looks like) the end-user of the product we produce. So if you can help shape this product to improve the end user's experience of it, please make a contribution small or large.

There are many ways to contribute to this project:

* From creating videos or writing tutorials or blog posts,
* improving the documentation,
* submitting bug reports, and
* creating feature requests or writing code that can be incorporated into the technology itself.

## Support Questions

If you have questions about the use of this technology, then, please don't use the Issues register on this project site for that purpose.

The Issues register is a place to address bugs and feature requests in the improvement of the automate product itself.

> You can certainly ask questions for clarity on how things work, in the design of automate



Use one of the following resources for questions about using automate or issues with your own code:

- The `#questions` channel on our [Discord server](https://discord.gg/vpc3gDPR)
- Ask on [Stack Overflow](https://stackoverflow.com/questions/tagged/automate?tab=Frequent). Search with Google first using: `site:stackoverflow.com automate {search term, exception message, etc.}`
- Ask on our [GitHub Discussions](https://github.com/jezzsantos/automate/discussions) for long-term discussion or larger questions.

# Code of Conduct

The goal is to maintain a diverse community that's pleasant for everyone. That's why we would greatly appreciate it if everyone contributing to and interacting with the community also followed this [Code of Conduct](CODE_OF_CONDUCT.md).

The Code of Conduct covers our behavior as members of the community, in any forum, mailing list, wiki, website, Internet relay chat (IRC), public meeting, or private correspondence.

> Our Code of Conduct is adapted from the [Contributor Covenant](https://www.contributor-covenant.org/version/2/0/code_of_conduct.html) version 2.

### Be considerate

Your work will be used by other people, and you in turn will depend on the work of others. Any decision you take will affect users and colleagues, and we expect you to take those consequences into account when making decisions. Even if it's not obvious at the time, our contributions to automate will impact the work of others. For example, changes to code, infrastructure, policy, documentation and translations during a release may negatively impact others' work.

### Be respectful

The automate community and its members treat one another with respect. Everyone can make a valuable contribution to automate. We may not always agree, but disagreement is no excuse for poor behavior and poor manners. We might all experience some frustration now and then, but we cannot allow that frustration to turn into a personal attack. It's important to remember that a community where people feel uncomfortable or threatened isn't a productive one. We expect members of the automate community to be respectful when dealing with other contributors as well as with people outside the automate project and with users of automate.

### Be collaborative

Collaboration is central to automate and to the larger free software community. We should always be open to collaboration. Your work should be done transparently and patches from automate should be given back to the community when they're made, not just when the distribution releases. If you wish to work on new code for existing upstream projects, at least keep those projects informed of your ideas and progress. It may not be possible to get consensus from upstream, or even from your colleagues about the correct implementation for an idea, so don't feel obliged to have that agreement before you begin, but at least keep the outside world informed of your work, and publish your work in a way that allows outsiders to test, discuss, and contribute to your efforts.

### When you disagree, consult others

Disagreements, both political and technical, happen all the time and the automate community is no exception. It's important that we resolve disagreements and differing views constructively and with the help of the community and community process. If you really want to go a different way, then we encourage you to make a derivative distribution or alternate set of packages that still build on the work we've done to utilize as common of a core as possible.

### When you're unsure, ask for help

Nobody knows everything, and nobody is expected to be perfect. Asking questions avoids many problems down the road, and so questions are encouraged. Those who are asked questions should be responsive and helpful. However, when asking a question, care must be taken to do so in an appropriate forum.

### Step down considerately

Developers on every project come and go, and automate is no different. When you leave or disengage from the project, in whole or in part, we ask that you do so in a way that minimizes disruption to the project. This means you should tell people you're leaving and take the proper steps to ensure that others can pick up where you left off.

### Set expectations for behavior (yours, and theirs).

This includes not just how to communicate with others (being respectful, considerate, etc) but also technical responsibilities (importance of testing, project dependencies, etc). Mention and link to your code of conduct, if you have one.

# Ground Rules for Contributing Code

Your Responsibilities are:

* Ensure cross-platform compatibility for every change that's accepted. Windows, Mac, Debian & Ubuntu Linux.
* Ensure that all functional code that goes into automate meets all these requirements
    * The change you make compiles, and a final package can still be built from the changed codebase
    * You have unit tests and integration tests to demonstrate how the change is working correctly. They must all be run and pass.
    * Your code is formatted using the configured formatting/linting tools.
    * Your code is self-documenting; the intent of which is easily understandable by other contributors. Comments are not the mechanism to share what the code does, or how.
    * Dead code should be removed. Do not include unused commented-out sections of code.
    * Your code does not break the build, and passes all checks enforced by GitHub Actions.

* Architectural decisions should be discussed with other contributors.
* Create issues for any major changes and enhancements that you wish to make. Discuss things transparently and get community feedback.
* Keep feature versions as small as possible, preferably one new feature per version.
* There are no specific conventions for formatting commit messages, except to ensure that they are meaningful descriptions of the history of change to the codebase. Describe the overall impact of your change, rather than describing the change you made. Make sure to include a reference to any Issue that the work relates to (e.g. `Closes #12` or `Fixes #56` or reference a discussion or design `#45`)
* Coding style and formatting rules will be included in the source code. The development tools that we are using (e.g. Jetbrains Rider and IntelliJ) can be used to apply and enforce them. If you are using other tools, (which is fine) you will need to find a way to apply these tools before submitting your code.

# Documentation

## Product Documentation

[This is where](https://jezzsantos.github.io/automate/) we have all of our product documentation, aimed at the users of our product, which in this case are primarily developers themselves.

This includes documentation for the core product of automate, our CLI interfaces and our plugins.

We build that documentation using [MkDocs](https://www.mkdocs.org/) from the source documentation files located in the: `docs` folder. The `mkdocs.yml` file is in the root of the repository.

## Contributors Documentation

[This is where](https://jezzsantos.github.io/automate/contributors) we have all of our contributor documentation, aimed at the developers of our product. Our users are not likely to care about this documentation.

This includes documentation for the design of all components of automate.

We build that documentation using [MkDocs](https://www.mkdocs.org/) from the source documentation files located in the: `docs` folder. The `mkdocs.yml` file is in the root of the repository.

## Building and viewing the docs locally

In a terminal, in the root directory of the repo, run the following commands:

* Install [MkDocs](https://www.mkdocs.org/user-guide/installation/)
* `pip install mkdocs-material`
* `pip install mkdocs-glightbox`

Now to see the site locally: `mkdocs serve`

> This command will run the docs locally at `http://127.0.0.1:8000/automate/`

## Publishing the docs

We compile and publish those docs automatically (with a special GitHub action step) that is run when any commit to the `main` branch of the repo is made. That compiles and uploads the compiled documentation to the `gh-pages` branch of the repo.

> This process is the standard process for all GitHub projects using GitHub pages with MkDocs. See [MkDocs](https://www.mkdocs.org/user-guide/deploying-your-docs/) for more details on how that works.

# Building & testing the Code

## Environment

We recommend using JetBrains Rider to develop this codebase.

> Included in the codebase there are many formatting rules that need to be run before committing your code. These rules may not be supported by other developer tools.

> It is possible to apply for a free license for Rider from Jetbrains, for OSS projects

* Clone the repo locally
* Ensure that you have `.NET6.0` installed on your system. [Download](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* Open the `src/Automate.sln` solution

## Building the code

* Build the solution
* or run `dotnet build` on the `src/Automate.sln` solution

There are 2 flavors of build configuration: `Debug` and `Release`, these are very standard build configurations, and only the `Release` configuration has been enhanced.

* `Debug` includes symbols for debugging, `Release` does not

* `Debug` includes a conditional compilation variable `#if...#endif`called `TESTINGONLY` that is used to include code specifically for testing purposes only - not for publishing. This code is removed by the compiler in the `Release` flavor.

* `Release` includes an additional MSBUILD target that installs the NuGet packages locally on your machine.

  > Since the CLI interface is a NuGet tool, you might want it installed (and kept up to date) on your machine for use in other projects or for manual testing.

> The `Release` build flavor must pass to submit changes to the codebase

## Testing the code

* Run all the **unit** tests (`Category=Unit`)
* Run all the **integration** tests (`Category=Integration`)

> All automated tests must pass to submit changes to the codebase

## Integration

When you push your changes (or push your pull requests), they will be built and tested by GitHub Actions automatically.

> The build must pass to submit changes to the codebase

## Publishing

> Versioning and Publishing (publicly) are performed by core contributors only.

### Versioning

> We use [SemVer](https://www.semver.org) rules for publishing releases (Major for breaking, Minor and Patch for Non-Breaking).
>
> If this is a 'pre-release' version (using `-preview`) we only ever increase the Minor number for breaking changes.

1. In `src/Directory.Build.props`, update the `<VersionPrefix>x.y.z</VersionPrefix>` (and optionally `<VersionSuffix>-preview</VersionSuffix>`)
2. Commit message `#vx.y.z` (or `#vx.y.z-preview`) 
3. Tag the commit `vx.y.z` (or `vx.y.z-preview`)
4. Push commit and tags

Wait until the latest build goes green, at which point:

1. A new [Github Draft Release](https://github.com/jezzsantos/automate/releases) has already been created
2. A new [NuGet release](https://www.nuget.org/packages/automate) has also already been published

**Final Step**: Open the [Draft Release](https://github.com/jezzsantos/automate/releases), check the details, and Publish it.

### Local Release

Either:

1. Build the solution in `Release` build configuration.

> In the 'Release' configuration the dotnet tool is built, packed, and installed.

OR:

1. Package the nuget: `dotnet pack --configuration Release /p:Version=x.y.z CLI/CLI.csproj`
2. Uninstall existing tool: `dotnet tool uninstall automate --global`
3. Install the local tool: `dotnet tool install automate --global --add-source CLI\nupkg --version x.y.z`

### Public Release

#### Nuget.org

The GitHub Action that has run in response to you pushing the tag, has already built and published the NuGet package to nuget.org.

https://www.nuget.org/packages/automate

MANUALLY:

1. Package the nuget: `dotnet pack --configuration Release /p:Version=x.y.z CLI/CLI.csproj`
2. Log into nuget.org, and upload the built package at: `src/CLI/nupkg`

#### Github.com

The GitHub Action that has run in response to you pushing the tag, has already created a GitHub release and published all artifacts.

MANUALLY:

1. Create a new Draft Release: https://github.com/jezzsantos/automate/releases
2. Set the release title: `vx.y.z` (or `vx.y.z-preview`)
3. Attach the build NuGet package from `src/CLI/nupkg`
4. Download the `Win-x64 Binary`, `Linux-64 Binary` and `OSX-64 Binary` built artifacts from the last successful build (of the #vx.y.z commit) at: https://github.com/jezzsantos/automate/actions
5. Attach those zip files to the draft release
6. Tick: pre-release
7. Publish release

# Your First Contribution

Unsure about how to start contributing to automate? You can start by tackling any of the Issues in the Issues register marked with either the `help-wanted` or `good-first-issue`.

Those kinds of issues should involve a small amount of time and work.

All other contributions are of course also welcome, should you wish to dive in deeper.

### First contribution to open source?

Here are a couple of friendly tutorials you can include: http://makeapullrequest.com/ and http://www.firsttimersonly.com/

> Working on your first Pull Request? You can learn how from this *free* series, [How to Contribute to an Open Source Project on GitHub](https://egghead.io/series/how-to-contribute-to-an-open-source-project-on-github).

# The Submission Process

All contributions should be submitted via a pull request or arranged directly with a core contributor on this project.

Core contributors will:

* Review your contribution (as quick as possible)
* Provide feedback if necessary (as accurate as possible)
* Provide direction on what to do next
* and integrate your contribution when it is accepted

After a core contributor has responded, contributors are expected to:

* Take an action within two weeks to act on any feedback from a core contributor, otherwise, core contributors may close the pull request if it is not showing any activity.

The core contributors [at present] are not working on this project full-time (they have other day jobs and families to raise), and they may live in other time-zones than you, so please allow a day or so to hear back from them. They know what it feels like to be ignored and so pledge to respond in some way as soon as possible.

> If you want to become a core contributor, and you have a good track record on this project, then contact the core contributors directly.

# Community

We have this [discord server](https://discord.gg/vpc3gDPR) to talk or discuss with people directly.
