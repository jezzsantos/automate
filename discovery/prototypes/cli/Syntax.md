# CLI Syntax Example

This is a **paper prototype** of a CLI that could be used to deliver the core capabilities of autōmate.

## The Use Case

A software team is about to embark on building a new SaaS API product/service for their company `Acme`.

The new product/service is called `RoadRunner`.

They have prior experience building such APIs and have access to a number of codebases where they have done kind of work before.

> We assume for now that: those codebases are either existing in their company already (from prior APIs products/services) and that were built by this tech lead, or from open source projects the tech lead might be familiar with. Either way, the tech lead has access to these existing codebases, they are familiar with them, and they contain patterns that the tech lead wishes to use in the new product codebase.

The Tech Lead knows that the code in those existing codebases is largely what they want to replicate in this new `RoadRunner` API, and the tech lead desires NOT to have the wheel re-invented by other contributors on their team. At the same time, this new `RoadRunner` API will be based upon patterns that are very slightly different from the existing APIs for a couple of reasons:

1. This new `RoadRunner` API does NOT need some of the details/aspects/interfaces/abstractions that the existing API codebases have, because the `RoadRunner` team wants to get started faster, and not have to deal with those kinds of technical problems/constraints just yet. They will evolve. Some of these details are assumed to be more important later on in the development of this API, but NOT now. This implies some extra work untangling some of the coding patterns of the existing codebases, which will lead to some extra experimentation by the tech lead.
2. This new API is going to improve on the old API patterns (in the existing codebases) to deal with some of the accumulated technical debt that existed in the previous codebases. Now, seems like a good time and opportunity to address them. This implies some extra experimentation and iteration to improve on those old patterns/capabilities by the tech lead.
3. Existing patterns in existing codebases are solid enough and well-proven in production already, and the tech lead wants to reuse them on this new `RoadRunner` API. Furthermore, the `RoadRunner` team is slightly different and contains contributors that are not as familiar with these patterns as the tech lead.

### Problem Statement

How can the tech lead *quickly and easily* capture the coding patterns they think should be re-used in the new codebase, and *make it easy* for the contributors in their new team to use them reliably and consistently. So that they all (tech lead and contributors) *can focus on* where the real learning needs to be in the new `RoadRunner` product/service?

> We will use the word *Conceptual* to describe the idea in your mind.

> We will use the word *Logical* to describe the realized model of the idea (in practice, with practical constraints considered).

## The New Process

This is how that tech lead (on the `RoadRunner` team) could go about using the **autōmate** to get the job done.

> It is not important at this stage what programming language we are using to demonstrate this example. It could be in C# building a .Net5 ASP/NET app, or it could be JavaScript building a NodeJS app, or Java files building a Spring app. By necessity we need to give examples in code, so for now, we will be demonstrating this in C# ASP.NET MVC in Net5. The CLI itself is technology agnostic and can perform the same tasks no matter what programming language is ultimately used.

Assumptions about `Acme` APIs:

* An API at `Acme` is implemented with common conventions and patterns developed that have already been evolving at `Acme`.
* The spoken/written language that describes how APIs are designed/constructed, is common (enough) across the engineering teams building them.
* At `Acme` API products/services are usually built with patterns that loosely follow a ports and adapters architecture (Hexagonal/Onion/Clean architecture), use CQRS-like patterns, and use some domain-driven design at its core.

When a new API product/service is built at `Acme`, it involves building these concepts/terms in code (in bold), and is logically put together like this:

