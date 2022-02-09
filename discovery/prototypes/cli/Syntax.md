# CLI Syntax Example

This is a paper prototype of a CLI that could be used to deliver the core capabilities of autōmate.

## The Use Case

A software team is about to embark on building a new SaaS API product/service for their company `Acme`. 

The new product/service is called `RoadRunner`.

They have prior experience building such APIs, and have access to a number of codebases where they have done kind of work before.

> We assume for now that: those codebases are either existing in their company already (from prior APIs products/services) and that were built by this tech lead, or from open source projects the tech lead might be familiar with. Either way, the tech lead has access to these existing codebases, they are familiar with them, and they contain patterns that the tech lead wishes to use in the new product codebase.

The Tech Lead knows that the code in those existing codebases is largely what they want to replicate in this new `RoadRunner` API, and the tech lead desires NOT to have the wheel re-invented by other contributors on their team. At the same time, this new `RoadRunner` API will be based upon patterns that are very slightly different than the existing APIs for a couple of reasons:

1. This new `RoadRunner` API does NOT need some of the things/aspects/interfaces/abstractions that the existing API codebases have, because the `RoadRunner` team wants to get started faster, and not have to deal with those kinds of technical problems/constraints just yet. They will evolve. Some of which are assumed will be more important later on the development of the this API, but NOT now. This implies some extra work untangling some of the coding patterns of the existing codebases, which will lead to some extra experimentation by the tech lead.
2. This new API is going to improve on the old API patterns (in the existing codebases) to deal with some of the accumulated technical debt that existed in the previous codebases. Now, seems like a good time and opportunity to address them. This implies some extra experimentation and iteration to improve on those old patterns/capabilities by the tech lead.
3. Existing patterns in existing codebases are solid enough and well-proven in production already, and the tech lead wants to reuse them on this new `RoadRunner` API. Furthermore, the `RoadRunner` team is slightly different and contains contributors that are not so familiar with these patterns as the tech lead.

### Problem Statement

How can the tech lead *quickly and easily* capture the coding patterns they think should be re-used in the new codebase, and *make it easy* for the contributors in their new team to use them reliably and consistently. So that they all (tech lead and contributors) *can focus on* where the real learning needs to be in the new `RoadRunner` product/service?

>  We will use the word *Conceptual* to describe the idea in your mind.

>  We will use the word *Logical* to describe the realised model of the idea (in practice, with practical constraints considered). 

## The New Process

This is how that tech lead (on the `RoadRunner` team) could go about using the **autōmate** to get the job done.

> It is not important at this stage what programming language we are using to demonstrate this example. It could be in C# building a .Net5 ASP/NET app, or it could be JavaScript building a NodeJS app, or Java files building a Spring app. By necessity we need to give examples in code, so for now, we will be demonstrating this in C# ASP.NET MVC in .Net5. The CLI itself is technology agnostic, and can perform the same tasks no matter what programming language is ultimately used.

Assumptions about `Acme` APIs:

* An API at `Acme` is implemented with common conventions and patterns developed that have already been evolving at `Acme`. 
* The spoken/written language that describes how API's are designed/constructed, is common (enough) across the engineering teams building them.
* At `Acme` API products/services are usually built with patterns that loosely follow a ports and adapters architecture (Hexagonal/Onion/Clean architecture), uses CQRS-like patterns, and uses some domain driven design at its core.

When a new API product/service is built at `Acme`, it involves building these concepts/terms in code (in bold), and is logically put together like this:

