# What is it ?
Pivotal Tracker FluentAPI is C# API that uses the Fluent pattern to connect to the PivotalTracker REST API.

ptfluentapi-portable is a portable version of Pivotal Tracker FluentAPI. It has many changes and now PCL version support Pivotal Tracker v5 APIs instead of legacy v3 APIs.

# How to use it ?

First create the Pivotal Tracker Facade

```csharp
	var token = new Token("APIKEY"); //get a pivotal API key from your Profile
	var Pivotal = new PivotalTrackerFacade(token);
```

List all stories

```csharp
(await
(await Pivotal.Projects().GetAsync(123456))
    .Stories().AllAsync())
    .Each(s => Console.WriteLine("{0} : {1}", s.Item.Name, s.Description));
```

List some stories

```csharp
var filteredStories = (await (await Pivotal.Projects().GetAsync(123456))
    .Stories().FilterAsync("label:ui state:started"));
filteredStories.Each(s => Console.WriteLine("{0} : {1}", s.Item.Name, s.Description));
```

Create a story

```csharp
(await Pivotal.Projects().GetAsync(123456)).Stories()
    .Create()
        .SetName("Hello World")
        .SetType(StoryTypeEnum.Bug)
        .SaveAsync();
```

Complete sample

1. create a project
2. create a story
3. add a note
4. start a story
5. then retrieves all stories in started state

```csharp
(await
(await
(await
(await
(await Pivotal.Projects().GetAsync(Project.Id)) //ProjectId
    .Stories()
    .Create()
    .SetName("This is my first story")
    .SetType(StoryTypeEnum.Feature)
    .SetDescription("i'am happy it's so easy !")
    .SaveAsync())
        .AddNoteAsync("this is really amazing"))
            // NOTE: attaching files on stories is not yet supported.
            // .UploadAttachment(someBytes, "attachment.txt", "text/plain")
            .UpdateAsync(story =>
            {
                story.Estimate = 3;
                story.CurrentState = StoryStateEnum.Started;
            })).Done()
            .FilterAsync("state:started"))
                .Do(stories =>
                {
                    foreach (var s in stories)
                    {
                        Console.WriteLine("{0}: {1} ({2})", s.Id, s.Name, s.Type);
                        foreach (var n in s.Notes)
                        {
                            Console.WriteLine("\tNote {0} ({1}): {2}", n.Id, n.Description, n.NoteDate);
                        }
                    }
                });

```

There is many other methods. Just download the code and let's follow the Fluent API :)