* The code defining a web API interface is defined in the **Infrastructure Layer**.
* An API is segregated by a logical **Resource** (as in, a REST resource)
* Each Resource has one or more **Service Operations** (i.e. endpoints)
* Each service operation has a **Request DTO**, and a **Response DTO**. Each of these DTOs has one or more **Fields**. These fields make up a simple dictionary for requests and can be more complex nested dictionaries for responses. All request/response DTOs are serializable and can be serialized to JSON on the wire (and other common formats: CSV, XML, etc).
* Each service operation has a **Route** (multiple can be defined).
* Each service operation may be **Authorized** with a token or not authorized at all (anonymous).
* Optionally, each service operation may cache its response.
* Optionally, each service operation may apply a rate-limiting policy (eg. per user, per timeframe, etc).
* Each service operation corresponds to a method call to an **Application Interface** in the **Application Layer**. The call from service operation to the application interface converts all request/response DTO data into shared application-level DTOs, extracts the user's identifier from the Authorization token, and passes through all of that data to the application interface. On the way back, exceptions are converted to appropriate HTTP status codes (and descriptions), and all data is converted into response DTOs. The only data in and out of the application interface are DTOs that are declared by the Application Layer.
* The Application Layer is defined by **bounded contexts**. One Application Interface per bounded context. (these may or may not reflect directly the REST resources)
* The Application Interface retrieves a Root Aggregate (for this bounded context) from a persistence store and operates on that aggregate (for Command operations). The Application Interface retrieves data directly from persistence via Read Models (for Query operations). The code for each of these kinds of operations has some *loose* patterns, but none of those patterns can be predicted well enough and must be hand crafted by a contributor on the team.
* The Infrastructure Layer also contains adapters to persistence stores via **Repositories**, **Read Models**, and other **Service Client** adapters to external HTTP services that are also necessarily hand crafted.
* The **Domain Layer** necessarily contains hand-crafted **Aggregates**, **Value Objects**, **Domain Services**, etc.

> Note: all the terms in bold above, are terms that all contributors on API codebases at `Acme` understand in their mental model of building an API. These terms are often encoded into the names and into the structure of codebases built at Acme. They are part of the language of building APIs at `Acme` and are used in all communicating the design of API products/services.

### Step 1 - Capture a basic pattern

The tech lead now intends to provide some custom tooling to their team to help build out new APIs at `Acme`, using the logical programming model described earlier.

On the command line, navigate to the source code directory of the new product called `RoadRunner`. eg. `cd C:/projects/acme/roadrunner/src`

`automate create pattern "AcmeAPI"`

> This command registers a new pattern called `AcmeAPI` in the current directory. It saves a bunch of files defining the pattern, which could be added to source control in the `C:/projects/acme/roadrunner/src/automate` directory.

Now, the tech lead will want to extract coding patterns that exist in the existing codebases. For example, the last codebase they worked on, rooted at: `C:/projects/acme/coyote/src/`.

These patterns may exist in one file, or they may exist across several files in multiple directories. We will assume here that the pattern is spread across multiple files, in different directories.

The tech lead identifies all the files (that each contains a fragment of the pattern). At this stage, the tech lead has an idea of the complete pattern in their head and uses their editing tools to view it as a whole.

All they need to know at this point is which set of files the pattern exists within.

Let's assume for this example, that the fragments of this pattern exist in all these files of the `Coyote` codebase:

```
- coyote
	- src
		- backend
			- controllers
				- BookingController.cs
			- services
				- BookingService.cs
				- IBookingService.cs
			- data
				- Bookings.cs
```

For each file that contains a fragment of the pattern, they execute this command:

`automate edit add-codetemplate "<file-path>"`

> This command registers a "Code Template" on the `AcmeAPI` pattern, which will be automatically named like `CodeTemplate1`, which contains all the code from the file at the specified relative `<file-path>`.
>
> You can set the name of your choice with the `--name` parameter.

For this codebase, we will need four similar commands:

* `automate edit add-codetemplate "backend/controllers/BookingController.cs"`
* `automate edit add-codetemplate "backend/services/BookingService.cs"`
* `automate edit add-codetemplate "backend/controllers/IBookingService.cs"`
* `automate edit add-codetemplate "backend/data/Bookings.cs"`

> This means now that conceptually, the pattern called `AcmeAPI` has a root element (called `AcmeAPI`) with four code templates called `CodeTemplate1` `CodeTemplate2`, `CodeTemplate3` and `CodeTemplate4`, each containing code that we will later templatize for our pattern.

So far, we don't have much, just a new pattern called `AcmeAPI`, and four code templates.

Conceptually, it would look like this, if we drew it as a logical structure.

```
- AcmeAPI (root element, attached with 4 code templates)
```

### Step 2 - Define some attributes of the pattern

Before getting into the code, we need to step back and determine a conceptual model of the code.