* The code defining a web API interface is defined in the **Infrastructure Layer**.
* An API is segregated by a logical **Resource** (as in, a REST resource)
* Each Resource having one or more **Service Operations** (i.e. endpoints)
* Each service operation has a **Request DTO**, and a **Response DTO**. Each of these DTO's has one or more **Fields**. These fields make up a simple dictionary for requests, and can be more complex nested dictionaries for responses. All request/response DTO's are serializable and can be serialised to JSON on the wire (and other common formats: CSV, XML, etc).
* Each service operation has a **Route** (can define multiple).
* Each service operation may be **Authorized** with a token or not authorized at all (anonymous).
* Optionally, each service operation may cache its response.
* Optionally, each service operation may apply a rate limiting policy (eg. per user, per timeframe, etc).
* Each service operation corresponds to a method call to an **Application Interface** in the **Application Layer**. The call from service operation to application interface, converts all request/response DTO data into shared application level DTOs, and extracts the user's identifier from the Authorization token, and passes through all of that data to the application interface. On the way back, exceptions are converted to appropriate HTTP status codes (and descriptions), and all data is converted into response DTOs. The only data in and out of the application interface are DTOs that are declared by the Application Layer.
* The Application Layer is defined by **bounded contexts**. One Application Interface per bounded context. (these may or may not reflect directly the REST resources)
* The Application Interface retrieves a Root Aggregate (for this bounded context) from a persistence store, and operates on that aggregate (for Command operations). The Application Interface retrieves data directly from persistence via Read Models (for Query operations). The code for each of these kinds of operations has some *loose* patterns, but none of those patterns can be predicted well-enough, and must be hand crafted by a contributor on the team.
* The Infrastructure Layer also contains adapters to persistence stores via **Repositories**, **Read Models** and other **Service Client** adapters to external HTTP services that are also necessarily hand crafted.
* The **Domain Layer** necessarily contains hand crafted **Aggregates**, **Value Objects**, **Domain Services** etc.

> Note: all the terms in bold above, are terms that all contributors on API codebases at `Acme` understand in their mental model of building an API. These terms are often encoded into the names and into the structure of codebases built at Acme. They are part of the language of building API's at `Acme`, and used in all communicating the design of API products/services.

### Step 1 - Capture a basic pattern

The tech lead now intends to provide some custom tooling to their team to help build out new API's at `Acme`, using the logical programming model described earlier.

On the command line, navigate to the source code directory of the new product called `RoadRunner`. eg. `cd C:/projects/acme/roadrunner/src`

`automate create pattern "AcmeAPI"`

> This command registers a new pattern called `AcmeAPI` in the current directory. It saves a bunch of files defining the pattern, which could be added to source control in the `C:/projects/acme/roadrunner/src/automate` directory.

Now, the tech lead will want to extract coding patterns that exists in the existing codebases. For example, the last codebase they worked on, which is rooted at: `C:/projects/acme/coyote/src/`. 

These patterns may exist in one file, or they may exist across several files in multiple directories. We will assume here that the pattern is spread across multiple files, in different directories.

The tech lead identifies all the files (that each contain a fragment of the pattern). At this stage, the tech lead has an idea of the complete pattern in their head, and uses their editing tools to view it as a whole. 

All they need to know at this point, is which set of files the pattern exists within.

Lets assume for this example, that the fragments of this pattern exist in all these files of the `Coyote` codebase:

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

> This command registers a "Code Template" on the `AcmeAPI` pattern, which will be automatically name like `CodeTemplate1`, which contains all the code from the file at the specified relative `<file-path>`

For this codebase, this will be four similar commands:

* `automate edit add-codetemplate "backend/controllers/BookingController.cs"`
* `automate edit add-codetemplate "backend/services/BookingService.cs"`
* `automate edit add-codetemplate "backend/controllers/IBookingService.cs"`
* `automate edit add-codetemplate "backend/data/Bookings.cs"`

> This means now that conceptually, the pattern called `AcmeAPI` has a root element (called `AcmeAPI`) with four code templates called `CodeTemplate1` `CodeTemplate2`, `CodeTemplate3` and `CodeTemplate4`.

So far, we don't have much, just a new pattern called `AcmeAPI`, and four code templates. 

Conceptually, it would look like this, if we drew it as a logical structure.

```
- AcmeAPI (root element, attached with 4 code templates)
```

### Step 2 - Define some attributes of the pattern

Now that the tech lead has defined a new *pattern* to conceptually represent building a new API (for new products at `Acme`), the tech lead now needs to figure out how to allow a contributor on their team customize this specific API to be useful in a new product like the `RoadRunner` API their team will be building. 

Obviously, a contributor (on the `RoadRunner` team) is going to need to define their own API's for the `RoadRunner` product, and they are not going to want to copy and paste the `Booking` API that was extracted from the `Coyote` codebase, and now lives in the 3 code templates. These code templates are not yet reusable.

The tech lead now has to identify, **what will be different** between the `Booking` API of the `Coyote` codebase, and any new API being built in the next codebases.

