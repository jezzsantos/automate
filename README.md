[![Build and Test](https://github.com/jezzsantos/automate/actions/workflows/build.yml/badge.svg)](https://github.com/jezzsantos/automate/actions/workflows/build.yml)

      ┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐
      ├─┤│ │ │ │ ││││├─┤ │ ├┤ 
      ┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘

**Question to a Developer:**

What if I gave you a command-line tool that wrote a lot of your code for you? and it took about ~10 command-line commands (or so)? for you to build out an API endpoint or all the scaffolding for a UI page?

* This tool would be used _once per_ API or once per UI page.

**Question to the Tech Lead on that codebase:**

What if I gave you a command-line tool that built that other command-line tool for that developer? and it took about ~15 commands-line commands (or so) for you to build it?

* You would only need to do this _once per codebase_ that you work together on.

What if that command-line tool was updated (by you) as the codebase evolved? And it refactored and automatically fixed the code for the developer automatically over time?

What if you had a library of these kinds of tools (for this codebase) stored in that codebase?

## What is Automate?

Automate is a tool for Tech Leads, Lead Developers or Tech Consultants to give their fellow codebase contributors dev tools that capture and apply their coding patterns consistently across to their codebases as they evolve and change.

## How does it work?

It works like this:

1. You identify a *pattern* in your codebase. eg. It might be a layer pattern (an interface) or a vertical slice pattern (a domain pattern), or a way to compose certain components together that is done similarly in multiple places.
2. You pick (from your codebase) the files and folders where the code exists for this pattern right now.
3. Those files are automatically extracted for you into a *template*.
4. You name the pattern, and you define some *attributes* for it. The things that could be variable in it when it gets used elsewhere in the codebase. (e.g.: the names of functions, classes, types, files, folders in the pattern, etc.)
5. You modify the templates to add *substitutions* (and calculations) for the attributes that you defined, which will be executed when the template is *rendered*.
6. You can define automation *commands* that can be executed on the codebase before and after the template is applied. e.g., you want to run some automated tests to verify the code has not been broken.
7. Optionally, you define *constraints* about where the pattern can be reused in a codebase (e.g., only in certain languages, etc.)
8. You build your pattern. And a *toolkit* is automatically built for you containing this template and containing a custom executable tool (CLI or IDE plugin) to help apply it to other codebases.
10. You ship this toolkit to your dev teams to install into their codebase (or you add it to source control in an existing codebase they are using)
11. They construct an instance of the template and fill values for the attributes that were defined in it, configured for their specific use case and their codebase.
12. The toolkit renders the template into code, files, folders, etc., into their codebase automatically.

### Dealing with change

At some point later, you will want to update the pattern. Refactor it, modify it, fix a defect in it, or just add new capabilities to it, etc.

1. You edit the pattern, edit or add more templates, optionally change the commands and change the constraints.
2. An upgraded toolkit version is created for you, automatically containing these upgrades.
2. You ship this upgraded toolkit to your team to use.
2. They download and run the upgraded toolkit (CLI or IDE plugin) and upgrade their codebase.
2. The toolkit automatically detects the previous files/folders that were written before and detects the previous attribute configuration that was used before. The toolkit re-applies the upgraded pattern. The codebase evolves.

### Does automate apply to you?

If these assumptions about your software team are **all true**, then you might consider taking a look at this tool.

- [ ] You work on a codebase with others - you are working in a software team.

- [ ] You are the Tech Lead/Lead Dev/Tech Consultant of the team. (or have some other well-respected position of authority in the team).
- [ ] You contribute code yourself to this codebase, and you often collaborate with others on your team about the code.
- [ ] You have some working coding patterns or can create some working coding patterns that are worth sharing in this specific codebase.
- [ ] Your team values: consistency, clarity, and maintainability.
- [ ] You accept that you can be wrong sometimes and that your codebase necessarily changes over time.

If this sounds like your situation, then maybe we can help you define (and enforce) some codebase-specific coding patterns for your team to reuse.

# Getting Started

See our [Documentation](https://github.com/jezzsantos/automate/wiki/Documentation) for more details on each of the commands below.

1. Install the **automate** tool:
    * Platform installers are all here: `https://github.com/jezzsantos/automate/releases`
    * Otherwise, if .NET installed: `dotnet tool install --global automate`

2. Navigate to your codebase, and harvest your first pattern from it:

   `automate create pattern "MyPattern1"`

3. Add attributes, code templates, and automation to make it programmable:

    * `automate edit add-attribute "Name" --isrequired`
    * `automate edit add-codetemplate "Controllers/BookingController.cs"`
    * `automate edit add-codetemplate-command "CodeTemplate1" --targetpath "~/Controllers/{{Name}}Controller.cs"`
    * `automate edit add-command-launchpoint "*" --name "Generate"`

4. Edit the contents of the code template:

* `automate edit code-template "CodeTemplate1" --with "notepad.exe"`
* Add the following snippet somewhere in this file:
  ```
  Here is the {{Name}}Contoller.
  ```
* Save the file

5. Generate the toolkit for this pattern to share with others on your team:

   `automate build toolkit`

   > this will build, version a self-contained toolkit file, and will export it to the desktop on your machine.

6. Share the toolkit file (`MyPattern1_0.1.0.toolkit`) over email/slack/etc with someone on your team.

---

Now, that person (or you), can:

1. Install the **automate** tool:
    * Installers are here: `https://github.com/jezzsantos/automate/releases`
    * Otherwise, if .NET installed: `dotnet tool install --global automate`

2. Install the toolkit, fetched from above (i.e email/slack/etc):

* `automate install toolkit "<DOWNLOADLOCATION>/MyPattern1_0.1.0.toolkit"`

3. Create a new 'Draft' from this installed toolkit:

* `automate configure on "{MyPattern1}" --and-set "Name=Banana"`

4. Now run the automation:

* `automate execute command "Generate"` this command will generate the code from the code template into the destination folder (`--targetpath`), using the value of the `Name` property which you configured as `Banana`
* If you now open that generated file, you will see the value of `Banana` in the contents of the file.

---

The pattern and code templates can now be edited (by its author), and the toolkit can be rebuilt and redeployed (by its author).

It can be reinstalled (by its user) to rewrite any new and improved code, using the changes in the toolkit.

# Learn more

At present, we are in the validation stage. Validation, viability, and research work are captured in the `docs` and `discovery` folders.

Keen to see an example? There is a demo toolkit with a walk-through available which you can follow: [demo](discovery\demo\Demo Script.md)
