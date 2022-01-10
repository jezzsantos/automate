# Lean Canvas - autōmate

## Business Problem

> What problem does the business have that you are trying to solve? (Hint: Consider your current offerings and how they deliver value, changes in the market, delivery channels, competitive threats and customer behavior.)

1. Most software teams start new projects/products from first principles with only rudimentary and sample starter code templates. Then they build up a basic codebase structure from scratch for every piece of software they create. Code implementation patterns and code structure patterns specific to this codebase may emerge. Depending on the type of pattern, they may persist across software archetypes (eg. Web API, Website, Mobile, EDA, etc) but only if the developers build more than one instance of that archetype. If they do, they will likely extract coding patterns from successive instances and hone those patterns (over time). However, structural patterns and implementation patterns are still quite difficult to tease out of any specific codebase to be reused on the next codebase, unless the developer has extensive experience across many similar projects (unlikely for products as they have longer lifecycles). (opportunity here for: sharing knowledge, without violating IP constraints).
2. Tech Leads/Lead Developers/Architects/Tech Consultants find it very difficult to provide or prescribe known/pre-defined coding patterns to less experienced team members in a form that is easy to re-apply (where and when they need them), in order to maintain consistency across a codebase. Generally, to do that they have to write extensive guidance and documentation, and ensure that it is followed, and keep it up to date with changes. (opportunity here to: capture important patterns in a canonical form for easily sharing).
3. Less experienced developers (in a specific codebase) find it hard to know where to put their point-solutions in a codebase, as they are more focused on getting the task done rather then how, or where and how it should fit in an architecture. (opportunity here for: contextualising the patterns, and the patterns knowing where to put code)
4. Applying coding patterns (verbatim) into a codebase is very tedious and error prone for codebase contributors to re-use them correctly. (opportunity here for: automating rote tasks, and versioning auto-migration of patterns).
5. Enforcing the correct application of coding patterns is very challenging for coding teams since the development tools in common use today are too general-purpose to be specific enough to understand the constraints of specific coding patterns in a specific codebase. The DSL argument. Development tools (IDE's specifically) are very good today at making it trivially easy and seamless to manipulate code structure and code/configuration in any way the developer wishes (highly flexible) and this easily compromises any established structural coding patterns. Forcing Tech Leads to use other tools (like unit tests, architectural validation tools) to ensure that coding patterns are not compromised.
5. The tools and technologies that most IDE's provide to enforce coding patterns (eg. Rosyln Analysers, Code-Completion technologies) are still far too difficult to program, configure, maintain, version and deploy to a specific team (for team members to maintain effectively alongside their code as it changes).

### Existing Alternatives

> List how these problems are solved today.

1. **Worked around**: No re-use, no coding patterns (aka the Wild West) - Building software without pre-defined architectures, structural patterns, separation of concerns, etc, and with no patterns of reuse (that were learned from previous codebase experiences). Often lacking technical leadership in these codebases, each codebase contributor is starting from first principles each time, doing as they please, limited by their own experience, often supplemented with plethora of their familiar and favourite 3rd party OSS libraries. Creating massive amounts of technical debt, cruft, and accidental complexity. Evolving quickly into big-balls-of-mud.
2. **Manual Application and Enforcement**: Tech Leads/Lead Developers/Architects/Tech Consultants are having to work very hard to manually harvest, tailor, and prepare suitable patterns manually and consistently across a codebase. Then meet/pair with every team member to demonstrate and teach them how to reproduce those patterns and how to extend them in legitimate ways. Code reviews and frequent manual inspections are employed to prevent codebase contributors from working around those patterns, and the necessary constraints they provide to the codebase.
2. **Automated Application**: There are some rudimentary templating tools in most IDE's to help with applying simple templated files (eg Live Templates in Jetbrains IDE, Visual Studio Snippets) , most are either at the snippet level, or at the file level, but none are integrated well enough to apply patterns that span across multiple projects/folders/files, nor that extract information harvested from the existing code. None (explored) manage the versioning of the patterns and migrating from one version to another, nor refactoring code generated from previous versions of the pattern, as the patterns change.
4. **Automated Enforcement** solutions: To enforce that patterns are used, and used correctly, Tech Leads/Lead Developers/Architects/Tech Consultants are having to resort to:
    1. Write Automated unit Tests or create Roslyn Analysers plugins to ensure that these patterns are adhered to correctly, and architectural/structural/semantic constraints are not incidentally nor accidentally violated when code is changed.
    2. Code completion tools (with Machine Learning assistance) such as eg. Github CoPilot and TabNine are second guessing and suggesting exact code matches (as developers type their code), but these tools are not yet learning the variability of the larger component patterns in the code. At best, they are only learning what has been typed before in that codebase. (more investigation into their capabilities is needed as these technologies emerge).
    3. The NuPattern project (https://github.com/nupattern) open source, already provides the first version of this capability, and has addressed many of the challenges of this kind of tooling, and how to integrate it into an IDE. However, it is only targeted at .NET stacks, works in Visual Studio 2013, and has not been updated to work with any other IDE, and is no longer maintained.

## Customer Segments

> List your target customer and users.

1. Producers of developer tooling: Tech Leads/Lead Developers/Architects/Tech Consultants working with software teams, that desire to provide their software teams (Code Contributors) with specific coding patterns in their software. Especially software teams that create similar products or solutions of specific types or technical domains (eg. WebAPIs, Mobile Apps, IoT Sensing Devices, etc).
1. Consumers of tooling: Code contributors that want to re-use existing patterns (re-use the knowledge of their experts). Code contributors on a team where these tools are mandated by Tech Leads/Lead Developers/Architects/Tech Consultants.
1. Technology Evangelists/Tech Domain Experts/Community Leads who want to share their best/recommended practices and guidance in an easy to apply format, for others in their communities.

### Early Adopters

> List the characteristics of your ideal customer.

They are an experienced software company/team that sees value in establishing and maintaining consistency across a codebase, that must evolve and evolve over a reasonable period of time.

They have several wishes to accelerate their creation of quality software:

1. To accelerate the on-boarding of less experienced contributors onto the codebase (and teach them existing coding patterns and the shape of new code and how things are shaped and where they fit).
2. To re-use learned patterns between software codebases that are similar in archetype (eg. WebAPIs, Websites, Mobile Apps, IoT devices, etc).
3. Are willing to invest in developer tooling that captures and evolving these patterns over the years (as tech platforms change, and as the experts learn more).

## Unique Value Proposition

> Single, clear, compelling message that states why you are different and worth paying attention.

Today, more than half of the developers (building software products/services/solutions) have less than ~5 years experience creating maintainable, robust and supportable software systems that can scale to larger teams and larger markets. Many of those developers are re-learning over and over again every year the things that were already learned by the previous generation of developers before them about how to manage complexity of their software as it necessarily has to scale. The same fundamental mistakes that are accumulating cruft (technical debt) are being made over and over again, and this makes the process of getting to market and scaling up the team and scaling up the software extremely expensive. Often, at just the wrong inconvenient time for the software business itself. This is one of the contributing factors to tech start-up failures. The inability to scale the software team, and still iterate and experiment fast enough in the codebase, to address explore and expand new
market opportunities.

We recognise that these problems are ultimately worked on and resolved by most developers at some point in their careers, as they become more experienced, and many of those developers go on to learn how to tackle and account for these challenges in the next product/service/solution. However, reusing what they have learned on the next product/service/solution (as architecture and coding patterns) is a challenge both in terms of tooling, in terms of intellectual property, and in terms of sharing across their next software team. Much of what has to be reapplied to the next software venture, is not specific IP to the last product/service/solution, it is really just "best practice" guide lines and more prescriptive "guide rails" that ensure that complexity of the software is managed correctly over time. But this kind of knowledge/wisdom is very hard to tease out, harvest and re-apply in a generic enough from to benefit the next software product/service/solution, especially if it has to
account for advances in technology platforms as they gradually evolve in the same timescale as these experiences.

autōmate understands these complex constraints and has been optimized to make these managing these constraints easy for developers as things necessarily change for them.

autōmate has an opportunity to inform and improve existing Code-completion technologies (eg. Github CoPilot and TabNine).

### High-level Concept

> List your X for Y analogy e.g. Youtube = Flickr for videos

autōmate = Y for coding patterns (TBA)

## Solution

> Outline a possible solution for each problem.

Build a set of (paid for, open source) tooling that allows Tech Leads/Lead Developers/Architects/Tech Consultants to capture their personal/community/company coding patterns, and provide tooling that other code contributors can use to apply those patterns in their own codebases.

1. **Harvesting/Capturing Coding Patterns into an Abstract Model** - Ability to create a high level (and composable) meta-model of components of the software that describes parts of the codebase. Notionally, "lasso" coding patterns across files/classes/functions as a specific component of the overall codebase. That can then be replicated by configuring a set of custom attributes that this notional component has. Then, optionally, at a higher-level abstraction of the system, compose another model that arranges these components and their relationships. For example: describing what the Horizontal Layers and Vertical Slices of a specific codebase are, with components defined in each cell of that matrix. The larger model defining its concerns with data that describes how the individual components are related when assembled together. For each component, define a "logical" model for it that defines its attributes: what data it requires to be created/configured, and where to get that data
   from in the rest of the codebase. Then how the component and its component parts manifest themselves as code and configuration in projects/folders/files of the software solution. Be able to version each components in that model, and mange version compatibility and migration.
2. **Distribute tools that generate files from the model** - Ability to create versioned shippable toolkits/plugins (that install into a specific IDE), that provide a visual representation of the abstract model, and allow the consumer to compose together larger models with other plugins. This representation can then be used to invoke the models to directly affect their codebase (generate, manipulate code and configuration files). These tools also track what was created and where in the codebase it was created, also tracking where users manually moved/renamed/refactored/changed/deleted the files/folders that the toolkit has an vested interest in.
3. **Evolving, Versioning and Migration** - code changes over time and patterns evolve over time. Provide tools to identify changes in the generated existing patterns and notify the developer to submitting a change request to the author of the toolkit.
4. **Validation and Checking of existing code** - code changes and violates the patterns. Provide tools to identify changes in the generated existing patterns and notify the developer that they are violating the pattern, and should conform.

Provide a central online service to store these coding patterns, for an individual (i.e. company employee, or tech consultant). Where that person makes choices about the rights to the toolkits/patterns, and who has access to them [this will be a complex model since there are IP and ownership issues to mitigate].

Provide integrations with popular IDE's to integrate all these tools (i.e. Visual Studio, Jetbrains IDE, VS Code, etc) so that these tools are more easily integrated into the consumers everyday processes.

Provide (cross-platform) standalone command-line interface (CLI) to provide similar rudimentary capabilities. So that these tools can be used in any environment.

Provide a simple online editor, with most of the capabilities of the other tools, but clearly a limited set of functionality, so that Authors can make simple updates and changes to their patterns without requiring computers, IDE's etc.

## Channels

> List your path to customers (inbound or outbound).

1. Tech Consultants who move between organisations, who see value in re-using what they learn and refine between engagements
2. Tech Leads/Senior Developers who move between employers, who see value in reusing what they already learned in previous employers.
3. Youtube influencers/channels/teachers who want to promote their architectures/coding patterns to their communities.
4. Developer evangelists at IDE vendors who want to promote/distinguish the capabilities of their IDEs.

## Revenue Streams

> List your sources of revenue.

1. Software license fees and/or subscriptions

## Cost Structure

> List your fixed and variable costs.

1. Fixed monthly cost of a product team to develop the technology.
2. Fixed monthly cost of a (digital) marketing team to promote the technology.
2. Variable cost of online services/tools/hosting/etc to support the business.
3. Variable costs to (physically) market the technology at physical conferences/trade shows.

> All costs above, increasing over time with increased adoption

## Key Metrics

> List the key numbers that tell you how your business is doing.

1. MRR
1. # patterns created (by unique licensees)
2. # patterns re-used
3. # pattern versions created

## Unfair Advantage

> Something that cannot easily be bought or copied.

The authors have worked in this space across the globe for many years. The authors have created a sponsored developer program and a product (at Microsoft Corp, circa 2005-2012) that created the first generation of this kind of tooling in Visual Studio, deploying it to a large software partner (Raytheon), and then going on to open source it. Albeit underfunded, and unsupported by Microsoft and its eco-system. The authors have built and operated their own tech SaaS start-up previously, and mentors other tech start-up founders.