> Clearly, a bunch of things will need to be different. 
>
> * (1) The names of the files (and possibly directories) produced from this pattern will need to be different, and 
> * (2) so will the actual code in those files. Including the names of code constructs like classes and methods in those files. 
> * But also (3) there are also a whole bunch of technical details that can very between API's.

The tech lead decides that the contributor will have to at least name the new API, (equivalent to `Bookings` from the `Coyote` API) because that name influences the file names of the logical `Controller`, `Service Interface` and `Service Implementation` files which are also classes that need to be created to contain the code specific to this new API. They also decide that they need a singular name for the resource of the API (e.g. `Booking` from the `Coyote` API) 

So the tech lead defines an attribute on the pattern called `Name`

`automate edit add-attribute "Name" --isrequired`

and then the `ResourceName` attribute:

`automate edit add-attribute "ResourceName" --isrequired`

Now, the tech lead knows that every new API contains multiple "Service Operations" (according to existing coding patterns).

They now need to add a collection to the pattern to allow their contributors to define multiple service operations.

`automate edit add-collection "ServiceOperation" --displayedas "Operations" --describedas "The service operations of the web API" `

> Notice here that the tech lead decided to name this collection with a name of `ServiceOperation` and give it a meaningful display name and description.

> The name of a collection or element cannot contain spaces, since it is an identifier.

And then the necessary attributes of a service operation:

`automate edit add-attribute "Name" --isrequired --aschildof {AcmeAPI.ServiceOperation}`

`automate edit add-attribute "Verb" --isrequired --isoneof "POST;PUT;GET;PATCH;DELETE" --aschildof {AcmeAPI.ServiceOperation}`

`automate edit add-attribute "Route" --isrequired --aschildof {AcmeAPI.ServiceOperation}`

`automate edit add-attribute "IsAuthorized" --isrequired --typeis "boolean" --defaultvalueis "true" --aschildof {AcmeAPI.ServiceOperation}`

> Note: The tech lead has deliberately ignored the optional properties of a service operation such as response caching and rate limiting for this next API. Which is an example of the tech lead picking and choosing what to start their team with in the first iterations of the pattern, leaving room for evolving as the `RoadRunner` product matures.

Now, a service operation (conceptually) has a Request DTO and a response DTO. These DTO's for now will just be dictionaries (for simplicity). So the tech lead would define both of those on the `ServiceOperation` as a "Collection" of `Field` elements with various attributes.

First the request DTO:

`automate edit add-element "Request" --describedas "The HTTP request" --aschildof {AcmeAPI.ServiceOperation}`

`automate edit add-collection "Field" --displayedas "Fields" --aschildof {AcmeAPI.ServiceOperation.Request}`

`automate edit add-attribute "Name" --isrequired --aschildof {AcmeAPI.ServiceOperation.Request.Field}`

`automate edit add-attribute "DataType" --isrequired --isoneof "string;int;bool;DateTime" --defaultvalueis "string" --aschildof {AcmeAPI.ServiceOperation.Request.Field}`

`automate edit add-attribute "IsOptional" --isrequired --typeis "bool" --defaultvalueis "false" --aschildof {AcmeAPI.ServiceOperation.Request.Field}`

and similarly, for the Response DTO:

`automate edit add-element "Response" --describedas "The HTTP response" --aschildof {AcmeAPI.ServiceOperation}`

`automate edit add-collection "Field" --displayedas "Fields" --aschildof {AcmeAPI.ServiceOperation.Response}`

`automate edit add-attribute "Name" --isrequired --aschildof {AcmeAPI.ServiceOperation.Response.Field}`

`automate edit add-attribute "DataType" --isrequired --isoneof "string;int;bool;DateTime" --defaultvalueis "string" --aschildof {AcmeAPI.ServiceOperation.Response.Field}`

So far, we are starting to build up our conceptual model. It now looks like this:

```
- AcmeAPI (root element) (attached with 4 code templates)
	- Name (attribute) (string, required)
	- ResourceName (attribute) (string, required)
	- ServiceOperation (collection)
		- Name (attribute) (string, required)
		- Verb (attribute) (required, oneof: "POST;PUT;GET;PATCH;DELETE")
		- Route (attribute) (string, required)
		- IsAuthorized (attribute) (boolean, default: true)
		- Request (element)
			- Field (collection)
				- Name (attribute) (string, required)
				- DataType (attribute) (required, oneof "string;int;bool;DateTime")
				- IsOptional (attribute) (boolean, default: false)
		- Response (element)
			- Field (collection)
				- Name (attribute) (string, required)
				- DataType (attribute) (required, oneof "string;int;bool;DateTime")
```

