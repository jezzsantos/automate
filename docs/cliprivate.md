# Additional (unpublished) CLI Options

These options are documented but not published, as they are for internal use only. 
They are hidden from users, but the contributors on this project still need to understand how they work.

## Tool Information

To integrate with the CLI (ie from a plugin) we need to know a little more information about it, rather than just the version.

To view detailed information about the CLI tool itself: `automate info`

!!! example
    ``` batch
    automate info --output-structured
    ```
    Will produce a structured output similar to this:
    ``` batch
    {
        "Info": [],
        "Output": [
            {
                "Message": "{Command} ({Location}, v{RuntimeVersion}) with, usage:{CollectUsage}",
                "Values": {
                    "Command": "automate",
                    "Location": "C:\\Users\\you\\.dotnet\\tools\\.store\\automate\\1.0.7\\automate\\1.0.7\\tools\\net6.0\\any",
                    "RuntimeVersion": "1.0.7",
                    "CollectUsage": {
                        "IsEnabled": true,
                        "MachineId": "mid_89d01e80abf04f3ab00c3f3139ec6ae5"
                    }
                }
            }
        ]
    }
    ```