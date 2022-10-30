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

## Collecting Usage Data

To understand how people are using this tool (so that we can improve it) we collect information about the use of the tool.

### Disabling

You can opt-out of collecting usage data by using the `--collect-usage:false` option.

!!! important
    The intention of, and what is and is not collected about your usage, is described in far more detail in our [Privacy Statement](privacy.md)

### Correlation

As part of our strategy for collecting usage data (for analytics) we support the ability for a consumer of the CLI (eg. a plugin) to provide correlation information to better describe end to end successive usages of the CLI.

To provide a correlation ID:
``` batch
automate *any* --usage-correlation "<CORRELATIONID>"
```

- The `<CORRELATIONID>` is a 3 part identifier, where the three parts are: `<SESSIONID>`, `<OPERATIONID>` and `<PARENTOPERATIONID>` separated by the `|` character.
    - `<SESSIONID>` correlates all telemetry emitted across several successive calls to the CLI, the scope of this logical session is defined by the consumer.
    - `<OPERATIONID>` correlates all telemetry emitted across several successive calls to the CLI, the scope of this logical operation is defined by the consumer.
    - `<PARENTOPERATIONID>` provides a parent identifier to the top level logical operation created in the CLI.

!!! example
    ``` batch
    automate create pattern "APatternName" --usage-correlation "a_session_id|an_operation_id|a_parent_operation_id"
    ```

!!! info
    If the `--usage-correlation` option is not defined, the CLI will fabricate its own session identifier, and fabricate its own logical operation identifier, which it will use as a parent identifier to the operation scope that it creates every command.

    If the `--usage-correlation` option is defined as a single part identifier, the CLI will assume that identifier is a session identifier, and fabricate its own logical operation identifier, which it will use as a parent identifier to the operation scope that it creates every command.
    
    If the `--usage-correlation` option is defined as a three part identifier (as described above), the CLI will use the session identifier, and the operation identifier to correlate all its telemetry. It will use the parent operation identifier as a parent identifier to the operation scope that it creates every command.
