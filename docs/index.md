<pre class="logo-font">
  ┌─┐┬ ┬┌┬┐┌─┐┌┬┐┌─┐┌┬┐┌─┐
  ├─┤│ │ │ │ ││││├─┤ │ ├┤ 
  ┴ ┴└─┘ ┴ └─┘┴ ┴┴ ┴ ┴ └─┘
</pre>
![Concept](https://github.com/jezzsantos/automate/raw/main/docs/Images/Concept.png)

# What problem does it solve?

In just about all long-lived software products/projects there will become a need to make some key decisions about how things should get done in that codebase. Structure and patterns emerge, and are repeated and evolve as things change over time - we learn.

Given that, with all long-lived codebases, people come and go all the time, and everyone has developed their own ways of writing code to solve problems. On your next codebase, you wish there had better be some structure, patterns, or rules to follow, or else chaos reigns as individuals do their own thing their own way. Staying consistent in a team of developers is a major goal in any collaboration.

### Managing Complexity in Codebases

Architecture, structure, and the patterns in the fine details of the implementation - are all constraints on a codebase. Keeping things small, well-defined, and consistent is critical to managing complexity over time as the code grows and dependencies are added. Contributors to a codebase must have a single, unified and simpler understanding of the codebase -> a shared "mental model" of the codebase so that everyone touching it can move freely across the codebase, know where things go, and NOT fear changing any part of it.

Being able to lay down codebases in this way is a skill earned over the years of doing similar things in other codebases over and over again. As that is done, patterns emerge and they are adapted and improved on each successive codebase where they are applied. No two codebases are the same, but the patterns are often very similar indeed. Often improving over time as those that define and refine them learn more each time they are applied from the context of the current codebase.

### Sharing Know-How

The primary challenge a Tech Lead/Lead Dev/Tech Consultant has is communicating their hard-earned knowledge in a portable form that can be learned, reused, and adapted by others for the current codebase and context.

Learning by doing is key, but learning directly from experienced people through tools (demonstration in code) is far more effective at avoiding repeating the same expensive mistakes that were learned in the past.

One of the main challenges in codebases (for teams) is that without direction, others in the codebase will naturally re-invent unfamiliar or novel code patterns because they lack the knowledge that other team members may have. They are likely unaware that the others before them already possess this kind of knowledge. They just don't know where to look for it.

These kinds of problems are common. Left unchecked, codebases become too complex too fast, and that slows down progress over time working within them, some times progress grinds to a halt.

### Developer Tooling Today

Today's, languages and development tools (IDEs) are so fantastically powerful, but they are also very general-purpose tools. Beyond code formatting/linting tools, they cannot be easily configured to help define or enforce necessary constraints on the programmer, in ways that make sense to a specific codebase. Thus they are not effective at preventing the programmer when they start working around specific constraints defined for that codebase. If anything, they make it too easy for the programmer to do that. 

Consider how easy it is to add a reference to a component/symbol/type across a codebase, regardless of the fact that this new dependency (that was now created) may violate the intended architecture, patterns, and rules of the codebase. 

Codebase-specific developer tools are fit for this purpose, but building those tools is too difficult for most to embark on doing it when they need them. What is needed is a platform that makes that trivial to build those tools that can be applied easily to a codebase, and reused in other codebases.

But more importantly, those codebase-specific tools must evolve and adapt to the changing and evolving codebase, and change in lockstep with it.

Automate is this platform.

Tech Lead/Lead Dev/Tech Consultants can use quite a bit of help with defining their own codebase-specific constraints and have adaptable tools that communicate and enforce those constraints for their teams.