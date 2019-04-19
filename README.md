# Schedule Map &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://github.com/your/your-project/blob/master/LICENSE)

## Summary

The best way to explain this project is with a game. Look at the following graph:

![Game 1](Images/Game1.png)

You play as the blue character. During a turn, you can
- Stay on your current node, or
- Move to a connected node

After each turn, every pile on the board loses a red square token. If you are on a node at the point when it loses its last red token, you gain all the green circle tokens in that pile. If you arrive too early, you'll have to wait until all the red tokens are gone. If you are too late, you get nothing. The goal of the game is to get as many green tokens as possible.

Now, let's make the game harder:

![Game 2](Images/Game2.png)

Now, moving across an edge takes the indicated amount of turns. Another way to sat it is whenever you move across an edge, the indicated number of red tokens is taken from every pile on the board.

Now, let's make it harder again:

![Game 3](Images/Game3.png)

Now, some piles have yellow pentagonal tokens. In order to get the green tokens from those piles, you have to:
- Be at the node when the last red token is gone
- Stay at the node while the yellow tokens disappears at the same rate as the red tokens i.e. one token per turn

This last game is the type of game that this project is solving.

## How it works

Basically, for every decision that a character makes, they create copies of themself, and sends the copy of on the choice. At the end of each turn, every node compares all the character copies that have arrived at said node. It keeps the copy that has the largest number of tokens, and destroys the rest.

Then, if the node has any piles that have just arrived at the yellow token stage, an extra copy of the character is made to wait at the node for the number of yellow tokens, and is then reintroduced into the mix at the end of the yellow tokens.

## How to use

Consider the following code as a quick example of use (will return a Queue of strings)
```
new Map<String>(
    new List<string> { "A", "B", "C", "D", "E", "F", "G" },
    new Dictionary<string, Dictionary<string, int>> {
        {"A", new Dictionary<string, int>{ { "B", 3 }, { "D", 1 }, { "E", 1 }}},
        {"B", new Dictionary<string, int>{ { "A", 3 }, { "C", 3 }, { "D", 2 }}},
        {"C", new Dictionary<string, int>{ { "B", 3 }}},
        {"D", new Dictionary<string, int>{ { "A", 1 }, { "B", 2 }, { "E", 1 }}},
        {"E", new Dictionary<string, int>{ { "A", 1 }, { "D", 1 }, { "F", 2 }, { "G", 1 }}},
        {"F", new Dictionary<string, int>{ { "E", 2 }, { "G", 2 }}},
        {"G", new Dictionary<string, int>{ { "E", 1 }, { "F", 2 }}}
    }
).GetBot(
    new List<ScheduleMap.ScheduleItem<string>> {
        new ScheduleMap.ScheduleItem<string>( "C", 5, 3 ),
        new ScheduleMap.ScheduleItem<string>( "D", 6, 1, 1 ),
        new ScheduleMap.ScheduleItem<string>( "D", 7, 6 ),
        new ScheduleMap.ScheduleItem<string>( "E", 2, 6, 2 ),
        new ScheduleMap.ScheduleItem<string>( "F", 7, 8 ),
        new ScheduleMap.ScheduleItem<string>( "G", 3, 3 )
    }, "A", 0, 8
).Path;
```

To instantiate a Schedule Map with each node represented by a class T (in the above case, a string), two things are needed:
- A List of all the T instances that should be represented as a node
- A Dictionary with T instances as keys, and for values, either
    - A List of T instances the T instance is attached to.
    - A Dictionary with the attached T instance as the key and the number of turns it takes to get from one to the other as the value

To get a list of directions (in the form of a Queue of T instances), you first want to use the GetBot function, with the following inputs:
- A List of ScheduleItems(the token piles), which are instantiated with
    - The T instance that holds the token pile
    - The number of green tokens
    - The number of red tokens
    - The number of yellow tokens, if neccessary
- The T instance that will be the starting node
- How many red tokens should be taken off at the beginning
- How many turns should be planned out

This will return a bot that got the most green tokens in the game. From this bot, you can get either:
- Path: A Queue of the T nodes that the bot passed on it's way to get the green token, with repititions of a node if the bot stayed there for a number of turns. Any turns that was spent on the path between nodes is not included in the Queue.
- Instructions: A Queue of instructions to replicate the bot's path, including an instruction at the start to indicate the starting point. Each instruction includes the following attributes:
    - value: Which T node to go to
    - time: Which turn to start heading on the path to go to the node
    - score: How many green nodes the bot has at that point in time