Now that the tech lead has defined a new *pattern* to conceptually represent building a new API (for new products at `Acme`), the tech lead now needs to figure out how to allow a contributor (on their team) to customize this specific pattern to be useful in a new product like the `RoadRunner` API their team will be building.

Obviously, a contributor (on the `RoadRunner` team) is going to need to define their own APIs for the `RoadRunner` product (they are not likely to want a `Bookings` API), and they are not going to want to copy and paste the `Booking` API that was extracted from the `Coyote` codebase, and now lives in the 3x code templates. These code templates are not yet reusable.

The tech lead has to identify, **what will be different** between the `Booking` API of the `Coyote` codebase, and any new API being built in the next codebases.

> Clearly, a bunch of things will need to be different.
>
> * (1) The names of the files (and possibly directories) produced from this pattern will need to be different, and
> * (2) so will the actual code in those files. Including the names of code constructs like classes and methods in those files.
> * But also (3) there are also a whole bunch of technical details that can vary between APIs.

The tech lead decides that the contributor will have to at least give a name for the new API, (equivalent to `Bookings` from the `Coyote` API) because that name influences the naming of the files of the logical `Controller`, `Service Interface`, and `Service Implementation` files which are also classes that need to be created to contain the code specific to this new API. They also decide that they need a singular name for the resource of the API (e.g. `Booking` from the `Coyote` API)

So, in order to capture the name from the contributor, the tech lead defines a mandatory attribute on the pattern called: `ResourceName`

`automate edit add-attribute "ResourceName" --isrequired`

Then, the tech lead knows that every new API logically contains multiple "Service Operations" (according to existing coding patterns).

So, they add a collection to the pattern to allow their contributors to define multiple service operations.

`automate edit add-collection "ServiceOperation" --displayedas "Operations" --describedas "The service operations of the web API" `

> Notice here that the tech lead decided to name this collection with a name of `ServiceOperation` and give it a meaningful display name and description. Those are only used for display purposes.

> The name of a collection or element cannot contain spaces, since it is an identifier.

> Collections are created with a "Cardinality" of `ZeroOrMany` by default. This means that they can have any number of instances of them in the collection, and none at all. Elements have a cardinality of `One` they must have one instance, and only one instance. Other cardinalities can be also used on collections to apply different constraints, i.e. `ZeroOrOne`, `OneOrMany`.

And now, they add the necessary attributes of a "service operation":

`automate edit add-attribute "Name" --isrequired --aschildof "{AcmeAPI.ServiceOperation}"`

`automate edit add-attribute "Verb" --isrequired --isoneof "POST;PUT;GET;PATCH;DELETE" --aschildof "{AcmeAPI.ServiceOperation}"`

`automate edit add-attribute "Route" --isrequired --aschildof "{AcmeAPI.ServiceOperation}"`

`automate edit add-attribute "IsAuthorized" --isrequired --isoftype "bool" --defaultvalueis "true" --aschildof "{AcmeAPI.ServiceOperation}"`

> Note: The tech lead has deliberately ignored some of the optional behaviors of a "service operation" such as response caching and rate-limiting for this next API. Which is an example of the tech lead picking and choosing what to start their team with in the first iterations of the pattern, leaving room for evolving the pattern as the `RoadRunner` product matures.

Now, a service operation (conceptually) has an HTTP Request DTO and an HTTP Response DTO. These DTOs for now will just be dictionaries (for simplicity). So the tech lead would define both of those on the `ServiceOperation` as a "Collection" of `Field` elements with various attributes.

First the request DTO:

`automate edit add-element "Request" --describedas "The HTTP request" --aschildof "{AcmeAPI.ServiceOperation}"`

`automate edit add-collection "Field" --displayedas "Fields" --aschildof "{AcmeAPI.ServiceOperation.Request}"`

`automate edit add-attribute "Name" --isrequired --aschildof "{AcmeAPI.ServiceOperation.Request.Field}"`

`automate edit add-attribute "DataType" --isrequired --isoneof "string;int;decimal,bool;DateTime" --defaultvalueis "string" --aschildof "{AcmeAPI.ServiceOperation.Request.Field}"`

`automate edit add-attribute "IsOptional" --isrequired --isoftype "bool" --defaultvalueis "false" --aschildof "{AcmeAPI.ServiceOperation.Request.Field}"`

