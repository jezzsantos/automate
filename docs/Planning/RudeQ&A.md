# Rude Q & A

## Why this technology?

The value proposition is still immediately obvious to me, it is still relevant to tech today, the tech already exists (in a form).

I started investigating the automation of code patterns back in the early 2000's at Microsoft as a channel to teach and disseminate architectural/technical guidance to developers as a technical consultant in the APAC/EMEA region. Adopting and learning .NET best practices and architectural patterns, and technical leadership, was a common demand from enterprise customers to Microsoft Consulting Services. My journey into developer tooling started out with attempting to teach enterprise developers how to use specific (recommended) libraries and frameworks as intended (eg. Enterprise Library and others). That led into the development of architectural frameworks (eg. Enterprise Framework) that implemented the best practice guidance coming out of the Microsoft patterns & practices team in code assets that our enterprise customers adopted. Clearly, raising challenges around versioning and support once adopted.

At the same time, the MS patterns & practices team was looking at moving from delivering just written guidance to delivering "actionable guidance" and investigating open source, binary and tooling approaches for doing that. We saw the emergence of the first automation tooling from Microsoft in the "Guidance Automation Toolkit" that wrote the code configuration for various fixed architectural patterns. At the same time deep investments (in talent acquisition from industry and money) were being made into DSM and DSL tooling in the Visual Studio teams, and the MS Software Factories movement started from the new thought leadership around this kind of approach. After establishing the movement, the Visual Studio team were creating product plans for a new software factory tooling platform, and it was discovered that I had already created a working software factory independently in the field, and was already selling it to our enterprise customers. Everything was aligning in that space at
Microsoft for a new set of DSM tools.

Then serendipitously, a technical partnership was sought by Raytheon with MS to create an in-house program to adopt these kinds of DSM tools (software factories) to be adopted across their vast programs of work. A significant opportunity for MS to introduce its .NET platform into another US defence contractor. We established the program at Raytheon in Boston, and we worked side by side supporting Raytheon in developing the training facility. At the same time, informing the development of the new Visual Studio platform for this kind of tooling. Sadly, the Visual Studio team unexpectedly changed strategy (series of unfortunate events), and exited the DSM space. That required MS to step in (executive support to save the partnership) and build this tooling independently from the core Visual Studio team. I led a new product team based out of Buenos Aires staffed with our best partners in this space (Clarius Consulting), and we build what became the VSPAT technology - the first actual DSM
tooling platform. Raytheon adopted the new technology in its programs. But as the partnership with Raytheon matured, at risk of this technology being exclusive to only Raytheon (and losing support in the wider community) I led the efforts to open source VSPAT, and it evolved into NuPattern on github.com.

I've always had a desire to help engineers/developers work more effectively, no matter what technology, language, platform. I've discovered the hard way that there are patterns (coding and interaction) that work in each and every software/hardware context, that persist over time and evolve over time as the whole tech stack below changes. Mastering software over the course of my career has been about adapting and evolving these patterns, orthogonal to the specific domain or product being built. All long lived software has patterns in it, that are repeated over and over again, and keeping them consistent is paramount to keeping up the pace of working in a codebase as it becomes larger and more complex.

This is difficult skill to master, and as such, many professionals take years to get there, meanwhile they are not reaping the benefits of having them. That is a deep shame, and I believe the causes are many, but primarily. Too little experience in most of the software teams out there, coupled with poor ways to communicate guidance to developers as they are writing code.

## Why now?

Reflecting on the fact that ~15 years has past (since the story above), and in the context of the work I do now (teaching product development to engineers) and in the work I was doing in my own start-up for the last 8 years - building APIs) I see opportunity for reaping the benefits of this capability again, in the contexts I work in.

Perhaps, the industry had matured enough now, to seek out this capability as being more important than it was before?

## Why you?

It is a space that I (and others) understand deeply, and I see opportunity for,

It's a general mission I care about deeply.

I've (and others) have had the experience of bringing this capability to market before and failing at it.

## Why did it fail last time around?

(I'm not so certain about these conclusions about last time around.)

We failed to get more traction in the wider industry because:

1. Compelling working examples were hard (lacked good ideas) to create and demonstrate effectively to general audiences. The working examples that were demonstrated did not seem to resonate with general audiences. Specific working examples did resonate with specific audiences, but then the details seemed to distract audiences from the overarching value. The time required to ideate the general examples, and the time to implement the specific examples was very scarce.
1. Due to some of the novelty of this tooling approach (not seen before), realising some of the potential power it, and what it could deliver was often not exposed. Only those who understood it deeply could "see" that power, and then see the potential of wielding it.
1. We were coupled too tightly with Visual Studio (at the time) that was changing its automation interfaces too rapidly (once every 2 years). We had to support Vcurrent-3 implementations, to support existing users who migrated to newer version of Visual Studio slowly.
3. Visual Studio performance was a major usability constraint (when enumerating and analysing medium/large solutions/projections/folders and files).
4. There was only 1, sometimes 2 contributors maintaining this project.
5. The niche target audience was never really identified. It was never meant to be general purpose for all programmers in all contexts. There was no obvious channel to market, or specific go to market strategy.
5. I have found from many (audible/textual) interactions with technical people (tech consultants, program managers etc, even those in the developers tools space, and those who are highly experienced too) that when trying to explain the objectives and techniques of this approach often fails to hit the mark, and resonate with them. First, there seems to be a barrier in terms of language that makes it very hard to describe exactly how the benefits are experienced. Then there seems to be a deep scepticism that that seems more based upon lack of actual experience at the coalface programming, and empathy for real world developers. Whatever the reason, in conclusion, without demonstrating concrete examples that are meaningful to the present audience), the value proposition is very hard to communicate to any audience. This would explain why this kind of tech is hard to understand just from a simple conversation, and may only be realised by using this technique. This is a significant barrier.

## What makes you think it will succeed now?

I will need a lot more evidence that it is desirable first. Then I'll need to believe in a model that could be financially viable in the long run.

Then we will have to find a way to overcome the various challenges above, for this to result in an outcome that is different than what we experienced prior.

## What makes you think you can make it succeed now?

That's not certain at all. I can't do it alone anymore. It will need more technical, financial and marketing investment.

It's not a full time commitment at this point, and could not sustain a living yet. But it could be a worthy community project.

Perhaps, one channel to market could be to ship and support toolkits for popular technologies, as a way to demonstrate its capabilities.

## Why are you the best person to start this?

I'I should be one of the Founders of this business because this capability has been part of my life for decades and I've explored it in more than one way or another more than almost any other person. I'm not the best person to front this business, I'm not a CEO, nor want to do what good CEO's do , but contributing to the product vision and strategy and execution is where I should bring my skills to bare.

## Who would use this capability and what for?

There is no doubt that this is NOT a general purpose tool for all programmers all of the time.

There are many kinds of programming (as well as many platforms) and individual programmers perform many kinds different programming tasks even on the same software project/product. Even on the same codebase, using tooling like this, there are times when it is not the appropriate tool for the task. And conversely there are some tasks that benefit from it hugely.

* Specifically, activities around the creation of new components, where there is much activity across many projects/folders/files setting things up to support the bespoke programming that must happen.

* Specifically, in codebases where there are well-defined technical patterns already agreed to, that should be followed to get things established.

## What is it all about?

* Reusing knowledge
* Applying custom Coding Patterns reliably and consistently
* Automating Recipes for quickly building code
* Configuring things with different options, to adapt to different use cases.
* Recording what you've learnt over the years
* Speeding up the dull and boring parts of working in a codebase
* Being and staying consistent.
* Evolving consistently

