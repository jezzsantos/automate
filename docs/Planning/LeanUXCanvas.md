# Lean UX Canvas - autōmate

## Business Problem (1)

> What problem does the business have that you are trying to solve?
>
> (Hint: Consider your current offerings and how they deliver value, changes in the market, delivery channels, competitive threats and customer behaviour.)

[same as: Business Problem](LeanCanvas.md#Business Problem)

## Business Outcomes (2)

> How will you know you solved the business problem? What will you measure?
>
> (Hint: What will people/users be doing differently if your solutions work? Consider metrics that indicate customer success like average order value, time on site, and retention rate.)

They will be creating *toolkits* from which coding patterns are configured and then change codebases. We will measure the number of unique toolkits being created, and the number of times each is used to *touch* a codebase. The first metric demonstrating adoption of this practice, and the second metric indicating the stickiness of their toolkits, as a proxy for the stickiness of this tech.

## Users (3)

> What types (i.e., personas) of users and customers should you focus on first?
>
>  (Hint: Who buys your product or service? Who uses it? Who configures it? Etc)

There are essentially to sets of users:

1. The Authors (thought to be Tech Leads/Tech Consultants) will define the toolkits/models and possibly also the initial coding patterns to go with them for a specific codebase. This group will be attributed for the toolkit, and will maintain the toolkit and coding patterns within it for the long term, sharing them with their employers, open source projects, personal projects, communities, etc. The toolkits will be a proxy for their technical prowess in their communities.
2. The Developers (thought to be developers on a codebase) will apply the toolkits, configure them and generate the code from the models, using that code.

It will be the Authors that will pay for the capability to create these toolkits. These Authors could be employees of a company assigned to one or more team, or could be independent tech consultants who work (temporarily) with a company on one or more teams, or who contribute to one or more open source projects, or who evangelise developer tooling.

## User Outcomes & Benefits (4)

> Why would your users seek out your product or service? What benefit would they gain from using it? What behaviour change can we observe that tells us they've achieved their goal?
> (Hint: Save money, get a promotion, spend more time with family)

For Authors, they will have a single place (online repository) of toolkits/patterns that they have built up over the years representing the implementation of their "best/recommended practices" (for certain tech stacks). These can and will evolve for each company/project they work in (depending on IP constraints). We may see that they maintain variants of the same kinds of toolkits/patterns for different companies/projects (to manage IP constraints). They can choose to expose these toolkits/patterns to a limited private audience, or expose them out to the public, and advertise them across public communities. They would do this as way to advertise their prowess in these communities.

Consumers would not necessarily seek out this capability. It is more likely mandated to them on a codebase by an Author, at which point they will benefit from the coding patterns in their daily work. At least initially, until some Consumers get used to using this capability, after which they may become Authors themselves (by extracting patterns from existing codebases they are working on, from that point forward).

## Solutions (5)

> What can we make that will solve our business problem and meet the needs of our customers at the same time? List product, feature, or enhancement ideas here.

[same as: Solution](LeanCanvas.md#Solution)

## Hypotheses (6)

> Combine the assumptions from 2, 3, 4 & 5 into the following hypothesis statement:
> “We believe that [business outcome] will be achieved if [user] attains [benefit] with [feature].”
> (Hint: Each hypothesis should focus on one feature only.)

- We believe that Authors (Tech Leads/Lead Devs/Coding Architects/Tech Consultants etc) will invest some time into building tooling (to be used on their coding teams) that will be easy to use and that will reliably lay down coding patterns that they believe are important to keep consistent in a codebase.
- We believe that Consumers (Contributors on a codebase) will see value in these coding patterns, and will use the toolkits/patterns provided to them. They may however, need cues to remind them when and where to use them.
- We believe that Authors will want to show off and advertise their toolkits/patterns to certain technical communities as a way to communicate their expertise.

## What’s the most important thing we need to learn first? (7)

> For each hypothesis from Box 6, identify its riskiest assumptions. Then determine the riskiest one right now. This is the assumption that will cause the entire idea to fail if it’s wrong.
> (Hint: In the early stages of a hypothesis focus on risks to value rather than feasibility.)

Risky Assumptions:

- **Riskiest: Pre-defined coding patterns in a codebase are valuable to all those working on a codebase.**
- Coding patterns are just as applicable and important to both tech product development (new, rapidly evolving, experimental domains), and to Enterprise project development (predefined, business automation, existing business domains, components of a enterprise architecture).
- It is a Tech Lead/Lead Dev/Architect/Tech Consultant etc that sets the coding patterns for a codebase. That duty is not left to team members who only contribute to the codebase.
- Coding patterns are set at the start/early of a new software product/project, and they evolve as the team progresses on the project/product. The speed at which they evolve can vary.
- A Tech Lead/Lead Dev/Architect/Tech Consultant has (available to them, or can create) coding patterns to share with a team on a codebase.
- A Tech Lead/Lead Dev/Architect/Tech Consultant wants to ensure that the coding patterns are used by contributors on a codebase (where they are applicable)
- Tech Lead/Lead Dev/Architect/Tech Consultants understand that they are not applicable everywhere in a codebase, nor applicable at all times for all coding tasks. (They have their time and place).
- "Consistency" of code patterns, across a codebase, is important to the those working on a codebase.
- A Tech Lead/Lead Dev/Architect/Tech Consultant will go so far as to build custom tooling to promote coding patterns, for the long term.
- A Tech Lead/Lead Dev/Architect/Tech Consultant will maintain this tooling as the patterns (inevitably) change over time.
- Contributors on a codebase will use tooling provided to them by their Tech Lead/Lead Dev/Architect/Tech Consultant. (as long as they know about it).
- Tech Lead/Lead Dev/Architect/Tech Consultant AND Contributors on a codebase are motivated to improve the patterns as, and when, needed.

## What’s the least amount of work we need to do to learn the next most important thing? (8)

> Design experiments to learn as fast as you can whether your riskiest assumption is true or false.

This is a radical/disruptive innovation, that introduces a new practice that may not be known about today. i.e. provide tooling to help apply coding patterns. However, applying coding patterns is nothing new in software product/projects, and we assume has been going on for decades.

Therefore:

* The first thing to do is prove is that coding patterns are indeed used in codebases, and learn about what kinds of codebases they are used in, and what kinds they are not used in.
* Then find out who created those coding patterns, and why.
* Then find out if those coding patterns had changed over time, and by whom. (we think know "why" they change: because tech moves forward, and developers learn new things as they get more into a product/project, and they have to account for and design in other things to the codebase as the software lives longer, or scales up).

If we find that coding patterns are not that common in existing codebases, or that there are only specific kinds of codebases where they are commonly used, then we may choose that niche, or we may decide that the market is too small to invest in this technology. 