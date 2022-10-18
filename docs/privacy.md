# automate Privacy Statement

!!! info
    Effective date: 26th June 2022

    Last updated: 19th October 2022 

## Thank you for entrusting us with your data.

This Privacy Statement explains our practices regarding the collection, use, and any disclosure of your data, including any personal data, that we may collect and process in connection with our software, websites and any applications, tools, products, and services.

## What we collect

We collect only directly from our software tools running on your computer, including information about your:

* Computer and network details (Your IP address, your operating system)
* A random generated unique identifier for each tool
    * Created once per computer that you install the software on
* Monikers of the features in the tools being used
* Identifiers from data that these tools have created
    * We use those identifiers only for correlation purposes between uses of these tools. 
    * Any identifiers that are collected are obscured from their original values (using MD5 cryptographic hashing - where the obscured values cannot be reverse engineered back to original identifiers).
* Names and version numbers of these tools

!!! note
    To the contributors of this project: This list will be kept up to date as the software changes

## What we do NOT collect

We do NOT collect:

* Any personal information about you (a.k.a PII data)
* Any information (of any kind) that can be traced back to you, as a person, or to your device.
* Any data you enter into these tools

## Why do we collect it?

We collect this information in order to:

* Understand individual usage of these tools
* Understand repeat usage of these tools, and what features are used between uses
* Understand common user patterns/flows/scenarios (across many users and uses)


## Where do we store the collected data?

* Data is stored locally on your computer first, and then uploaded to the cloud (when a network connection is made).
  * Local data is kept on your hard disk in same directory where the tool is installed.
  * We keep the uploaded usage data safe online in permanent storage in Azure.
* We only permit our core contributors access to it, and no other party.
* All collected data is automatically discarded after 90 days.

## Common Enquiries

### Can you see what we collect?

Yes 

You can see the kind of data we collect. 

These are open source tools. You can see what is collected in each of the calls (in the code) to the [`IMetricReporter.Count()`](https://github.com/jezzsantos/automate/blob/main/src/Core/Common/IRecorder.cs) method. There are several calls throughout the codebase. In each of these calls, you can see what is collected and how it is obscured to comply with this privacy policy. 

No

You cannot see the actual data we collect.

### Can you request the data we collect from you?

No

Since, we collect no personal information from you, there is no practical way to know what data was created by you and not some body else.

### Can you opt-out of collecting this data?

Yes

You always have the option to opt-out of this data collection in our tools. See the [Collecting Usage Data](clioptions.md#collecting-usage-data)