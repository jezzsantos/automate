# Additional CLI Options

## Version

To view the version of automate: `automate --version`

## Structured Output

### The problem with non-structured output

By default, all output from the CLI is displayed in a human-readable and friendly format (in stdout). All exceptions from the CLI are displayed in human-readable and friendly format (in stderr).

For a terminal user, all errors that occur (in stderr) are displayed inline with any Output (in stdout) in a terminal window.

This human-readable format often embeds key information (i.e. IDs of created elements).

These identifiers, are sometimes needed to be passed to subsequent commands by the end-user (and by any tools using the
CLI).

!!! tip
    The end-user is free to copy and paste these values from the terminal, but the machine would have to scrape the values from messages.

Consuming key data from the output (and from the errors) by scripts and tools can be very difficult. We want to enable these tools to be able to reliably parse the outputs and errors in order to chain tools together. So we offer the Structured output option to provide that data in machine-readable format.

### Enabling structured out

To receive structured output from any command, add the `--output-structured` option

!!! info
    This command will display all output AND errors into separate JSON responses in both stdout and in stderr.

#### Successful responses

In response to a command that succeeds, you will see a JSON response like this (in stdout):

```json
{
  "Info" : [
    "acontextualmessage1",
    "acontextualmessage2"
  ],
  "Output" : [{
    "Message" : "a message template with some value {AName1} and with some data {AName2}",
    "Values" : {
      "AName1": "avalue",
      "AName2": {
        "AKey1" : "value1",
        "AKey2" : "value2"
      }
    }
  }]
}
```

* The `Info` element contains any number of contextual messages about the command being run. For example, on which pattern or draft is the current one.
* The `Output` element contains one or more structured messages, composed of a `Message` template, and collection
  of `Values` that are used in that message template. These values may be strings, or maybe JSON documents.

!!! info
    The `Error` element will not present

#### Failure responses

In response to a command that fails with an exception, you will see a JSON response like this (in stderr):

```json
{
 "Info" : [
    "acontextualmessage1",
    "acontextualmessage2"
  ],
  "Error": {
    "Message" : "an error message"
  },
  "Output" : []
}
```

* The `Info` element contains any number of contextual messages about the command being run. For example, on which pattern or draft is the current one.
* The `Error` element contains the error `Message`
* The `Output` element may contain one or more structured messages ([as above](#successful-responses)), if there is any
  output before the error was raised.

## Debug Output

If you receive an error running any command, you can view extra information about the stack-trace of the error in detail to better understand the root cause.

!!! info
    This feature is only really useful to the developers of the automate project

To view more details about the error, add the: `--debug` option