      ┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐
      ├─┤│ │ │ │ ││││├─┤ │ ├┤ 
      ┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘

![Concept](https://github.com/jezzsantos/automate/raw/main/docs/Images/Concept.png)

# What problem does it solve?

Often, for many software teams (including individuals with varying degrees of expertise from novice to expert) a Tech Lead/Lead Dev/Tech Consultant will at some point see the need to make some key technical decisions about how things need to get done in that codebase.

Architecturally, structurally or in the details of the implementation - all these decisions are constraints on the codebase. Keeping things small, well defined, and consistent is often the key to managing complexity over time. So that contributors to this codebase can have a single, unified and simpler understanding of the codebase -> A reasonable, shared mental model of the codebase, so that everyone touching it can move freely across the codebase and not fear changing any part of it.

Being able to construct codebases in this way is earned over the years of doing similar things in other codebases over and over again. As this is done, patterns emerge and they are adapted and improved for each codebase where they are applied. No two codebases are the same, but the patterns are often very similar indeed. Often improving over time as those that define and refine them learn more each time they are applied from the context of the current codebase.

The primary challenge a Tech Lead/Lead Dev/Tech Consultant has: is communicating this knowledge in a form that can be learnt, reused and adapted by others for the current codebase. Learning by doing is key, but learning from experienced people through demonstration is a more effective way that we can avoid repeating the same expensive mistakes from the past.

One of the main challenges in codebases (for a whole team) is that others in the codebase will naturally re-invent unfamiliar or more novel code patterns because they lack the knowledge that other team members have, or they are unaware that the other team members already possess this kind of knowledge. Teams are also fluid over time, with members coming and going. So the learning process is continuous and relentless.

Another more common challenge here is not knowing how to solve certain problems in the codebase nor where to put that stuff in the codebase. This results in solutions being peppered around the codebase as workarounds are introduced, and pilled on top of each other.

All these issues contribute to creating a big-ball-of-mud over time, which becomes un-navigable for a team as time moves forward.

### The call to action

A Tech Lead/Lead Dev/Tech Consultant needs to remain vigilant to ensure that established patterns and architectures are not violated because of a lack of knowledge from team members in the same codebase. Whilst, they also need to adapt to the software as it evolves, and as new constraints are defined.

Today's, languages and development tools (IDEs) are so *general purpose* that they cannot be used to help define or enforce constraints on the programmer (in different ways in different codebases) and thus they cannot prevent the programmer (or warn them) when they are working around specific constraints to that codebase.

Codebase-specific tools, fit for purpose, must be used to define and enforce these constraints. But more importantly, those tools must adapt to the changing and evolving codebase as it changes over time.

Tech Lead/Lead Dev/Tech Consultant could use a little help with defining their own codebase-specific constraints and have adaptable tools that communicate and enforce those constraints.