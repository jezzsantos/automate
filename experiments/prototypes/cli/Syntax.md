# CLI Syntax Example

## Context

The steps for the Tech Lead, with the CLI, could look like this.

A software team is about to embark on building a new API product/service for their company `Acme`. The new product/service is called `banana`.

They have prior experience building such APIs, and have access to codebases where they have done his before.

> We assume for now that: those codebases are either existing in their company already from prior APIs products/services that were built by this tech lead, or from open source projects the tech lead might be familiar with). Either way, they have access to these codebases, that they are familiar with.

The Tech Lead knows that the code in those existing codebases is largely what they want to replicate in this new `banana` API, and the tech lead desires NOT to re-invent the wheel, and at the same time, that this new `banana` API will be based upon patterns that are slightly different than the existing APIs for a couple of reasons:

1. This new `banana` API does NOT need some of the things/aspects/interfaces/abstractions that the existing API codebases have, because the `banana` team wants to get started faster, and not have to deal with those kinds of technical problems/constraints just yet. Some of which are assumed will be more important later on the development of the API, but NOT now. This implies some extra work untangling some patterns of the existing codebases, which will lead to some extra experimentation by the tech lead.
2. This new API is going to improve on the old API patterns (in the existing codebases) to deal with some of the accumulated technical debt that existed in the previous codebases. Now seems like a good time and opportunity to address them. This implies some extra experimentation and iteration to improve on those old patterns/capabilities by the tech lead.
3. Existing patterns in existing codebases are solid enough and well-proven in production, and the tech lead wants to reuse them on this new `banana` API, as the `banana` team is slightly different and contains contributors that are not so familiar with these patterns as the tech lead.

## The New Process

This is how that tech lead (on the `banana` team) would go about using the **autōmate** (prototypical CLI) to get the job done.

The job being to:

        **Give their new `banana` team some tools to accelerate the build out the new API codebase, based upon patterns that they harvested from existing API codebases.**

> It is not important at this stage what programming language we are building these API's in. It could be in C# building a .Net5 ASP/NET app, or it could be JavaScript building a NodeJS app, or Java files building a Spring app. By necessity we need to give examples in code, so for this example, we will be demonstrating this in C# ASP.NET MVC in .Net5.

Logically, an API is implemented with common conventions and patterns developed and adopted at `Acme`. These patterns loosely follow a ports and adapters architecture (Hexagonal/Onion/Clean architecture), uses CQRS-like patterns and uses some domain driven design.

When a new API is created at `Acme`, it involves building these concepts in code (in bold), and is logically put together like this:

* An API is defined in the **Infrastructure Layer**.
* An API is defined as containing multiple **Resources** (as in, as REST resource)
* Each Resource having one or more **Service Operations**
* Each service operation has a **Request DTO**, and a **Response DTO**. Each of these DTO's has one or more fields. These fields are a simple dictionary for requests, and can be more complex nested dictionaries for responses. All request/response DTO are serializable and can be serialised to JSON (and other common formats: CSV, XML, etc).
* Each service operation has a **Route** (can define multiple).
* Each service operation may be **Authorized** with a token or not authorized at all (anonymous).
* Optionally, each service operation may cache its response.
* Optionally, each service operation may apply a rate limiting policy (eg. per user, per timeframe, etc).
* Each service operation corresponds to a method call to an **Application Interface** in the **Application Layer**. The call from service operation to application interface, converts all request DTO data into shared DTOs, and extracts the user's identifier from the token, passes through all of that to the application interface. On the way back, exceptions are converted to appropriate HTTP status codes and descriptions, and all data is converted to response DTOs. The only data in and out of the application interface are DTOs that are declared by the Application Layer.
* The Application Layer is defined by bounded contexts. One Application Interface per bounded context.
* The Application interface retrieves the Root Aggregate (for this bounded context) from persistence, and operates on that aggregate (for Commands. The Application interface retrieves data directly from persistence via Read Models (for Queries). The code for each of these kinds of operations has some *loose* patterns, but none of those patterns can be predicted well-enough, and must be hand crafted by a contributor.
* The Application Layer also contains adapters to persistence stores via **Repositories**, **Read Models** and contains other **Service Client** adapters to external HTTP services that are necessarily hand crafted.
* The **Domain Layer** necessarily contains hand crafted **Aggregates**, **Value Objects**, **Domain Services** etc.

> Note: all the terms in bold above, are terms that all contributors on API codebases at `Acme` understand in their mental model of building an API. These terms are often encoded into the names and into the structure of codebases built at Acme. They are part of the language of building API's at `Acme`, and used in all design communication.

### Step 1 - Capture a basic pattern

The tech lead now intends to provide some custom tooling to their team to help build out new API's at `Acme`, using the logical programming model described earlier.

On the command line, navigate to the source code directory of the new product called `banana`. eg. `cd C:/projects/acme/banana/src`

`automate new pattern AcmeAPI`

> This command registers a new pattern called `AcmeAPI` in the current directory. It saves a bunch of files defining the pattern, which could be added to source control.

Now, the tech lead will want to extract a coding pattern that exists in the existing codebases. For example, the last one, which is rooted at: `C:/projects/acme/apple/src/`. These patterns may exist in one file, or they may exist across several files in multiple directories. We assume here that the pattern is spread across multiple files, in different directories.

The tech lead identifies all these files that each contain a fragment of the pattern. At this stage, the tech lead has an idea of the complete pattern in their head, and uses their editing tools to view it. All they need to know at this point, is which files it exists in.

Lets assume for this example, that the fragments of this pattern exist in all these files of a current codebase:

```
- src
	- backend
		- controllers
			- BookingController.cs
		- services
			- BookingService.cs
			- IBookingService.cs
```

For each file that contains a fragment of the pattern, they execute this command:

`automate add template <file-path>`

> This command registers a CodeTemplate on the `AcmeAPI` pattern, called `Template1`, which contains all the code from the file at `<file-path>`.

For this codebase, this will be 3 commands:

* `automate add template backend/controllers/BookingController.cs`

* `automate add template backend/services/BookingService.cs`

* `automate add template backend/controllers/IBookingService.cs`

> This means now that conceptually, the pattern called `AcmeAPI` has three templates called `Template1` `Template2` and `Template3`.

So far, we don't have much, just a new pattern called `AcmeAPI`, and that has 3 templates. Conceptually, it looks like this.

```
- AcmeAPI (pattern element, attached with 3 templates)
```

### Step 2 - Define some attributes of the pattern

Now that the tech lead has a *pattern* to conceptually represent building a new API (for new products at `Acme`), the tech lead now needs to figure out how to allow a contributor on their team customize this specific API to be useful in a new product called `banana`. Obviously, a developer (on the `banana` team) is going to need to define their own API's for the `banana` product, and they are not going to want to copy and paste the `Booking` API, that was extracted from the existing codebase, and now lives in the 3 code templates.

The tech lead now has to identify, **what will be different** between the `Booking` API of the existing codebase, and any new API being built in the next codebase.

> Clearly, a bunch of things will need to be different. (1) The names of the files (and possibly directories) produced from this pattern will need to be different, and (2) so will the actual code in those files. Including the names of code constructs like classes and methods in those files.

The tech lead decides that the developer will have to at least name the new API, because that name influences the file names of the logical `Controller`, `Service Interface` and `Service Implementation` which are all classes that need to be created to contain the code specific to this new API.

So the tech lead defines an attribute on the pattern called `Name`

`automate add attribute "Name" isrequired`

Now, the tech lead knows that every new API contains multiple Service Operations (according to existing coding patterns).

They now need to add a collection to the pattern to allow their contributors to add multiple Service Operations.

`automate add collection "Operations" describedas "The service operations of the API" `

> Notice here that the tech lead decided to name this collection `Operations` and give it a meaningful description, rather than name it `Service Operations`. This is for brevity.

Now, the tech lead needs to add a new Element to the collection representing each `Service Operation`, and on this element, define some attributes that describe each operation (as per pattern defined above).

`automate add element "ServiceOperation" aschildof {AcmeAPI.Operations}`

And then the necessary attributes:

`automate add attribute "Name" isrequired aschildof {AcmeAPI.Operations.ServiceOperation}`

`automate add attribute "Verb" isrequired isoneof "POST;PUT;GET;PATCH;DELETE" aschildof {AcmeAPI.Operations.ServiceOperation}`

`automate add attribute "Route" isrequired aschildof {AcmeAPI.Operations.ServiceOperation}`

`automate add attribute "IsAuthorized" isrequired typeis "bool" defaultvalueis "true" aschildof {AcmeAPI.Operations.ServiceOperation}`

> Note: the optional properties of a `Service Operation`, such as response caching and rate limiting are deliberately omitted here for simplicity. Which is an example of the tech lead picking and choosing what to start their team with in the first iterations of the pattern, leaving room for evolving as the `banana` product matures.

A `ServiceOperation` is required to have a Request DTO and a response DTO. These DTO's for now will just be dictionaries (for simplicity). So the tech lead would define both of those on the `Service Operation` as a collection of `Field` elements with various useful attributes.

First the request DTO:

`automate add collection "Request" aschildof {AcmeAPI.Operations.ServiceOperation}`

`automate add element "Field" aschildof {AcmeAPI.Operations.ServiceOperation.Request}`

`automate add attribute "Name" isrequired aschildof {AcmeAPI.Operations.ServiceOperation.Request.Field}`

`automate add attribute "Type" isrequired isoneof "string;int;bool;DateTime" defaultvalueis "string" aschildof {AcmeAPI.Operations.ServiceOperation.Request.Field}`

`automate add attribute "IsOptional" isrequired typeis "bool" defaultvalueis "false" aschildof {AcmeAPI.Operations.ServiceOperation.Request.Field}`

and similarly, for the Response DTO:

`automate add collection "Response" aschildof {AcmeAPI.Operations.ServiceOperation}`

`automate add element "Field" aschildof {AcmeAPI.Operations.ServiceOperation.Response}`

`automate add attribute "Name" isrequired aschildof {AcmeAPI.Operations.ServiceOperation.Response.Field}`

`automate add attribute "Type" isrequired isoneof "string;int;bool;DateTime" defaultvalueis "string" aschildof {AcmeAPI.Operations.ServiceOperation.Response.Field}`

So far, we are starting to build up our conceptual model. It now looks like this:

```
- AcmeAPI (pattern element, attached with 3 templates)
	- (attributes):
		- Name (string, required)
	- Operations (collection)
		- ServiceOperation (element)
			- (attributes):
				- Name (string, required)
				- Verb (required, oneof: "POST;PUT;GET;PATCH;DELETE")
				- Route (string, required)
				- IsAuthorized (boolean, default: true)
			- Request (collection)
				- Field (element)
					- (attributes):
						- Name (string, required)
						- Type (required, oneof "string;int;bool;DateTime")
						- IsOptional (boolean, default: false)
			- Response (collection)
				- Field (element)
					- (attributes):
						- Name (string, required)
						- Type (required, oneof "string;int;bool;DateTime")
```

So, with this conceptual meta-model, a contributor on the `banana` product can now define any API in the `banana` product in terms of its `ServiceOperations` and its `Request` and `Response` DTO's.

The code that needs to be written into the codebase (C# classes, enums, interfaces, etc) that are needed to contain the Controller, Service Interface, and Service Implementation can now be derived from this meta-model, and the file names, and directory structure for those classes can be derived from this meta-model too, with naming and structural conventions the tech lead can define.

### Step 3 - Templatize the Code

The next challenge is to modify the code templates of existing code to read the meta-model above, and produce the appropriate code in the right places in the codebase.

For this, the tech lead will need to update the code templates that they have already captured (on the pattern element), which will now need to navigate the meta-model and combine the data from the meta-model with established coding patterns into generated code.

> Note: Each file that is generated from each of the code templates will eventually need to be named, and placed into the `banana` codebase in the appropriate directory, with the appropriate name. That process will happen in the next step.

The tech lead, now needs to fire up their favourite text editor and modify the 3 code templates they harvested already.

The code template files have been renamed with the extension `.tt` and can be found in the following location:

`C:/projects/acme/banana/src/automate/codetemplates`

There are 3 templates, each with a unique name that was assigned to it when the `automate add template` command was run.

Either look at the CLI output to find out the name of the template that was created. Or you can run this command to list them:

`automate list codetemplates`

Open each of the `*.tt` files in a text editor.

Make the following changes:

#### Template1 - Controller

```
<#@ Pattern processor="PatternModelDirectiveProcessor" requires="fileName='automate\Model\Pattern.metamodel'"#>
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Acme.<#=Pattern.Name #>.Controllers
{
// Generate the ASP.NET Controller class
	[ApiController]
	public partial class <#=this.AcmeAPI.Name #>Controller : Controller
	{
<#
foreach (var operation in this.AcmeAPI.Operations.Items)
{
#>
<#
if (operation.IsAuthorized)
{
#>
			[Authorize]
<#
}
else
{
#>
			[AllowAnonymous]
<#
}
#>
			[Http<#=operation.Verb #>]
			[Route( "<#=operation.Route #>" )]
			public IActionResult <#=operation.Name #>(<#=operation.Name#>Request request)
			{
				var response = this.application.<#=operation.Name #>(request.ToDto());
				return OK(response.ToResponse());
			}
<#
}
#>
	}
    
// Generate the Request class
<#
foreach (var operation in this.AcmeAPI.Operations.Items)
{
#>
	public class <#=operation.Name #>Request
	{
<#
foreach (var field in operation.Request.Items)
{
#>
		public <#=field.Type #><#=field.IsOptional ? "?" : "" #> <#=field.Name #> { get; set; }
<#
}
#>
	}

// Generate the Response class
	public class <#=operation.Name #>Response
	{
<#
foreach (var field in operation.Response.Items)
{
#>
		public <#=field.Type #> <#=field.Name #> { get; set; }
<#
}
#>
	}
<#
}
#>

// Generate converters (using AutomMapper)
	internal static class <#=this.AcmeAPI.Name #>ConversionExtensions
	{
<#
foreach (var operation in this.AcmeAPI.Operations.Items)
{
#>
			public static <#=operation.Name #>Dto ToDto(this <#=operation.Name#>Request request)
			{
				var dto = request.ConvertTo<<#=operation.Name #>Dto>();
				return dto;
			}

			public static <#=operation.Name #>Response ToResponse(this <#=operation.Name#>Dto dto)
			{
				var response = dto.ConvertTo<<#=operation.Name #>Response>();
				return response;
			}
<#
}
#>
	}
}
```

#### Template2 - Service Implementation

```
<#@ Pattern processor="PatternModelDirectiveProcessor" requires="fileName='automate\Model\Pattern.metamodel'"#>
using System;

namespace Acme.<#=Pattern.Name #>.Services
{
	public class <#=this.AcmeAPI.Name #>Service : I<#=this.AcmeAPI.Name #>Service
	{
<#
foreach (var operation in this.AcmeAPI.Operations.Items)
{
#>
		public <#=operation.Name#>Dto <#=operation.Name #>(<#=operation.Name#>Request dto)
		{
			// Put your custom here and populate the return object
			return null;
		}
<#
}
#>
	}
}
```

#### Template 3 - Service Interface

```
<#@ Pattern processor="PatternModelDirectiveProcessor" requires="fileName='automate\Model\Pattern.metamodel'"#>
using System;

namespace Acme.<#=Pattern.Name #>.Services
{
	public interface <#=this.AcmeAPI.Name #>Service
	{
<#
foreach (var operation in this.AcmeAPI.Operations.Items)
{
#>
		<#=operation.Name#>Dto <#=operation.Name #>(<#=operation.Name#>Request dto);
<#
}
#>
	}
}
```

> Note: The actual code generated from these templates is just an example of how to write coding patterns using a text template language. Microsoft T4 text templating is the example here.

### Step 4 - Generate the Code

Now that the tech lead has the three code templates modified, the last step is to define *when* the code templates are rendered, and *where* the generated code is placed in the codebase of the new `banana` product.

The tech lead decides that they will use the same directory structure and naming convention as used in the previous product `apple`.

However, the tech lead also knows that the `Service Implementation` class is likely to be written by hand by one of the contributors on the team. Whereas, the `Controller` class and the `Service Interface` can likely be generated.

Therefore the `Service Implementation` class will only be generated if the file does not already exist, and once it is generated never generate again. Whereas the `Service Interface` and `Controller` files will always be generated and kept up to date as things change.

The next thing to figure out, is how to generate the files.

This can be done whenever new `Service Operations` are added (or changed) as an *Event* or perhaps when the codebase is compiled, or it can be done at any time by executing a command explicitly (or in fact all of those options). In either case, a *Launch Point* needs to be defined and added to the pattern to do this code rendering. These commands will decide **where** to render the files.

`automate add codetemplatecommand "Template1" aschildof {AcmeAPI} withpath "~/backend/Controllers/{{Name}}Controller.gen.cs"`

`automate add codetemplatecommand "Template2" astearoff aschildof {AcmeAPI} withpath "~/backend/Services/{{Name}}Service.cs"`

`automate add codetemplatecommand "Template3" aschildof {AcmeAPI} withpath "~/backend/Services/I{{Name}}Service.gen.cs"`

> These commands adds new commands for each template to the root pattern element (AcmeAPI). Each of these commands returns the Command ID (CMDID) of the command, which we will need in the next step.
>
> Notice that for "Template2" we use the keyword `astearoff`  (and a slight variation on the file extension in the `withpath` attribute) to indicate that this file will only be generated once, and only if, the specified file does not exist at the specified location with the specified name.
>
> The `withpath` attribute starts with a tilde `~` indicating that we want the files to be rooted at the root directory of the pattern.

Now, that we have three explicit commands to execute, we define a single launch point that will be able to execute them all **when** we want (using the values of `<CMDID>` that were returned from the previous commands):

`automate add commandlaunchpoint "<CMDID1>;<CMDID2>;<CMDID3>" as "Generate" aschildof {AcmeAPI}`

> This command adds a launch Point called "Generate" that can now be executed on the `AcmeAPI` element.

### Step 5 - Build the Toolkit, and Ship It

Now the tech lead has a functioning pattern it time to ship it to their team.

`automate build "AcmeAPI"`

> This command creates a standalone (cross-platform) application that will automatically be versioned and can now be installed by contributors at Acme.

### Step 6 - Apply the Pattern

Navigate to the new `banana` codebase: `cd C:/projects/acme/banana/src`

Install the downloaded toolkit:

`automate install "C:/Downloads/AcmeAPI.toolkit"`

> This command installs the `AcmeAPI.toolkit` into the current directory, which in this case is `C:/projects/acme/banana/src/automate/toolkits/AcmeAPI/v1.0.0.0`

Now, a project contributor can define a new API with this toolkit.

#### The New API

For this example, lets call the new API the `Orders` API.

This API has one authorized POST operation called `CreateOrder` at `/orders`.

This HTTP request takes a `ProductId` as the only request parameter, and returns a completed Order with an `Id` property in the response.

To get started:

`automate with "AcmeAPI" add "AcmeAPI" with "Name=Orders"`

> This command creates a new `AcmeAPI`, and returns its unique ID.

`automate with "AcmeAPI" add "ServiceOperation" to "<PATTERNID>.Operations" with "Name=CreateOrder" with "Verb=Post" with "Route=/orders" with "IsAuthorized=true"`

> This command creates a new `ServiceOperation` to the "Operations" collection, and returns its unique ID.
>
> Notice we `add` `to` an expression that navigates the graph of object instances here, starting from the referenced ID, which is a convenience.

`automate with "AcmeAPI" add "Field" to "<OPERATIONID>.Request" with "Name=ProductId" with "Type=string" with "IsOptional=false"`

> This command creates a new field in the Request DTO called `ProductId`

`automate with "AcmeAPI" add "Field" to "<OPERATIONID>.Response" with "Name=Id" with "Type=string"`

> This command creates a new field in the Response DTO called `Id`

Now that the new Acme API called `Orders` is completely configured, a codebase contributor can now get the toolkit to write the code for them!

`automate with "AcmeAPI" execute "Generate"`

> This command runs the `Generate` launch point (on the root pattern element), which runs the configured commands, that generates the code files from the three code templates. The code is written into the codebase at the relevant locations.

> If any of the required properties are not set, an error will be displayed. If all goes well, a list of commands will be displayed.

The `banana` codebase should now look like this:

```
- projects
	- acme
		- banana
			- src
				- automate
					- (various directories and files)
				- backend
					- Controllers
						- OrdersController.gen.cs
					- Services
						- IOrdersService.gen.cs
						- OrdersService.cs
```

The codebase contributor is now able to change the new Orders API in whatever way they like, or they can add more APIs.

The codebase contributor will need to manually complete write the missing code in the `ordersService.cs` class, as this toolkit (as it is now) is not able to take it further.

The tech lead is free to modify or extend their pattern, add/remove/change anything they like, and then ship another version of the toolkit to their team. Where the toolkit will be updated.