and similarly, for the Response DTO:

`automate edit add-element "Response" --describedas "The HTTP response" --aschildof "{AcmeAPI.ServiceOperation}"`

`automate edit add-collection "Field" --displayedas "Fields" --aschildof "{AcmeAPI.ServiceOperation.Response}"`

`automate edit add-attribute "Name" --isrequired --aschildof "{AcmeAPI.ServiceOperation.Response.Field}"`

`automate edit add-attribute "DataType" --isrequired --isoneof "string;int;decimal,bool;DateTime" --defaultvalueis "string" --aschildof "{AcmeAPI.ServiceOperation.Response.Field}"`

Okay, now that's a lot of commands to absorb in one shot.

So far, we are starting to build out our conceptual model.

It now looks like this:

```
- AcmeAPI (root element) (attached with 4 code templates)
    - ResourceName (attribute) (string, required)
    - ServiceOperation (collection)
            - Name (attribute) (string, required)
            - Verb (attribute) (string, required, oneof: POST;PUT;GET;PATCH;DELETE)
            - Route (attribute) (string, required)
            - IsAuthorized (attribute) (bool, required, default: true)
            - Request (element)
                    - Field (collection)
                            - Name (attribute) (string, required)
                            - DataType (attribute) (string, required, oneof: string;int;decimal,bool;DateTime, default: string)
                            - IsOptional (attribute) (bool, required, default: false)
            - Response (element)
                    - Field (collection)
                            - Name (attribute) (string, required)
                            - DataType (attribute) (string, required, oneof: string;int;decimal,bool;DateTime, default: string)
```

You can run this command to view your current configuration:

`automate view pattern`

So, with this conceptual *meta-model* of an API, a contributor on the `RoadRunner` product can now define any API in the `RoadRunner` product in terms of its `Name` its `ServiceOperations`, and its `Request` and `Response` DTO's.

This is all we need for the definition of the pattern.

