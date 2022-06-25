# Additional CLI Options

## Version

To view the version of automate: `automate --version`

## Structured Output

By default, all output (Output Channel of a CLI) from the CLI is displayed in a human-readable friendly format. This format often embeds key information (i.e. IDs of created elements). These identifiers might be needed to be passed to subsequent commands, by the end-user.

> The end-user is free to copy and paste these values from the terminal.

By default, all errors that occur (Error Channel of a CLI) are displayed inline with any Output. 

Consuming this output (and errors) by other scripts and tools can be difficult. We want to avoid these tools from having to parse the outputs and errors in order to chain tools together. So we offer the Structure Output option to provide that data in machine-readable format.

To receive structured output from any command, add the `--output-structured` option

> This command will display all output and errors combined into a single JSON document.

For example:

```json
{
  "error": {
    "message" : "...an error message..."
  },
  "output" : {
    "message" : "a message template with {A}, {B} and {C}",
    "values" : [
      "avalueofa",
      "avalueofb",
      "avalueofc"
    ]
  }
}
```

## Debug Output

If you receive an error running any command, you can view the stacktrace of the error in detail, if you wish to understand the root cause. 

> This feature is only really useful to the developers of the CLI project

To view more details about the error, add the: `--debug` option