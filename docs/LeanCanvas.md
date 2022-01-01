# Lean Canvas - automate

## Problem
> List your top 1-5 problems.

1. A software team usually starts from first principles with only rudimentary sample coding templates, and they build a codebase from scratch for every piece of software they create. Coding patterns and architectures specific to this codebase may emerge and may persist across software archetypes. If they do, they will be honed (over time) with each reuse (by the same people). However, they are still quite difficult to tease out of any specific codebase to be reused on the next codebase. (opportunity for sharing knowledge, without violating IP constraints).
2. Any developer finds it very hard to provide or prescribe known/pre-defined coding patterns to less experienced team members in a form that is easy to re-apply (where and when they need them), in order to maintain consistency across a codebase. (opportunity to capture important patterns in a canonical form for sharing).
3. Less experienced developers (in a specific codebase) find it hard to know where to put their point-solutions in a codebase, as they are more focused on getting the task done rather then how, or where and how it should fit in an architecture. (opportunity for contextualising the patterns)
4. Documenting accurately (over time, as things change), and then carbon copying coding patterns (verbatim) is very tedious and error prone for developers to re-use them correctly. (opportunity for automating rote tasks, and versioning auto-migration of patterns).
5. Enforcing the correct application of [agreed to] coding patterns is very challenging for teams since the development tools are too general-purpose to be specific enough to understand specific patterns. The DSL argument. Development tools (IDE's specifically) are very good today at making it easy and seamless to manipulate code structure and coding patterns in any way the developer wishes. The tools that most IDE's provide to enforce coding patterns (eg. Rosyln Analysers) are still too difficult to program, configure, maintain, version and deploy to a specific team (for team members to maintain effectively alongside their code as it changes).

### Existing Alternatives
> List how these problems are solved today.

1. No re-use, no patterns - Building software without  pre-defined architectures, and with no patterns of reuse (learned from previous codebase experiences). Often lacking technical leadership, developers are starting from first principles each time, and falling into common traps that are mitigated by having an architecture and documented constraints -> evolving quickly into big balls of mud.
2. Tech Leads (lead developers) are having to work very hard to harvest and lay down patterns manually and consistently, and then pair with every team member to demonstrate and teach them how to reproduce those patterns and how to extend them in legitimate ways. There are some rudimentary templating tools in most IDE's to help with applying simple templated files, but none are integrated well enough to apply patterns that span across multiple projects/folders, nor that use information harvested from their existing code. None (explored) manage the versioning of the patterns and migrating from one version to another, nor refactoring code generated from previous versions of the pattern, as they change. 
3. To enforce that patterns are used correctly, Tech Leads are having to write automated tests to ensure that these patterns are adhered to correctly, and architectural constraints are not accidentally violated, because the development tools are so general purpose and must allow the developer full freedom to do whatever they want, even violating the architecture of the codebase.
4. Code completion tools (with Machine Learning assistance) such as eg. Github CoPilot and TabNine are second guessing exact code matches (as developers type code), but these tools are not yet learning the variability of the larger patterns in the code. At best, they are only learning what has been typed before in that codebase.

## Customer Segments
> List your target customer and users.

1. Tech Leads/Lead Developers of software teams that want to provide their team with specific coding patterns in their software. Especially software teams that create similar products or solutions of specific types (or domains).
1. Any developer that wants to re-use existing patterns (reuse knowledge of experts).

### Early Adopters
> List the characteristics of your ideal customer.

They are an experienced software company/team that sees value in establishing and maintaining consistency across a codebase, that must evolve.

They have several wishes to accelerate their creation of quality software:

1. To accelerate the onboarding of less experienced developers onto the codebase (and teach them existing coding patterns and the shape of new code and where it fits).
2. To re-use learned patterns between software codebases that are similar in archetype (eg. web API patterns, IoT device patterns).
3. Are willing to invest in developer tooling that captures and evolving these patterns over the years (as tech platforms change).


## Unique Value Proposition
> Single, clear, compelling message that states why you are different and worth paying attention.

Today, more than half of the developers (building software products/services/solutions) have less than 5 years experience creating maintainable, robust and supportable software systems that can scale to larger teams and larger markets. Many of those developers are re-learning over and over again every year the things that were already learned by the previous generation of developers before them about how to manage complexity of their software as it necessarily has to scale. The same fundamental mistakes are being made over and over again, and this makes the process of getting to market and scaling up the team and scaling up the software extremely expensive, at just the wrong inconvenient time for the software business itself. This is a major contributing factor to start-up failure. The inability to scale the software team, and still iterate and experiment fast enough in the codebase, to address expanding market opportunities.

We recognise that these problems are ultimately worked on and resolved by every developer at some point as they become more experienced, and many of those developers go on to learn how to tackle and account for these challenges in the next product/service/solution. However, reusing what they have learned on the next product/service/solution (as architecture and coding patterns) is a challenge both in terms of tooling, in terms of intellectual property, and in terms of sharing with their next software team. Much of what has to be reapplied to the next software venture, is not specific IP to the last product/service/solution, it is really just "best practice" guidelines and "guide rails" that ensure that complexity of the software is managed correctly over time. But this kind of knowledge/wisdom is very hard to tease out, harvest and re-apply in a generic enough from to benefit the next software product/service/solution, especially if it has to account for advances in technology platforms as they gradually evolve in the same timescale as these experiences. 

Automate understands these complex constraints and has been optimized to make these managing these constraints easy for developers as things necessarily change for them. 

Automate has an opportunity to inform and improve existing Code-completion technologies (eg. Github CoPilot and TabNine).

### High-level Concept
> List your X for Y analogy e.g. Youtube = Flickr for videos

Automate = Y for coding patterns


## Solution
> Outline a possible solution for each problem.

Build a set of (open source) tooling that allows Tech Leads/Lead Devs to capture their coding patterns, and provide tools that other developers can use to apply those patterns in their own codebases.  

1. **Harvesting/Capturing Coding Patterns in a model** - Ability to create a high level (and composable) meta-model of components in the architecture of the software that describes parts of the codebase. Notionally, lasso files/classes/functions are a specific component. Then draw the arrangement and relationships of the components to one another. For example: describing the Layers of the architecture and the components within them, and what their concerns are (data to describes how to configure them). Be able to define a logical model for each component, that defines it variability and how it manifests as code and configuration in files. Be able to mark up version of the components in that model.
2. **Distribute tools that generate files from the model** - Ability to create plugins (for an IDE), that display and compose and manage versions of the models, and that can be used by a developer to create new components in their codebase. These tools then track what was created and where in the codebase it was created, also tracking where things are moved and renamed, as they are refactored (modified or deleted).
3. **Evolving, Versioning and Migration** - code changes and patterns evolve. Provide tools to identify changes in the generated existing patterns and notify the developer to submitting a change request to the author of the pattern.  
4. **Validation and Checking of existing code** - code changes and violates the patterns. Provide tools to identify changes in the generated existing patterns and notify the developer that they are violating the pattern, and should conform.

Provide a central online service to store these coding patterns, for a company team, or for an individual (i.e. tech consultant).

Provide integrations with popular IDE's to integrate these tools.

Provide simple standalone tools to provide similar rudimentary command line capabilities.


## Channels
> List your path to customers (inbound or outbound).

1. Tech Consultants who move between organisations, who see value in reusing what they learn and refine between engagements
2. Tech Leads/Senior Developers who move between employers, who see value in reusing what they already learned in previous employers.
3. Youtube influencers/channels/teachers who want to promote their architectures/coding solutions to their communities.
4. Developer evangelists at IDE vendors who want to distinguish the capabilities of their IDEs. 

## Revenue Streams
> List your sources of revenue.

1. Software license fees and/or subscriptions

## Cost Structure
> List your fixed and variable costs.

1. Cost to develop the technology
2. Cost to (digital) market the technology
3. Cost to (physical) market the technology at physical conferences/trade shows.  

## Key Metrics

> List the key numbers that tell you how your business is doing.

1. MRR
1. #patterns created (by unique licensees)
2. #patterns re-used
3. #pattern versions created

## Unfair Advantage
> Something that cannot easily be bought or copied.

The author created a sponsored developer program and a product (at  Microsoft corp, circa 2005-2011) that created the first generation of this kind of tooling in Visual Studio, deploying it to a large software partner (Raytheon), and then going on to open source it. Albeit underfunded, and unsupported by Microsoft and its eco-system. 