The code that will be needed to be written into the codebase (C# classes, enums, interfaces, etc) that defines the Controller, the Service Interface, the Service Implementation, and the DTOs can now be derived from this meta-model. Their file names and directory structures can be derived from this meta-model too. With naming and structural conventions, the tech lead will define next.

### Step 3 - Templatize the Code

The next challenge is to templatize the code in the existing code templates to read the meta-model above producing the appropriate code in the right places in the codebase.

For this, the tech lead will need to update the code templates that they have already captured.

> Note: Each file that will be generated from each of the code templates will eventually need to be named and placed into the `RoadRunner` codebase in an appropriate directory, with an appropriate filename. That process will happen in the next step.

The tech lead, will now fire up their favorite text editor and modify all the code templates they harvested already.

> The code template files have been renamed and can be found in the following location: `C:\Projects\acme\roadrunner\src\automate\patterns\<PATTERNID>\CodeTemplates` where the `<PATTERID>` can be copied from a previous command the console output.

Either look at the CLI output to find out the name of the template that was created. Or you can run this command to list them:

`automate view pattern`

Edit each of the code templates in your favorite editor (like notepad or VSCode):

`automate edit codetemplate "CodeTemplate1" --with "automate edit codetemplate "Service" --with "%localappdata%\Programs\Microsoft VS Code\Code.exe"`

`automate edit codetemplate "CodeTemplate2" --with "%localappdata%\Programs\Microsoft VS Code\Code.exe"`

`automate edit codetemplate "CodeTemplate3" --with "%localappdata%\Programs\Microsoft VS Code\Code.exe"`

`automate edit codetemplate "CodeTemplate4" --with "%localappdata%\Programs\Microsoft VS Code\Code.exe"`

Make the following changes to them:

#### CodeTemplate1 - Controller

```
using System;
using Microsoft.AspNetCore.Mvc;

namespace Acme.RoadRunner.Controllers
{
{{~#Generate the Controller class~}}
	[ApiController]
	public class {{ResouceName | string.pascalsingular}}Controller : Controller
	{
{{~for operation in ServiceOperation.Items~}}
{{~if operation.IsAuthorized~}}
		[Authorize]
{{~else~}}
		[AllowAnonymous]
{{~end~}}
		[Http{{operation.verb}}]
		[Route( "{{operation.Route}}" )]
		public IActionResult {{operation.Name | string.pascalsingular}}({{operation.Name | string.pascalsingular}}Request request)
		{
			var resource = this.application.{{operation.Name | string.pascalsingular}}(request.ToDto());
			return OK(resource.ToResponse());
		}
{{~end~}}
	}
    
{{~#Generate the Request & Response classes~}}
{{~for operation in ServiceOperation.Items~}}
	public class {{operation.Name | string.pascalsingular}}Request
	{
{{~for field in operation.Request.Field.Items~}}
		public {{field.DataType}}{{if field.IsOptional}}?{{end}} {{field.Name | string.pascalsingular}} { get; set; }
{{~end~}}
	}

	public class {{operation.Name | string.pascalsingular}}Response
	{
{{~for field in operation.Response.Field.Items~}}
		public {{field.DataType}} {{field.Name | string.pascalsingular}} { get; set; }
{{~end~}}
	}
{{~end~}}

{{~#Generate the Application DTO classes~}}
{{~for operation in ServiceOperation.Items~}}
	public class {{operation.Name | string.pascalsingular}}
	{
{{~for field in operation.Request.Field.Items~}}
		public {{field.DataType}}{{if field.IsOptional}}?{{end}} {{field.Name | string.pascalsingular}} { get; set; }
{{~end~}}
	}

	public class {{ResourceName | string.pascalsingular}}
	{
{{~for field in operation.Response.Field.Items~}}
		public {{field.DataType}} {{field.Name | string.pascalsingular}} { get; set; }
{{~end~}}
	}
{{~end~}}

{{~#Generate DTO converters (using AutoMapper)~}}
	internal static class {{ResourceName | string.pascalsingular}}ConversionExtensions
	{
{{~for operation in ServiceOperation.Items~}}
			public static {{operation.Name | string.pascalsingular}} ToDto(this {{operation.Name | string.pascalsingular}}Request request)
			{
				return request.ConvertTo<{{operation.Name | string.pascalsingular}}>();
			}

			public static {{operation.Name | string.pascalsingular}}Response ToResponse(this {{ResourceName | string.pascalsingular}} dto)
			{
				return dto.ConvertTo<{{operation.Name | string.pascalsingular}}Response>();
			}
{{~end~}}
	}
}
```

#### CodeTemplate2 - Service Implementation

```
using System;

namespace Acme.RoadRunner.Services
{
	public class {{ResourceName | string.pascalsingular}}Service : I{{ResourceName | string.pascalsingular}}Service
	{
{{for operation in ServiceOperation.Items}}
		public {{operation.Name | string.pascalsingular}}Dto {{operation.name}}({{operation.Name | string.pascalsingular}}Request dto)
		{
			// Put your custom here and populate the return object
			return null;
		}
{{end}}
	}
}
```

#### CodeTemplate3 - Service Interface

```
using System;

namespace Acme.RoadRunner.Services
{
	public interface {{ResourceName | string.pascalsingular}}Service
	{
{{for operation in ServiceOperation.Items}}
		{{operation.Name | string.pascalsingular}}Dto {{operation.Name | string.pascalsingular}}({{operation.Name | string.pascalsingular}}Request dto);
{{end}}
	}
}
```

#### CodeTemplate4 - Application DTOs

```
using System;

namespace Acme.RoadRunner.DTOs
{
{{~#Generate the Application DTO classes~}}
{{~for operation in ServiceOperation.Items~}}
	public class {{operation.Name | string.pascalsingular}}
	{
{{~for field in operation.Request.Field.Items~}}
		public {{field.DataType}}{{if field.IsOptional}}?{{end}} {{field.Name | string.pascalsingular}} { get; set; }
{{~end~}}
	}

	public class {{ResourceName | string.pascalsingular}}
	{
{{~for field in operation.Response.Field.Items~}}
		public {{field.DataType}} {{field.Name | string.pascalsingular}} { get; set; }
{{~end~}}
	}
{{~end~}}
}
```

> Note: The actual code generated from these templates is just an example of how to write coding patterns using a text template language. The language used in the templates is the text-templating technology called [Scriban](https://github.com/scriban/scriban) which has its own templating language and syntax, similar to others (basically double `{{ }}` statements over the elements and attributes of the pattern).
>

As a tech lead develops these code templates they can test them by running some test data over them, and see the results. To do this they run the command like this: `automate test codetemplate "CodeTemplate1"` to see an example output.

### Step 4 - Add Code Generation Automation

Now that the tech lead has all the code templates modified, the last step is to define ***when*** the code templates are rendered, and ***where*** the generated code is placed in the codebase of the new `RoadRunner` product.

The tech lead decides that they will use the same directory structure and naming convention as used in the previous product `Coyote`.

However, the tech lead also knows that the `Service Implementation` class is likely to be written by hand by one of the contributors on the team. Whereas, the `Controller` class, the `Service Interface,` and the `DTO` classes can be generated in full.

Therefore the `Service Implementation` class will only be generated if the file does not already exist, and once it is generated, it won't need re-generating again, so that any custom code created by a contributor is not altered. Whereas the `Service Interface`, `Controller` and `DTO` files will always be generated and kept up to date, as and when the pattern changes.

The next thing to figure out is how to generate the files.

This can be done whenever some event on the meta-model is raised. For example, when new `Service Operations` are added (or changed) or perhaps when the codebase is compiled, or it can be done at any time by executing a command explicitly (or in fact all of those options).

In every case, a *Launch Point* needs to be defined and added to the pattern to execute the code template rendering. A Launch Point is just a user-based mechanism to execute actual automation commands that do the hard work.

These automation commands will decide **where** to render the files on disk, and what filenames to use.

`automate edit add-codetemplate-command "CodeTemplate1" --targetpath "~/backend/Controllers/{{name}}Controller.gen.cs"`

`automate edit add-codetemplate-command "CodeTemplate2" --isoneoff --targetpath "~/backend/Services/{{name}}Service.cs"`

`automate edit add-codetemplate-command "CodeTemplate3" --targetpath "~/backend/Services/I{{name}}Service.gen.cs"`

`automate edit add-codetemplate-command "CodeTemplate4" --targetpath "~/backend/Data/{{name}}.gen.cs"`

> These 4 statements add new "Commands" for each template to the root pattern element (`AcmeAPI`).
>
> Notice that the filename defined for each command uses an expression that includes the `name` attribute of the pattern. It is in lowercase here, as all attributes and element names will be snake_cased when this template is executed.
>
> Notice that for `CodeTemplate2` we use the option `--isoneoff`  (and a slight variation on the file extension in the `--targetpath` option) to indicate that this file will only be generated once, and only if the specified file does not exist at the specified location with the specified name.
>
> The `--targetpath` option starts with a tilde `~` indicating that we want the files to be rooted at the root directory of the pattern. Which for this case, is the current directory.

Now, that we have the four explicit commands to execute, we can define a single "Launch Point" that will be able to execute them all **when** we want:

`automate edit add-command-launchpoint "*" --name "Generate"`

> This command adds the "Launch Point" called `Generate` which will be executed on commands configured on the `AcmeAPI` element.

### Step 5 - Build the Toolkit, and Ship It

That's it for defining the pattern, and code templates.

Now the tech lead can build a toolkit from that pattern, and give it to their dev team.

`automate publish toolkit`

> This command creates a standalone (cross-platform) installer (`AcmeAPI_0.1.0.toolkit`) that will automatically be versioned and can now be installed by any contributor at Acme on any codebase.
>
> This package file will appear on your desktop, for you to install.

### Step 6 - Apply the Pattern

Navigate to the new `RoadRunner` codebase: `cd C:/projects/acme/roadrunner/src`

Download and install the new toolkit:

`automate install toolkit "<PATHTOTOOLKIT>"` where `<PATHTOTOOLKIT>` can be found in the console output of the previous command.

> This command installs the `AcmeAPI_0.1.0.toolkit` into the current directory, which in this case is `C:/projects/acme/roadrunner/src/automate/toolkits/<TOOLKITID>`

To list all the installed toolkits and versions of them:

`automate list toolkits`

> This cocommandists all the installed toolkits, installed in the current directory.

Now, it is time to switch hats and act as a contributor on the team.

Let's use this toolkit, and define a new API.

#### Creating a New API

For this example, let's call the new API that we want help building, the: `Orders` API, in the `RoadRunner` product/service.

* The new API will have one authorized POST operation called `CreateOrder` at `/orders`.
* This HTTP request takes a `ProductId` as the only request parameter
* It returns a completed `Order` with an `Id` property in the response.

To get started:

`automate run toolkit "AcmeAPI"`

> This command creates a new "draft" from the `AcmeAPI` toolkit, and returns its unique DRAFTID.

You can list all the solutions that have been created so far, with this command:

`automate list drafts`

> This command lists all the drafts created from toolkits in the current directory.

Now, lets program one of the drafts:

`automate configure on "{AcmeAPI}" --and-set "Name=Orders"`

> This command defines the value of the `ResourceName` for this draft of the pattern.

`automate configure add-one-to "{ServiceOperation}" --and-set "Name=CreateOrder" --and-set "Verb=Post" --and-set "Route=/orders" --and-set "IsAuthorized=true"`

> This command creates a new `ServiceOperation` instance and adds it to the `ServiceOperations` collection, and returns its unique OPERATIONID.
>

`automate configure add-one-to "{ServiceOperation.<OPERATIONID>.Request.Field}" --and-set "Name=ProductId" --and-set "DataType=string" --and-set "IsOptional=false"`

> This command creates a new `Field` in the Request DTO called `ProductId`

`automate configure add-one-to "{<OPERATIONID>.Response.Field}" --and-set "Name=Id" --and-set "DataType=string"`

> This command creates a new `Field` in the Response DTO called `Id`



After this set of commands, the draft is fully configured.

You can see the actual configuration of the draft, with this command:

`automate view draft`

This command should print out a JSON object that looks just like this:

```
{
    "Id": "xxxxxxxx",
    "ResourceName": "Orders",
    "ServiceOperation": {
        "Id": "xxxxxxxx",
        "Items": [
            {
                "Id": "xxxxxxxx",
                "name": "CreateOrder",
                "Verb": "Post",
                "Route": "/orders",
                "IsAuthorized": "true",
                "Request": {
                    "Id": "xxxxxxxx",
                    "Field": {
                        "Id": "xxxxxxxx",
                        "Items": [
                            {
                                "Id": "xxxxxxxx",
                                "Name": "ProductId",
                                "DataType": "string",
                                "IsOptional": "false"
                            }
                        ]
                    }
                },
                "Response": {
                    "Id": "xxxxxxxx",
                    "Field": {
                        "Id": "xxxxxxxx",
                        "Items": [
                            {
                                "Id": "xxxxxxxx",
                                "Name": "Id"
                                "DataType": "string"
                            }
                        ]
                    }
                }
            }
        ]
    }
}
```

> Notice, that the collections in the draft have an `Items` array containing their sub-elements.

A codebase contributor can now finally ask the toolkit to write the new API code for them!

`automate execute command "Generate"`

> This command executes the `Generate` Launch Point, which runs the configured code template commands, which in turn, generates the code files from each code template. The code is written into the codebase of the `RoadRunner` project in the respective locations.

> If any of the required properties (of attributes) are not set correctly, or any required elements of collections are missing, then appropriate validation errors will be displayed explaining the problem.

You can also manually validate the solution at any time, with this command:

`automate validate draft`

The `RoadRunner` codebase should now look like this:

```
- projects
	- acme
		- RoadRunner
			- src
				- automate
					- (...various directories and files)
				- backend
					- controllers
						- OrdersController.gen.cs
					- services
						- IOrdersService.gen.cs
						- OrdersService.cs
					- data
						- Orders.gen.cs
```

A codebase contributor will now need to manually write the missing code in the `OrdersService.cs` class, as this toolkit (as it is now) will not alter that code any further.

The codebase contributor could change the configuration of the Orders API in whatever way they like, and execute the command to update the code. Or they can add more service operations.

They can also use this toolkit to create another API, like `Customers` API.

The tech lead is free to modify or extend the pattern, add/remove/change anything they like, and then ship another version of the toolkit to their team. Where the toolkit will be updated, and code fixes can be applied.