So, with this conceptual *meta-model* of an API, a contributor on the `RoadRunner` product can now define any API in the `RoadRunner` product in terms of its `ServiceOperations` and its `Request` and `Response` DTO's.

The code that needs to be written into the codebase (C# classes, enums, interfaces, etc) that are needed to contain the Controller, Service Interface, Service Implementation and DTOs can now be derived from this meta-model, and the file names, and directory structure for those classes can be derived from this meta-model too, with naming and structural conventions the tech lead can define.

### Step 3 - Templatize the Code

The next challenge is to modify the code templates of the existing code templates to read the meta-model above, and produce the appropriate code in the right places in the codebase.

For this, the tech lead will need to update the code templates that they have already captured (on the pattern element), which will now need to navigate the meta-model and combine the data from the meta-model with established coding patterns into generated code.

> Note: Each file that is generated from each of the code templates will eventually need to be named, and placed into the `RoadRunner` codebase in an appropriate directory, with an appropriate file name. That process will happen in the next step.

The tech lead, now needs to fire up their favourite text editor and modify all the code templates they harvested already.

The code template files have been renamed and can be found in the following location:

`C:/projects/acme/roadrunner/src/automate/codetemplates`

Each code template has a unique name that was assigned to it when the `automate edit add --codetemplate "<filepath>"` command was run.

Either look at the CLI output to find out the name of the template that was created. Or you can run this command to list them:

`automate edit list-codetemplates`

Open each of the template files in a text editor.

Make the following changes to them:

#### Template1 - Controller

```
using System;
using Microsoft.AspNetCore.Mvc;

namespace Acme.RoadRunner.Controllers
{
{{~#Generate the Controller class~}}
	[ApiController]
	public class {{model.name}}Controller : Controller
	{
{{~for operation in model.service_operation.items~}}
{{~if operation.is_authorized~}}
		[Authorize]
{{~else~}}
		[AllowAnonymous]
{{~end~}}
		[Http{{operation.verb}}]
		[Route( "{{operation.route}}" )]
		public IActionResult {{operation.name}}({{operation.name}}Request request)
		{
			var resource = this.application.{{operation.name}}(request.ToDto());
			return OK(resource.ToResponse());
		}
{{~end~}}
	}
    
{{~#Generate the Request & Response classes~}}
{{~for operation in model.service_operation.items~}}
	public class {{operation.name}}Request
	{
{{~for field in operation.request.field.items~}}
		public {{field.data_type}}{{if field.is_optional}}?{{end}} {{field.name}} { get; set; }
{{~end~}}
	}

	public class {{operation.name}}Response
	{
{{~for field in operation.response.field.items~}}
		public {{field.data_type}} {{field.name}} { get; set; }
{{~end~}}
	}
{{~end~}}

{{~#Generate the Application DTO classes~}}
{{~for operation in model.service_operation.items~}}
	public class {{operation.name}}
	{
{{~for field in operation.request.field.items~}}
		public {{field.data_type}}{{if field.is_optional}}?{{end}} {{field.name}} { get; set; }
{{~end~}}
	}

	public class {{model.resource_name}}
	{
{{~for field in operation.response.field.items~}}
		public {{field.data_type}} {{field.name}} { get; set; }
{{~end~}}
	}
{{~end~}}

{{~#Generate DTO converters (using AutoMapper)~}}
	internal static class {{model.name}}ConversionExtensions
	{
{{~for operation in model.service_operation.items~}}
			public static {{operation.name}} ToDto(this {{operation.name}}Request request)
			{
				return request.ConvertTo<{{operation.name}}>();
			}

			public static {{operation.name}}Response ToResponse(this {{model.resource_name}} dto)
			{
				return dto.ConvertTo<{{operation.name}}Response>();
			}
{{~end~}}
	}
}
```

#### Template2 - Service Implementation

```
using System;

namespace Acme.RoadRunner.Services
{
	public class {{model.name}}Service : I{{model.name}}Service
	{
{{for operation in model.service_operation.items}}
		public {{operation.name}}Dto {{operation.name}}({{operation.name}}Request dto)
		{
			// Put your custom here and populate the return object
			return null;
		}
{{end}}
	}
}
```

#### Template3 - Service Interface

```
using System;

namespace Acme.RoadRunner.Services
{
	public interface {{model.name}}Service
	{
{{for operation in model.service_operation.items}}
		{{operation.name}}Dto {{operation.name}}({{operation.name}}Request dto);
{{end}}
	}
}
```

#### Template4 - Application DTOs

```
using System;

namespace Acme.RoadRunner.DTOs
{
{{~#Generate the Application DTO classes~}}
{{~for operation in model.service_operation.items~}}
	public class {{operation.name}}
	{
{{~for field in operation.request.field.items~}}
		public {{field.data_type}}{{if field.is_optional}}?{{end}} {{field.name}} { get; set; }
{{~end~}}
	}

	public class {{model.resource_name}}
	{
{{~for field in operation.response.field.items~}}
		public {{field.data_type}} {{field.name}} { get; set; }
{{~end~}}
	}
{{~end~}}
}
```

> Note: The actual code generated from these templates is just an example of how to write coding patterns using a text template language. 
>
> Here, we are using a text templating technology called [scriban](https://github.com/scriban/scriban) which has its own templating language and syntax.

### Step 4 - Generate the Code

Now that the tech lead has the all the code templates modified, the last step is to define *when* the code templates are rendered, and *where* the generated code is placed in the codebase of the new `RoadRunner` product.

The tech lead decides that they will use the same directory structure and naming convention as used in the previous product `Coyote`.

However, the tech lead also knows that the `Service Implementation` class is likely to be written by hand by one of the contributors on the team. Whereas, the `Controller` class,  the `Service Interface` and the `DTO` classes can be generated in full.

Therefore the `Service Implementation` class will only be generated if the file does not already exist, and once it is generated never generate again. Whereas the `Service Interface`, `Controller` and `DTO` files will always be generated and kept up to date as the pattern changes.

The next thing to figure out, is how to generate the files.

This can be done whenever some event on the meta-model is raised. For example, when new `Service Operations` are added (or changed) or perhaps when the codebase is compiled, or it can be done at any time by executing a command explicitly (or in fact all of those options). In either case, a *Launch Point* needs to be defined and added to the pattern to execute the code template rendering. 

These commands will decide **where** to render the files, and what filenames to use.

`automate edit add-codetemplate-command "CodeTemplate1" --withpath "~/backend/Controllers/{{Name}}Controller.gen.cs"`

`automate edit add-codetemplate-command "CodeTemplate2" --astearoff --withpath "~/backend/Services/{{Name}}Service.cs"`

`automate edit add-codetemplate-command "CodeTemplate3" --withpath "~/backend/Services/I{{Name}}Service.gen.cs"`

`automate edit add-codetemplate-command "CodeTemplate4" --withpath "~/backend/Data/{{Name}}.gen.cs"`

> These commands adds new "Commands" for each template to the root pattern element (AcmeAPI). Each of these commands returns the Command ID (CMDID) of the command, which we will need in the next step.
>
> Notice that the filename for each uses an expression that includes the `Name` attribute of the pattern.
>
> Notice that for `CodeTemplate2` we use the option `--astearoff`  (and a slight variation on the file extension in the `--withpath` option) to indicate that this file will only be generated once, and only if the specified file does not exist at the specified location with the specified name.
>
> The `--withpath` option starts with a tilde `~` indicating that we want the files to be rooted at the root directory of the pattern. Which for this case is the current directory.

Now, that we have the four explicit commands to execute, we can define a single "Launch Point" that will be able to execute them all **when** we want (using the values of `<CMDID>` that were returned from the previous commands):

`automate edit add-command-launchpoint "<CMDID1>;<CMDID2>;<CMDID3>;<CMDID4>" --name "Generate"`

> This command adds a "Launch Point" called `Generate` that can now be executed on the `AcmeAPI` element.

### Step 5 - Build the Toolkit, and Ship It

Now the tech lead has a functioning pattern, it time to ship it to their team.

`automate build toolkit --version "auto"`

> This command creates a standalone (cross-platform) package (`AcmeAPI.toolkit`) that will automatically be versioned and can now be installed by any contributor at Acme.

### Step 6 - Apply the Pattern

Navigate to the new `RoadRunner` codebase: `cd C:/projects/acme/roadrunner/src`

Download and install the new toolkit:

`automate install toolkit "C:/Downloads/AcmeAPI.toolkit"`

> This command installs the `AcmeAPI.toolkit` into the current directory, which in this case is `C:/projects/acme/roadrunner/src/automate/toolkits/AcmeAPI/v1.0.0.0`

To list all the installed toolkits and versions of them:

`automate run list-toolkits`

> This commands lists all the installed toolkits in the current directory.

Now, a project contributor can define their own API with this newly installed toolkit.

#### Creating the New API

For this example, lets call the new API that we want help building, the: `Orders` API, in the `RoadRunner` product/service.

* The new API will have one authorized POST operation called `CreateOrder` at `/orders`.
* This HTTP request takes a `ProductId` as the only request parameter
* It returns a completed `Order` with an `Id` property in the response.

To get started:

`automate run toolkit "AcmeAPI"`

> This command creates a new "solution" from the `AcmeAPI` toolkit, and returns its unique SOLUTIONID.

To list all the solutions that have been created:

`automate run list-solutions`

> This commands lists all the created solutions in the current directory.

Now, lets program one of the solutions:

`automate using "<SOLUTIONID>" --set "Name=Orders" --with ResourceName=Order"`

> This command defines the `Name` and the `ResourceName` attributes of the pattern

`automate using "<SOLUTIONID>" --add "ServiceOperation" --to "Operations" --with "Name=CreateOrder" --with "Verb=Post" --with "Route=/orders" --with "IsAuthorized=true"`

> This command creates a new `ServiceOperation` instance to the "Operations" collection, and returns its unique OPERATIONID.
>

`automate using "<SOLUTIONID>" --add "Field" --to "<OPERATIONID>.Request" --with "Name=ProductId" --with "Type=string" --with "IsOptional=false"`

> This command creates a new field in the Request DTO called `ProductId`

`automate using "<SOLUTIONID>" --add "Field" --to "<OPERATIONID>.Response" --with "Name=Id" --with "Type=string"`

> This command creates a new field in the Response DTO called `Id`



After this set of commands, the pattern is fully configured.

Behind the scenes, the pattern meta-model has been populated with data that looks like this.

```
{
	"name": "Orders",
	"resource_name": "Order",
	"service_operation": {
		"display_name": "Operations",
		"description": "The service operations of the web API",
		"items": [{
				"name": "CreateOrder",
				"verb": "Post",
				"route": "/orders",
				"is_authorized": true,
				"request": {
					"description": "The HTTP request",
					"field": {
						"display_name": "Fields",
						"description": "",
						"items": [{
							"name": "ProductId",
							"data_type": "string",
							"is_optional": false
							}
						], 
					},
				},
				"response": {
					"description": "The HTTP response",
					"field": {
						"display_name": "Fields",
						"description": "",
						"items": [{
							"name": "ProductId",
							"data_type": "string",
							}
						] 
					}
				}
			}
		]
	}
}
```

> Notice, that the names of elements in the meta-model have been changed to snake-case.
>
> Notice, that the collections in the meta model have `items` containing the sub-elements.



A codebase contributor can now ask the toolkit to write the new code for them!

`automate using "<SOLUTIONID>" --execute-command "Generate"`

> This command runs the `Generate` Launch Point (on the root pattern element), which runs the configured commands, that generates the code files from all the code templates. The code is written into the codebase at the relevant locations.

> If any of the required properties are not set, an error will be displayed. If all goes well, a list of commands will be displayed.

The `RoadRunner` codebase should now look like this:

```
- projects
	- acme
		- RoadRunner
			- src
				- automate
					- (various directories and files)
				- backend
					- controllers
						- OrdersController.gen.cs
					- services
						- IOrdersService.gen.cs
						- OrdersService.cs
					- data
						- Orders.gen.cs
```

A codebase contributor will need to manually complete write the missing code in the `OrdersService.cs` class, as this toolkit (as it is now) is not able to take that code any further.

The codebase contributor could change the configuration of the Orders API in whatever way they like, and execute the command to update the code. Or they can add more service operations.

They can also use this toolkit to create another API, like `Customers` API.

The tech lead is free to modify or extend the pattern, add/remove/change anything they like, and then ship another version of the toolkit to their team. Where the toolkit will be updated, and code fixes can be applied.
