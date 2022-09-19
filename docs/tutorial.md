# Your First Toolkit

This tutorial will walk you through building a very basic toolkit, and then using it on your own codebase.

## Make your first pattern

!!! info
    For this example, we are going to assume that you have the following arbitrary files and folders in the current directory (where you are running this tool). You can adjust the commands below to suit the files of your specific codebase (there is nothing specific about the specific files we show you here, they can be anything you want).

```
    .
    ├ 📂Controllers/
    │ └ 📜BookingController.cs
```

---

1. Install the [automate CLI](started.md#installation)

2. Navigate to the root of a codebase of yours (e.g. `C:\myprojects\myproject\src`)

3. Harvest your first pattern from it:

    * `automate create pattern "MyPattern1"`

4. Add attributes, code templates, and automation to make it programmable:

    * `automate edit add-attribute "Name" --isrequired`
    * `automate edit add-codetemplate "Controllers/BookingController.cs"`
    * `automate edit add-codetemplate-command "CodeTemplate1" --targetpath "~/Controllers/{{Name}}Controller.cs"`
    * `automate edit add-command-launchpoint "*" --name "Generate"`

5. Edit the contents of the code template:

   * `automate edit code-template "CodeTemplate1" --with "notepad.exe"`
     > Or use another editor like VS Code [on Windows]: `%localappdata%\Programs\Microsoft VS Code\code.exe`
   * Add this snippet in this file: `Here is the {{Name}}Contoller`
   * Save the file

6. Publish a toolkit for this pattern to share with others on your team:

    * `automate publish toolkit`
      > This will build a versioned self-contained toolkit file, and it will publish it to the desktop on your machine, and also provide you with the command to install it into this codebase.

7. Share the toolkit file (`MyPattern1_0.1.0.toolkit`) over email/slack/etc with someone on your team.

---

## Using your first toolkit

Now, that person that you shared the toolkit with (or you yourself), can:

1. Install the [automate CLI](started.md#installation)

2. Install the toolkit, fetched from above (i.e email/slack/etc):

    * `automate install toolkit "<DOWNLOADLOCATION>/MyPattern1_0.1.0.toolkit"`

3. Create a new 'Draft' from this toolkit:

    * `automate run toolkit "MyPattern1" --name "Demo"`

4. Configure it

    * `automate configure on "{MyPattern1}" --and-set "Name=Banana"`

5. Now run the toolkit:

    * `automate execute command "Generate"`

   This command will generate the code from the code template into the destination folder (`--targetpath`), using the value of the `Name` property which you configured as `Banana`

If you now open that generated file, you will also see the value of `Banana` in the contents of the file.

---

## Upgrading your first toolkit

This is the process if you want to make any changes to the toolkit.

In the location where you created the original toolkit:

1. Add a new attribute to your toolkit:

    * `automate edit add-attribute "Color" --isrequired -isoneof "Red;Green;Blue"`

2. Update the code in the code template:

    * `automate edit code-template "CodeTemplate1" --with "notepad.exe"`
    * Add the `{{Color}}` substitution to the content of this edited file: `Here is the {{Name}}Contoller it is {{Color}}`
    * Save the file

3. Rebuild the toolkit:

    * `automate publish toolkit`

Now back in the target codebase:

1. Upgrade the toolkit:

    * `automate install toolkit "<DESKTOPLOCATION>/MyPattern1_0.2.0.toolkit"`

2. Upgrade your current draft:

    * `automate upgrade draft`

3. Configure the new color property:

    * `automate configure on "{MyPattern1}" --and-set "Color=Green"`

4. Re-run the toolkit:

    * `automate execute command "Generate"`

If you now open that generated file, you will also see the value of `Green` in the contents of the file.