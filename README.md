# HandEvaluator

[![NuGet](https://img.shields.io/nuget/v/Ck.HandEvaluator.svg)](https://www.nuget.org/packages/Ck.HandEvaluator)
[![NuGet downloads](https://img.shields.io/nuget/dt/Ck.HandEvaluator.svg)](https://www.nuget.org/packages/Ck.HandEvaluator)
[![CI](https://github.com/captainkout/HandEvaluator/actions/workflows/ci.yml/badge.svg)](https://github.com/captainkout/HandEvaluator/actions/workflows/ci.yml)

Fast Texas Holdem hand evaluation and analysis. Pulled from [Fast Texas Holdem Hand Evaluation and Analysis](https://www.codeproject.com/Articles/12279/Fast-Texas-Holdem-Hand-Evaluation-and-Analysis) on CodeProject, with some basic tests added as examples, and then published to NuGet as [`Ck.HandEvaluator`](https://www.nuget.org/packages/Ck.HandEvaluator).

> **Note:** the NuGet package ID is `Ck.HandEvaluator`; the assembly and root namespace remain `HandEvaluator` and `HoldemHand`. The unrelated `HandEvaluator` package on NuGet.org is owned by a different author.

## Installation

```bash
dotnet add package Ck.HandEvaluator
```

## Quick start

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using HoldemHand;

// Simple example of using the Holdem.Hand class
class Program
{
    static void Main(string[] args)
    {
        // initialize board
        string board = "2d kh qh 3h qc";
        // Create a hand with AKs plus board
        Hand h1 = new Hand("ad kd", board);
        // Create a hand with 23 unsuited plus board
        Hand h2 = new Hand("2h 3d", board);

        // Find stronger hand and print results
        if (h1 > h2)
        {
            Console.WriteLine("{0} greater than \n\t{1}", h1.Description, h2.Description);
        }
        else
        {
            Console.WriteLine("{0} less than or equal \n\t{1}", h1.Description, h2.Description);
        }
    }
}
```

## Performance

The `HandEvaluator.SpeedTest` console project includes a throughput mode that measures how many
heads-up (two-player) 7-card evaluations can be completed in one wall-clock second. One two-player
evaluation performs two `Hand.Evaluate(mask, 7)` calls — one for each player's seven cards (two
pocket cards plus the shared five-card board).

Measured on Windows 10 (10.0.19045.6466) (Intel64 Family 6 Model 154 Stepping 3), .NET 10.0.401, Release build:

| Seed | 2-player evaluations / second | Single `Hand.Evaluate` calls / second |
|------|------------------------------:|--------------------------------------:|
| 1     | 35913728 | 71827456 |
| 42    | 36503552 | 73007104 |
| 777   | 36438016 | 72876032 |
| 12345 | 36831232 | 73662464 |
| 2026  | 37093376 | 74186752 |

These figures are machine-dependent and are provided as an order-of-magnitude reference, not a
guarantee. Reproduce on your own machine with:

```bash
dotnet run --project HandEvaluator.SpeedTest -c Release -- --throughput --seed <seed>
```

## API Reference

This API reference is generated from the compiler documentation XML (`HandEvaluator.xml`) emitted by the build. That XML ships alongside the assembly in the NuGet package at `lib/net10.0/HandEvaluator.xml` and is the source of truth for this reference. See [`RELEASING.md`](RELEASING.md) for how releases are produced.

### Hand

Represents a Texas Holdem Hand

*(Full name: `HoldemHand.Hand`)*

**Constructors**

#### `Hand()`

Default constructor

#### `Hand(string, string)`

Constructor

- **pocket**: Pocket Cards
- **board**: Board

```csharp
using System;
using System.Collections.Generic;
using System.Text;
using HoldemHand;

// Simple example of using the Holdem.Hand class
class Program
{
    static void Main(string[] args)
    {
        // initialize board
        string board = "2d kh qh 3h qc";
        // Create a hand with AKs plus board
        Hand h1 = new Hand("ad kd", board);
        // Create a hand with 23 unsuited plus board
        Hand h2 = new Hand("2h 3d", board);

        // Find stronger hand and print results
        if (h1 > h2)
        {
            Console.WriteLine("{0} greater than \n\t{1}", h1.Description, h2.Description);
        }
        else
        {
            Console.WriteLine("{0} less than or equal \n\t{1}", h1.Description, h2.Description);
        }
    }
}
```

**Properties**

#### `Hand.MaskValue`

Returns hand mask value

#### `Hand.PocketMask`

Represents the Mask of the Pocket cards for this instance of Hand

#### `Hand.BoardMask`

Represents the Mask of the Board cards for this instance of Hand

#### `Hand.PocketCards`

Returns/Sets pocket card string

#### `Hand.Board`

Returns/Sets board card string

#### `Hand.HandValue`

Returns/Sets the hand value. This value may be used to compare one hand to another using standard numeric compares.

#### `Hand.Description`

Returns a textual description of the current hand

#### `Hand.HandTypeValue`

Returns the current hand type.

**Methods**

#### `Hand.HandOdds(string[], string, string, long[], long[], long[], out long)`

Used to calculate the wining information about each players hand. This function enumerates all possible remaining hands and tallies win, tie and losses for each player. This function typically takes well less than a second regardless of the number of players.

- **pockets**: Array of pocket hand string, one for each player
- **board**: the board cards
- **dead**: the dead cards
- **wins**: An array of win tallies, one for each player
- **ties**: An array of tie tallies, one for each player
- **losses**: An array of losses tallies, one for each player
- **totalHands**: The total number of hands enumarated.

#### `Hand.Outs(ulong, ulong, ulong[])`

Returns the number of outs possible with the next card.

- **player**: Players pocket cards
- **board**: THe board (must contain either 3 or 4 cards)
- **opponents**: A list of zero or more opponent cards.

**Returns:** The count of the number of single cards that improve the current hand.

#### `Hand.OutsMask(ulong, ulong, ulong[])`

Creates a Hand mask with the cards that will improve the specified players hand against a list of opponents or if no opponents are list just the cards that improve the players current had. Please note that this only looks at single cards that improve the hand and will not specifically look at runner-runner possiblities.

- **player**: Players pocket cards
- **board**: The board (must contain either 3 or 4 cards)
- **opponents**: A list of zero or more opponent pocket cards

**Returns:** A mask of all of the cards taht improve the hand.

#### `Hand.IsSuited(ulong)`

This function returns true if the cards in the hand are all one suit

- **mask**: hand to check for "suited-ness"

**Returns:** true if all hands are of the same suit, false otherwise.

#### `Hand.IsConnected(ulong)`

Returns true if the cards in the two card hand are connected.

- **mask**: the hand to check

**Returns:** true of all of the cards are next to each other.

#### `Hand.GapCount(ulong)`

Counts the number of empty space between adjacent cards. 0 means connected, 1 means a gap of one, 2 means a gap of two and 3 means a gap of three.

- **mask**: two card hand mask

**Returns:** number of spaces between two cards

#### `Hand.HandPlayerOpponentOdds(ulong, ulong, out double[], out double[])`

Given a set of pocket cards and a set of board cards this function returns the odds of winning or tying for a player and a random opponent.

- **ourcards**: Pocket mask for the hand.
- **board**: Board mask for hand
- **player**: Player odds as doubles
- **opponent**: Opponent odds as doubles

#### `Hand.HandPlayerOpponentOdds(string, string, out double[], out double[])`

Given a set of pocket cards and a set of board cards this function returns the odds of winning or tying for a player and a random opponent.

- **pocketcards**: Pocket cards in ASCII
- **boardcards**: Board cards in ASCII
- **player**: Player odds as doubles
- **opponent**: Opponent odds as doubles

#### `Hand.HandPotentialOpp(ulong, ulong, ulong, int, out int[,])`

Internal function used by HandPotential.

#### `Hand.HandPotential(ulong, ulong, out double, out double)`

Returns the positive and negative potential of the current hand. This funciton is described in Aaron Davidson's masters thesis (davidson.msc.pdf).

- **pocket**: Hold Cards
- **board**: Community cards
- **ppot**: Positive Potential
- **npot**: Negative Potential

#### `Hand.ValidateHand(string)`

This function takes a string representing a full or partial holdem hand and validates that the text represents valid cards and that no card is duplicated.

- **hand**: hand to validate

**Returns:** true of a valid hand, false otherwise

#### `Hand.ValidateHand(string, string)`

This function takes a string representing pocket cards and a board and then validates that the text represents a valid hand.

- **pocket**: Pocket cards as a string
- **board**: Board cards as a string

#### `Hand.ParseHand(string)`

Parses an string description of a hand and returns a hand mask.

- **hand**: string descripton of a hand

**Returns:** a hand mask representing the hand

```csharp
// Takes an ascii seven card hand and prints out a 
// value description. For example: "ad kd 2d kh qh 3h qc" would
// output "Two pair, King's and Queen's with a Ace for a kicker"
static void PrintDescription(string hand)
{
    // Parse hand into a hand mask
    ulong handmask = Hand.ParseHand(hand);

    // Convert hand mask into a compariable hand value.
    uint handval = Hand.Evaluate(handmask, 7);

    // Print a description of the hand.
    Console.WriteLine("Hand: {0}", Hand.DescriptionFromHandValue(handval));
}
```

#### `Hand.ParseHand(string, out int)`

#### `Hand.ParseHand(string, string, out int)`

This static method parses the passed pocket cards and board and produces a card mask.

- **pocket**: ASCII string representing pocket cards
- **board**: ASCII string representing board
- **cards**: Number of cards represented in mask

#### `Hand.ParseCard(string)`

Reads an string definition of a card and returns the Card value.

- **card**: card string

#### `Hand.NextCard(string, out int)`

Parses Card strings (internal)

- **cards**: string containing hand definition
- **index**: iterator into card string

#### `Hand.CardRank(int)`

Given a card value, returns it's rank

- **card**: card value

**Returns:** returns rank

#### `Hand.CardSuit(int)`

Given a card value, returns it's suit

- **card**: Card value

**Returns:** suit

#### `Hand.DescriptionFromHandValue(uint)`

Takes a hand value and returns a description string. This function is obsolute. The use of Hand.DescriptionFromMask(ulong mask) is preferred.

- **handValue**: A hand value from Evaluate or Evaluate

**Returns:** A description string of the value of the hand

#### `Hand.MaskToDescription(ulong)`

Take a card mask like the ones that come out or the Hand.Parse functions.

- **mask**: hand mask

**Returns:** returns a description of the hand value

#### `Hand.DescriptionFromMask(ulong)`

#### `Hand.DescriptionFromHand(string)`

Takes an string describing a hand and returns the description.

- **hand**: the string describing the hand

**Returns:** Returns a description string

```csharp
// Print a description of the hand.
Console.WriteLine("Hand: {0}", Hand.DescriptionFromHand("ad kd 2d kh qh 3h qc"));
```

#### `Hand.UpdateHandMask()`

Updates handmask and handval, called when card strings change

#### `Hand.ToString()`

Returns the string representing the hand.

#### `Hand.Mask(int)`

This is a fast way to look up the index mask.

- **index**: index of mask

**Returns:** mask

#### `Hand.MaskToString(ulong)`

Turns a card mask into the equivalent human readable string.

- **mask**: mask to convert

**Returns:** human readable string that is equivalent to the hand represented by the mask

#### `Hand.EvaluateType(ulong)`

Evaluates the card mask and returns the type of hand it is. This function is faster (but provides less information) than Evaluate or Evaluate.

- **mask**: card mask

**Returns:** A HandTypes value

```csharp
public static long ValidateEnumerate5()
{
    int[] handtypes = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    int count = 0;

    // Iterate through all possible 5 card hands
    foreach (ulong mask in Hands(5))
    {
        handtypes[(int)Hand.EvaluateType(mask)]++;
        count++;
    }

    // Validate results.
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.HighCard] == 1302540);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.Pair] == 1098240);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.TwoPair] == 123552);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.Trips] == 54912);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.Straight] == 10200);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.Flush] == 5108);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.FullHouse] == 3744);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.FourOfAKind] == 624);
    System.Diagnostics.Debug.Assert(handtypes[(int)HandTypes.StraightFlush] == 40);
    System.Diagnostics.Debug.Assert(count == 2598960);
    return count;
}
```

#### `Hand.EvaluateType(ulong, int)`

This function is faster (but provides less information) than Evaluate or Evaluate.

- **mask**: card mask
- **cards**: number of cards in mask

**Returns:** HandType enum that describes the rank of the hand

#### `Hand.Evaluate(ulong)`

Evaluates a hand (passed as a hand mask) and returns a hand value. A hand value can be compared against another hand value to determine which has the higher value.

- **cards**: hand mask

**Returns:** Hand Value bit field

#### `Hand.Evaluate(string)`

Evaluates a hand (passed as a string) and returns a hand value. A hand value can be compared against another hand value to determine which has the higher value.

- **hand**: hand string

**Returns:** Hnad Value bit field

#### `Hand.Evaluate(ulong, int)`

Evaluates a hand (passed as a hand mask) and returns a hand value. A hand value can be compared against another hand value to determine which has the higher value.

- **cards**: hand mask
- **numberOfCards**: number of cards in the hand

**Returns:** hand value

```csharp
// Takes an ascii seven card hand and prints out a 
// value description. For example: "ad kd 2d kh qh 3h qc" would
// output "Two pair, King's and Queen's with a Ace for a kicker"
static void PrintDescription(string hand)
{
    // Parse hand into a hand mask
    ulong handmask = Hand.ParseHand(hand);

    // Convert hand mask into a compariable hand value.
    uint handval = Hand.Evaluate(handmask, 7);

    // Print a description of the hand.
    Console.WriteLine("Hand: {0}", Hand.DescriptionFromHandValue(handval));
}
```

#### `Hand.CompareTo(object)`

Used to compare one hand to another. This method allows normal compare functions to work as expected with a hand.

- **obj**: object to compare against

#### `Hand.Equals(object)`

Test for equality

**Returns:** returns true if equal, false otherwise

#### `Hand.GetHashCode()`

Returns hash code

**Returns:** Hash code

#### `Hand.operator ==(Hand, Hand)`

Test for equality

- **op1**: left side object
- **op2**: right side object

**Returns:** returns true if equal, false otherwise

```csharp
// These two hands are equal because the five best cards
// in both hands are the same.
Hand h1 = new Hand("ac as", "4d 5d 6c 7c 8d");
Hand h2 = new Hand("td js", "4d 5d 6c 7c 8d");

if (h1 == h2)
{
    Console.WriteLine("Hands are equal");
}
```

#### `Hand.operator !=(Hand, Hand)`

Test for inequality.

- **op1**: left side object
- **op2**: right side object

**Returns:** returns true if not equal, false otherwise

```csharp
Hand h1 = new Hand("ac as", "4d 5d 6c 7c 8d");
Hand h2 = new Hand("td 9s", "4d 5d 6c 7c 8d");

if (h1 != h2)
{
    Console.WriteLine("Hand h2 is a higher straight");
}
```

#### `Hand.operator >(Hand, Hand)`

Test that the left side is greater than the right side.

- **op1**: left side
- **op2**: right side

**Returns:** returns true of the left item is greater than the right item

```csharp
Hand h1 = new Hand("ac as", "4d 5d 6c 7c 8d");
Hand h2 = new Hand("td 9s", "4d 5d 6c 7c 8d");

if (h2 > h1)
{
    Console.WriteLine("Hand h2 is a higher straight");
}
```

#### `Hand.operator >=(Hand, Hand)`

Test that the left side is greater or equal than the right side.

- **op1**: left side
- **op2**: right side

**Returns:** returns true of the left item is greater or equal than the right item

```csharp
Hand h1 = new Hand("ac as", "4d 5d 6c 7c 8d");
Hand h2 = new Hand("td 9s", "4d 5d 6c 7c 8d");

if (h2 >= h1)
{
    Console.WriteLine("Hand h2 is a higher straight");
}
```

#### `Hand.operator <(Hand, Hand)`

Test that the left side is less than the right side.

- **op1**: left side
- **op2**: right side

**Returns:** returns true if the left item is less than the right item.

#### `Hand.operator <=(Hand, Hand)`

Test that the left side is less than or equal to the right side.

- **op1**: left side
- **op2**: right side

**Returns:** returns true if the left item is less than or equal to the right item.

#### `Hand.BitCount(ulong)`

Fast Bitcounting method (adapted from snippets.org)

- **bitField**: ulong to count

**Returns:** number of set bits in ulong

#### `Hand.PocketHand169Type(ulong)`

Given a pocket pair mask, the PocketPairType cooresponding to this mask will be returned.

#### `Hand.Hands(int)`

Enables a foreach command to enumerate all possible ncard hands.

- **numberOfCards**: the number of cards in the hand (must be between 1 and 7)

```csharp
// This method iterates through all possible 5 card hands and returns a count.
public static long CountFiveCardHands()
{
    long count = 0;

    // Iterate through all possible 5 card hands
    foreach (ulong mask in Hands(5))
    {
        count++;
    }
            
    // Validate results.
    System.Diagnostics.Debug.Assert(count == 2598960);
    return count;
}
```

#### `Hand.Hands(ulong, ulong, int)`

Enables a foreach command to enumerate all possible ncard hands.

- **shared**: A bitfield containing the cards that must be in the enumerated hands
- **dead**: A bitfield containing the cards that must not be in the enumerated hands
- **numberOfCards**: the number of cards in the hand (must be between 1 and 7)

```csharp
// Counts all remaining hands in a 7 card holdem hand.
static long CountHands(string partialHand)
{
    long count = 0;

    // Parse hand and create a mask
    ulong partialHandmask = Hand.ParseHand(partialHand);

    // iterate through all 7 card hands that share the cards in our partial hand.
   foreach (ulong handmask in Hand.Hands(partialHandmask, 0UL, 7))
   {
       count++;
   }

   return count;
 }
```

#### `Hand.GetRandomHand(ulong, int, System.Random)`

Returns a rand hand with the specified number of cards and constrained to not contain any of the passed dead cards.

- **dead**: Mask for the cards that must not be returned.
- **ncards**: The number of cards to return in this hand.
- **rand**: An instance of the Random class.

**Returns:** A randomly chosen hand containing the number of cards requested.

#### `Hand.RandomHands(ulong, ulong, int, int)`

This function iterates through random hands returning the number of random hands specified in trials. Please note that a mask can be repeated.

- **shared**: Cards that must be in the hand.
- **dead**: Cards that must not be in the hand.
- **ncards**: The total number of cards in the hand.
- **trials**: The total number of random hands to return.

**Returns:** Returns a random hand mask meeting the input specifications.

#### `Hand.RandomHands(int, int)`

Iterates through random hands with ncards number of cards. This iterator will return the number of masks specifed in trials. Masks can be repeated.

- **ncards**: Number of cards required to be in the hand.
- **trials**: Number of total mask to return.

**Returns:** A random hand as a hand mask.

#### `Hand.QueryPerformanceCounter(out long)`

C# Interop call to Win32 QueryPerformanceCount. This function should be removed if you need an interop free class definition.

- **lpPerformanceCount**: returns performance counter

**Returns:** True if successful, false otherwise

#### `Hand.QueryPerformanceFrequency(out long)`

C# Interop call to Win32 QueryPerformanceFrequency. This function should be removed if you need an interop free class definition.

- **lpFrequency**: returns performance frequence

**Returns:** True if successful, false otherwise

#### `Hand.RandomHands(ulong, ulong, int, double)`

Iterates through random hands that meets the specified requirements until the specified time duration has elapse. Please note that this iterator requires interop. If you need and interop free hand evaluator you should remove this function along with the other interop functions in this file.

- **shared**: These cards must be included in the returned hand
- **dead**: These cards must not be included in the returned hand
- **ncards**: The number of cards in the returned random hand.
- **duration**: The amount of time to allow the generation of hands to occur. When elapsed, the iterator will terminate.

**Returns:** A hand mask

#### `Hand.RandomHands(int, double)`

Iterates through random hands that meets the specified requirements until the specified time duration has elapse. Please note that this iterator requires interop. If you need and interop free hand evaluator you should remove this function along with the other interop functions in this file.

- **ncards**: The number of cards in the returned hand.
- **duration**: The amount of time to allow the generation of hands to occur. When elapsed, the iterator will terminate.

**Returns:** A hand mask.

**Fields**

#### `Hand.PreCalcPlayerOdds`

This table is used by HandPlayerOpponentOdds and contains the odds of each type of hand occuring against a random player when the board is currently empty. This calculation normally takes about 5 minutes, so the values are precalculated to save time.

#### `Hand.PreCalcOppOdds`

#### `Hand.Hearts`

Represents the suit - Hearts

#### `Hand.Diamonds`

Represents the suit - Diamonds

#### `Hand.Clubs`

Represents the suit - Clubs

#### `Hand.Spades`

Represents the suit - Spades

#### `Hand.Rank2`

Rank of a card with a value of two.

#### `Hand.Rank3`

Rank of a card with a value of three.

#### `Hand.Rank4`

Rank of a card with a value of four.

#### `Hand.Rank5`

Rank of a card with a value of five.

#### `Hand.Rank6`

Rank of a card with a value of six.

#### `Hand.Rank7`

Rank of a card with a value of seven.

#### `Hand.Rank8`

Rank of a card with a value of eight.

#### `Hand.Rank9`

Rank of a card with a value of nine.

#### `Hand.RankTen`

Rank of a card with a value of ten.

#### `Hand.RankJack`

Rank of a card showing a Jack.

#### `Hand.RankQueen`

Rank of a card showing a Queen.

#### `Hand.RankKing`

Rank of a card showing a King.

#### `Hand.RankAce`

Rank of a card showing an Ace.

#### `Hand.NumberOfCards`

The total number of cards in a deck

#### `Hand.handmask`

Hand mask for the current card set

#### `Hand.pocket`

Contains string representing the pocket cards

#### `Hand.board`

Contains a string representing the board (common cards)

#### `Hand.handval`

The value of the current had. This value allows hands to be compared using a normal arithmitic compare function.

#### `Hand.bits`

Bit count table from snippets.org

#### `Hand.CardMasksTable`

This table is equivalent to 1UL left shifted by the index. The lookup is faster than the left shift operator.

#### `Hand.TwoCardTable`

1326 ulong cards masks for all hold cards.

#### `Hand.Pocket169Table`

The 1326 possible pocket cards ordered by the 169 unique holdem combinations. The index is equivalent to the number value of Hand.PocketPairType.

### Hand.HandTypes

Possible types of hands in a texas holdem game.

*(Full name: `HoldemHand.Hand.HandTypes`)*

| Value | Description |
|-------|-------------|
| `HighCard` | Only a high card |
| `Pair` | One Pair |
| `TwoPair` | Two Pair |
| `Trips` | Three of a kind (Trips) |
| `Straight` | Straight |
| `Flush` | Flush |
| `FullHouse` | FullHouse |
| `FourOfAKind` | Four of a kind |
| `StraightFlush` | Straight Flush |

### Hand.PocketHand169Enum

An enumeration value for each of the 169 possible types of pocket cards.

*(Full name: `HoldemHand.Hand.PocketHand169Enum`)*

| Value | Description |
|-------|-------------|
| `None` | Not a PocketPairType |
| `PocketAA` | Represents a pair of Aces (Pocket Rockets) |
| `PocketKK` | Represents a pair of Kings (Cowboys) |
| `PocketQQ` | Represents a pair of Queens (Ladies) |
| `PocketJJ` | Represents a pair of Jacks (Fish hooks) |
| `PocketTT` | Represents a pair of Tens (Rin Tin Tin) |
| `Pocket99` | Represents a pair of Nines (Gretzky) |
| `Pocket88` | Represents a pair of Eights (Snowmen) |
| `Pocket77` | Represents a pair of Sevens (Hockey Sticks) |
| `Pocket66` | Represents a pair of Sixes (Route 66) |
| `Pocket55` | Represents a pair of Fives (Speed Limit) |
| `Pocket44` | Represents a pair of Fours (Sailboats) |
| `Pocket33` | Represents a pair of Threes (Crabs) |
| `Pocket22` | Represents a pair of Twos (Ducks) |
| `PocketAKs` | Represents Ace/King Suited (Big Slick) |
| `PocketAKo` | Represents Ace/King offsuit (Big Slick) |
| `PocketAQs` | Represents Ace/Queen Suited (Little Slick) |
| `PocketAQo` | Represents Ace/Queen offsuit (Little Slick) |
| `PocketAJs` | Represents Ace/Jack suited (Blackjack) |
| `PocketAJo` | Represents Ace/Jack offsuit (Blackjack) |
| `PocketATs` | Represents Ace/Ten suited (Johnny Moss) |
| `PocketATo` | Represents Ace/Ten offsuit (Johnny Moss) |
| `PocketA9s` | Represents Ace/Nine suited |
| `PocketA9o` | Represents Ace/Nine offsuit |
| `PocketA8s` | Represents Ace/Eight suited |
| `PocketA8o` | Represents Ace/Eight offsuit |
| `PocketA7s` | Represents Ace/seven suited |
| `PocketA7o` | Represents Ace/seven offsuit |
| `PocketA6s` | Represents Ace/Six suited |
| `PocketA6o` | Represents Ace/Six offsuit |
| `PocketA5s` | Represents Ace/Five suited |
| `PocketA5o` | Represents Ace/Five offsuit |
| `PocketA4s` | Represents Ace/Four suited |
| `PocketA4o` | Represents Ace/Four offsuit |
| `PocketA3s` | Represents Ace/Three suited |
| `PocketA3o` | Represents Ace/Three offsuit |
| `PocketA2s` | Represents Ace/Two suited |
| `PocketA2o` | Represents Ace/Two offsuit |
| `PocketKQs` | Represents King/Queen suited |
| `PocketKQo` | Represents King/Queen offsuit |
| `PocketKJs` | Represents King/Jack suited |
| `PocketKJo` | Represents King/Jack offsuit |
| `PocketKTs` | Represents King/Ten suited |
| `PocketKTo` | Represents King/Ten offsuit |
| `PocketK9s` | Represents King/Nine suited |
| `PocketK9o` | Represents King/Nine offsuit |
| `PocketK8s` | Represents King/Eight suited |
| `PocketK8o` | Represents King/Eight offsuit |
| `PocketK7s` | Represents King/Seven suited |
| `PocketK7o` | Represents King/Seven offsuit |
| `PocketK6s` | Represents King/Six suited |
| `PocketK6o` | Represents King/Six offsuit |
| `PocketK5s` | Represents King/Five suited |
| `PocketK5o` | Represents King/Five offsuit |
| `PocketK4s` | Represents King/Four suited |
| `PocketK4o` | Represents King/Four offsuit |
| `PocketK3s` | Represents King/Three suited |
| `PocketK3o` | Represents King/Three offsuit |
| `PocketK2s` | Represents King/Two suited |
| `PocketK2o` | Represents King/Two offsuit |
| `PocketQJs` | Represents Queen/Jack suited |
| `PocketQJo` | Represents Queen/Jack offsuit |
| `PocketQTs` | Represents Queen/Ten suited |
| `PocketQTo` | Represents Queen/Ten offsuit |
| `PocketQ9s` | Represents Queen/Nine suited |
| `PocketQ9o` | Represents Queen/Nine offsuit |
| `PocketQ8s` | Represents Queen/Eight suited |
| `PocketQ8o` | Represents Queen/Eight offsuit |
| `PocketQ7s` | Represents Queen/Seven suited |
| `PocketQ7o` | Represents Queen/Seven offsuit |
| `PocketQ6s` | Represents Queen/Six suited |
| `PocketQ6o` | Represents Queen/Six offsuit |
| `PocketQ5s` | Represents Queen/Five suited |
| `PocketQ5o` | Represents Queen/Five offsuit |
| `PocketQ4s` | Represents Queen/Four suited |
| `PocketQ4o` | Represents Queen/Four offsuit |
| `PocketQ3s` | Represents Queen/Three suited |
| `PocketQ3o` | Represents Queen/Three offsuit |
| `PocketQ2s` | Represents Queen/Two suited |
| `PocketQ2o` | Represents Queen/Two offsuit |
| `PocketJTs` | Represents Jack/Ten suited |
| `PocketJTo` | Represents Jack/Ten offsuit |
| `PocketJ9s` | Represents Jack/Nine suited |
| `PocketJ9o` | Represents Jack/Nine offsuit |
| `PocketJ8s` | Represents Jack/Eight suited |
| `PocketJ8o` | Represents Jack/Eight offsuit |
| `PocketJ7s` | Represents Jack/Seven suited |
| `PocketJ7o` | Represents Jack/Seven offsuit |
| `PocketJ6s` | Represents Jack/Six suited |
| `PocketJ6o` | Represents Jack/Six offsuit |
| `PocketJ5s` | Represents Jack/Five suited |
| `PocketJ5o` | Represents Jack/Five offsuit |
| `PocketJ4s` | Represents Jack/Four suited. |
| `PocketJ4o` | Represents Jack/Four offsuit |
| `PocketJ3s` | Represents Jack/Three suited |
| `PocketJ3o` | Represents Jack/Three offsuit |
| `PocketJ2s` | Represents Jack/Two suited. |
| `PocketJ2o` | Represents Jack/Two offsuit |
| `PocketT9s` | Represents Ten/Nine suited |
| `PocketT9o` | Represents Ten/Nine offsuit |
| `PocketT8s` | Represents Ten/Eigth suited |
| `PocketT8o` | Represents Ten/Eight offsuit |
| `PocketT7s` | Represents Ten/Seven suited |
| `PocketT7o` | Represents Ten/Seven offsuit |
| `PocketT6s` | Represents Ten/Six suited |
| `PocketT6o` | Represents Ten/Six offsuit |
| `PocketT5s` | Represents Ten/Five suited |
| `PocketT5o` | Represents Ten/Five offsuit |
| `PocketT4s` | Represents Ten/Four suited |
| `PocketT4o` | Represents Ten/Four offsuit |
| `PocketT3s` | Represents Ten/Three suited |
| `PocketT3o` | Represents Ten/Three offsuit |
| `PocketT2s` | Represents Ten/Two suited |
| `PocketT2o` | Represents Ten/Two offsuit |
| `Pocket98s` | Represents Nine/Eight suited |
| `Pocket98o` | Represents Nine/Eight offsuit |
| `Pocket97s` | Represents Nine/Seven suited |
| `Pocket97o` | Represents Nine/Seven offsuit |
| `Pocket96s` | Represents Nine/Six suited |
| `Pocket96o` | Represents Nine/Six offsuit |
| `Pocket95s` | Represents Nine/Five suited |
| `Pocket95o` | Represents Nine/Five offsuit |
| `Pocket94s` | Represents Nine/Four suited |
| `Pocket94o` | Represents Nine/Four offsuit |
| `Pocket93s` | Represents Nine/Three suited |
| `Pocket93o` | Represents Nine/Three offsuit |
| `Pocket92s` | Represents Nine/Two suited |
| `Pocket92o` | Represents Nine/Two offsuit |
| `Pocket87s` | Represents Eight/Seven Suited. |
| `Pocket87o` | Represents Eight/Seven offsuit |
| `Pocket86s` | Represents Eight/Six suited |
| `Pocket86o` | Represents Eight/Six offsuit |
| `Pocket85s` | Represents Eight/Five suited |
| `Pocket85o` | Represents Eight/Five offsuit |
| `Pocket84s` | Represents Eight/Four suited |
| `Pocket84o` | Represents Eight/Four offsuit |
| `Pocket83s` | Represents Eight/Three suited |
| `Pocket83o` | Represents Eight/Three offsuit |
| `Pocket82s` | Represents Eight/Two suited |
| `Pocket82o` | Represents Eight/Two offsuit |
| `Pocket76s` | Represents Seven/Six suited |
| `Pocket76o` | Represents Seven/Six offsuit |
| `Pocket75s` | Represents Seven/Five suited |
| `Pocket75o` | Represents Seven/Five offsuit |
| `Pocket74s` | Represents Seven/Four suited |
| `Pocket74o` | Represents Seven/Four offsuit |
| `Pocket73s` | Represents Seven/Three suited |
| `Pocket73o` | Represents Seven/Three offsuit |
| `Pocket72s` | Represents Seven/Two suited |
| `Pocket72o` | Represents Seven/Two offsuit |
| `Pocket65s` | Represents Six/Five suited |
| `Pocket65o` | Represents Six/Five offsuit |
| `Pocket64s` | Represents Six/Four suited |
| `Pocket64o` | Represents Six/Four offsuit |
| `Pocket63s` | Represents Six/Three suited |
| `Pocket63o` | Represents Six/Three offsuit |
| `Pocket62s` | Represents Six/Two suited |
| `Pocket62o` | Represents Six/Two offsuit |
| `Pocket54s` | Represents Five/Four suited |
| `Pocket54o` | Represents Five/Four offsuit |
| `Pocket53s` | Represents Five/Three suited |
| `Pocket53o` | Represents Five/Three offsuit |
| `Pocket52s` | Represents Five/Two suited |
| `Pocket52o` | Represents Five/Two offsuit |
| `Pocket43s` | Represent Four/Three suited |
| `Pocket43o` | Represents Four/Three offsuit |
| `Pocket42s` | Represents Four/Two suited |
| `Pocket42o` | Represents Four/Two offsuit |
| `Pocket32s` | Represents Three/Two suited |
| `Pocket32o` | Represents Three/Two offsuit |

### PocketHands

Represents a set of pocket hands and operations that can be applied to them.

*(Full name: `HoldemHand.PocketHands`)*

**Constructors**

#### `PocketHands()`

Default constructor

#### `PocketHands(PocketHands)`

Constructor

#### `PocketHands(ulong[])`

Constructor

#### `PocketHands(ulong)`

Constructor

#### `PocketHands(System.Collections.Generic.List{System.UInt64})`

Constructor

**Properties**

#### `PocketHands.AllHands`

Creates and instance of PocketHands with all 1326 possible pocket cards.

#### `PocketHands.Connected`

Creates an instance of PocketHands which are connected.

#### `PocketHands.Suited`

Creates an instance of PocketHands which contain all of the possible suited hands.

#### `PocketHands.Offsuit`

Creates an instance of PocketHands which contain all of the possible offsuit hands.

#### `PocketHands.Pair`

Creates an instance of PocketHands that contains all possible pocket pairs.

#### `PocketHands.Gap1`

Creates an instance of PocketHands that contains all possible pocket hands with gap of one.

#### `PocketHands.Gap2`

Creates an instance of PocketHands that contains all possible pocket hands with gap of two.

#### `PocketHands.Gap3`

Creates an instance of PocketHands that contains all possible pocket hands with gap of three.

#### `PocketHands.Gap`

Creates an instance of PocketHands that contains all possible pocket hands with gap a gap of one, two or three.

#### `PocketHands.Group1`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group1.

#### `PocketHands.Group2`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group2.

#### `PocketHands.Group3`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group3.

#### `PocketHands.Group4`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group4.

#### `PocketHands.Group5`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group5.

#### `PocketHands.Group6`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group6.

#### `PocketHands.Group7`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group7.

#### `PocketHands.Group8`

Creates an instance of PocketHands that contains all possible pocket hands that are in Skalansky group8.

#### `PocketHands.GroupNone`

Creates an instance of PocketHands that contains all possible pocket hands that are not in any of the Skalansky groups.

#### `PocketHands.Count`

Returns the number of ulong values in the pocket hand collection.

#### `PocketHands.this[int]`

Returns the value associated with a specific index of the collection.

**Methods**

#### `PocketHands.GroupType(ulong)`

Returns pocket grouping info for a given pocket cards. This is similar to but different from Sklansky groupings. It may be used just like Sklansky groupings and for most tables the this grouping and Sklansky groupings are identical.

- **mask**: pocket mask to group

**Returns:** An enum value representing the pocket grouping rank.

#### `PocketHands.PocketCard169StringToEnum(string)`

Takes as Pocket Card 169 string definition and returns the cooresponding PocketHand169Enum enum value.

- **s**: String representing the pocket card 169 type

**Returns:** The equivalent enum value in PocketHand169Enum

#### `PocketHands.WinOdds(ulong)`

Returns the probablity of the specified two card pocket hand winning against a random opponent. This is just a table lookup so the results are fairly quick.

- **mask**: 2 card pocket hand

#### `PocketHands.PocketHand169TypeCount(ulong)`

Returns the number of elements of the pocket type corresponding to this mask.

#### `PocketHands.IsConnected(ulong)`

Returns true if the 2 card pocket hand passed is connected. This function is a lookup so it is reasonably fast.

#### `PocketHands.IsSuited(ulong)`

Returns true if the two card pocket hand passed is suited. This method is a lookup so it is quite fast.

#### `PocketHands.GapCount(ulong)`

This method returns the gap count which is the distance between two pocket cards. For example, As 2h has a gap of 0 while As 3h has a gap of 1. The values returned are 0, 1, 2, 3 or -1.

#### `PocketHands.Pocket169(string)`

#### `PocketHands.FindFixCard169(string)`

#### `PocketHands.FixCard169(string)`

#### `PocketHands.BuildFix169Table()`

#### `PocketHands.PocketCards(string)`

Creates an instance of PocketHands that contains the specified pocket cards. Valid strings are "As Kd" for Ace Spaces and a King of Diamonds and so on.

#### `PocketHands.PocketCards169(string)`

Creates an instance of PocketHands that contains all of the hands specified by the Card 169 string. Valid strings are AKs (Ace/King suited), AA (A pair of aces) and so on.

#### `PocketHands.Condense169(PocketHands)`

This methods reduces the hands passed in hands to a set of that represents one of each of the 169 hand types (if such a hand exists).

#### `PocketHands.RemoveDead(ulong, PocketHands)`

The method removes all of the cards specified in the bit mask from the PocketHands passed in hands.

#### `PocketHands.Card169Max(string)`

#### `PocketHands.Card169Min(string)`

#### `PocketHands.PocketCards169Wild(string)`

Creates an instance of PocketHands given a 169 wild card string. For example AX would be and Ace and a rag. K?s would be a King and any other card suited.

#### `PocketHands.PocketCards169Range(string, string)`

Allows a range of 169 cards definitions for example Pocketcard169Range("AA", "22") would create a representation of all of the possible pairs. This method assumes that the 169 card definitions have an order. This order is AA-22, AKs, AKo, AQs, AQo and so on.

#### `PocketHands.PocketCard169Range(Hand.PocketHand169Enum, Hand.PocketHand169Enum)`

Allows a range of 169 cards definitions for example Pocketcard169Range(Hand.PocketHand169Enum.PocketAA, Hand.PocketHand169Enum.Pocket22) would create a representation of all of the possible pairs. This method assumes that the 169 card definitions have an order. This order is AA-22, AKs, AKo, AQs, AQo and so on.

#### `PocketHands.Group(PocketHands.GroupTypeEnum)`

Given a PocketGroupingRank (Sklansky group value) all of the pockethands that are in the specified group are returned.

#### `PocketHands.GroupRange(PocketHands.GroupTypeEnum, PocketHands.GroupTypeEnum)`

Given a PocketGroupRank range, all of the pockethands that are between (and including) the specfied groups are returned.

#### `PocketHands.op_BitwiseOr(PocketHands, PocketHands)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.op_BitwiseOr(PocketHands, ulong[])`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.op_BitwiseOr(ulong[], PocketHands)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.op_BitwiseOr(PocketHands, ulong)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.operator +(PocketHands, PocketHands)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.operator +(PocketHands, ulong)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.operator +(ulong, PocketHands)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.operator +(PocketHands, ulong[])`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.operator +(ulong[], PocketHands)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.operator +(PocketHands, System.Collections.Generic.List{System.UInt64})`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.operator +(System.Collections.Generic.List{System.UInt64}, PocketHands)`

This is a union operator. It combines all elements of the two collections into one combined collection.

#### `PocketHands.op_BitwiseAnd(PocketHands, PocketHands)`

This is a intersection operator. It produces a collection of the elements that are in both of the specified arguments.

#### `PocketHands.op_BitwiseAnd(PocketHands, ulong[])`

This is a intersection operator. It produces a collection of the elements that are in both of the specified arguments.

#### `PocketHands.op_BitwiseAnd(ulong[], PocketHands)`

This is a intersection operator. It produces a collection of the elements that are in both of the specified arguments.

#### `PocketHands.operator -(PocketHands, ulong)`

This operator returns a collection with all of the cards defined in the dead card mask set removed from the PocketHands collection. This can be used to remove dead cards from any PocketHands collection.

#### `PocketHands.operator -(PocketHands, System.Collections.Generic.List{System.UInt64})`

This operator returns a collection with all of the pocket hands defined in arg2 set removed from the PocketHands collection defined in arg1.

#### `PocketHands.operator -(System.Collections.Generic.List{System.UInt64}, PocketHands)`

This operator returns a collection with all of the pocket hands defined in arg2 set removed from the collection defined in arg1.

#### `PocketHands.operator -(PocketHands, PocketHands)`

This operator returns a collection with all of the pocket hands defined in arg2 set removed from the PocketHands collection defined in arg1.

#### `PocketHands.operator -(PocketHands, ulong[])`

This operator returns a collection with all of the pocket hands defined in arg2 set removed from the collection defined in arg1.

#### `PocketHands.operator -(ulong[], PocketHands)`

This operator returns a collection with all of the pocket hands defined in arg2 set removed from the collection defined in arg1.

#### `PocketHands.op_LogicalNot(PocketHands)`

This operation returns all the possible pocket hands that aren't in arg.

#### `PocketHands.operator ==(PocketHands, PocketHands)`

Compares arg1 to arg2 for equality. Order doesn't matter, but both collections must contain the same elements.

#### `PocketHands.operator !=(PocketHands, PocketHands)`

This operator compares two PocketHands collections for inequality.

#### `PocketHands.operator ==(ulong[], PocketHands)`

This oprator compares two collections of pocket hands for equality. Note: order doesn't matter.

#### `PocketHands.operator !=(ulong[], PocketHands)`

This operator compares two pocket hand collections for inequality.

#### `PocketHands.operator ==(System.Collections.Generic.List{System.UInt64}, PocketHands)`

This oprator compares two collections of pocket hands for equality. Note: order doesn't matter.

#### `PocketHands.operator !=(System.Collections.Generic.List{System.UInt64}, PocketHands)`

This operator compares two pocket hand collections for inequality.

#### `PocketHands.operator ==(PocketHands, ulong[])`

This oprator compares two collections of pocket hands for equality. Note: order doesn't matter.

#### `PocketHands.operator !=(PocketHands, ulong[])`

This operator compares two pocket hand collections for inequality.

#### `PocketHands.operator ==(PocketHands, System.Collections.Generic.List{System.UInt64})`

This oprator compares two collections of pocket hands for equality. Note: order doesn't matter.

#### `PocketHands.operator !=(PocketHands, System.Collections.Generic.List{System.UInt64})`

This operator compares two pocket hand collections for inequality.

#### `PocketHands.operator <(PocketHands, double)`

Compares each element of the PocketHands argument to see if it's win percentage is less than arg2.

- **arg1**: PocketHands collection to compare
- **arg2**: Win Value

**Returns:** A PocketHands collection where each item has win odds less than arg2

#### `PocketHands.operator <=(PocketHands, double)`

Compares each element of the PocketHands argument to see if it's win percentage is less than or equal to arg2.

- **arg1**: PocketHands collection to compare
- **arg2**: Win Value

**Returns:** A PocketHands collection where each item has win odds less than or equal to arg2

#### `PocketHands.operator >(PocketHands, double)`

Compares each element of the PocketHands argument to see if it's win percentage is greater than arg2.

- **arg1**: PocketHands collection to compare
- **arg2**: Win Value

**Returns:** A PocketHands collection where each item has win odds greater than arg2

#### `PocketHands.operator >=(PocketHands, double)`

Compares each element of the PocketHands argument to see if it's win percentage is greater than or equal to arg2.

- **arg1**: PocketHands collection to compare
- **arg2**: Win Value

**Returns:** A PocketHands collection where each item has win odds greater than or equal arg2

#### `PocketHands.operator <=(PocketHands, Hand.PocketHand169Enum)`

This operator returns all PocketHands that are in arg and are less than or equal to the 169 hand type specified in the argument type.

#### `PocketHands.operator <(PocketHands, Hand.PocketHand169Enum)`

This operator returns all PocketHands that are in arg and are less than to the 169 hand type specified in the argument type.

#### `PocketHands.operator >(PocketHands, Hand.PocketHand169Enum)`

This operator returns all PocketHands that are in arg and are greater than to the 169 hand type specified in the argument type.

#### `PocketHands.operator >=(PocketHands, Hand.PocketHand169Enum)`

This operator returns all PocketHands that are in arg and are greater than or equal to the 169 hand type specified in the argument type.

#### `PocketHands.operator <(PocketHands, PocketHands.GroupTypeEnum)`

This operator returns all PocketHands that are in arg and are less than the sklansky group type specified in the argument type.

#### `PocketHands.operator <=(PocketHands, PocketHands.GroupTypeEnum)`

This operator returns all PocketHands that are in arg and are less than or equal to the sklansky group type specified in the argument type.

#### `PocketHands.operator >(PocketHands, PocketHands.GroupTypeEnum)`

This operator returns all PocketHands that are in arg and are greter than the sklansky group type specified in the argument type.

#### `PocketHands.operator >=(PocketHands, PocketHands.GroupTypeEnum)`

This operator returns all PocketHands that are in arg and are greater than or equal to the sklansky group type specified in the argument type.

#### `PocketHands.LT(PocketHands, string)`

#### `PocketHands.LE(PocketHands, string)`

#### `PocketHands.GT(PocketHands, string)`

#### `PocketHands.GE(PocketHands, string)`

#### `PocketHands.Hands169(ulong, ulong)`

This method allows only one of each representative type of pocket hand to be iterated through. It might be preferred of over Hand.Hands() if you wish to reduce the number of pocket hands considered, but still cover all of the types of pocket hands.

- **shared**: The cards must be in the pocket hand
- **dead**: These cards must not be in the pocket hand

```csharp
using System;
using System.Collections.Generic;
using HoldemHand;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            foreach (ulong mask in PocketHands.Hands169())
            {
                count++;
            }
            // Prints out 169
            Console.WriteLine("count {0}", count);
        }
    }
}
```

#### `PocketHands.Hands169(ulong)`

This method allows only one of each representative type of pocket hand to be iterated through. It might be preferred of over Hand.Hands() if you wish to reduce the number of pocket hands considered, but still cover all of the types of pocket hands.

- **dead**: These cards must not be in the pocket hand

```csharp
using System;
using System.Collections.Generic;
using HoldemHand;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            foreach (ulong mask in PocketHands.Hands169())
            {
                count++;
            }
            // Prints out 169
            Console.WriteLine("count {0}", count);
        }
    }
}
```

#### `PocketHands.Hands169()`

This method allows only one of each representative type of pocket hand to be iterated through. It might be preferred of over Hand.Hands() if you wish to reduce the number of pocket hands considered, but still cover all of the types of pocket hands.

```csharp
using System;
using System.Collections.Generic;
using HoldemHand;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            foreach (ulong mask in PocketHands.Hands169())
            {
                count++;
            }
            // Prints out 169
            Console.WriteLine("count {0}", count);
        }
    }
}
```

#### `PocketHands.GetEnumerator()`

This method makes it possible to use the foreach statement on this class. This method has a typed return value so that box/unbox is not needed while iterating.

#### `PocketHands.IEnumerable.GetEnumerator()`

This method makes it possible to use the foreach statement on this class.

#### `PocketHands.ToArray()`

Converts a PocketHands collection into a ulong[].

#### `PocketHands.Contains(ulong)`

Returns true if the collection already contains the specified mask.

#### `PocketHands.GetHashCode()`

Calculate a hash code for the collection

#### `PocketHands.Equals(object)`

Checks this pocket hand collection with another for equality.

#### `PocketHands.implicit operator(PocketHands) -> ulong[]`

Implicit cast

**Fields**

#### `PocketHands.list`

Contains a list of the masks contains in this hole card collection.

#### `PocketHands._PocketGroupings`

Sklansky groupings

#### `PocketHands._connectedTable`

The card masked of pocket card sets that are connected such as 23o

#### `PocketHands._suitedTable`

The list of all pocket hands that are suited.

#### `PocketHands._pairTable`

A list of all pocket cards combinations that represent pairs

#### `PocketHands._gap1Table`

A list of all pocket card combintations such as 42o that have a gap of 1 cards.

#### `PocketHands._gap2Table`

A list of all pocket card combintations such as 52o that have a gap of 2 cards.

#### `PocketHands._gap3Table`

A list of all pocket card combintations such as 62o that have a gap of 3 cards.

#### `PocketHands._PocketTableMasks`

The 1326 pocket card combinations.

#### `PocketHands._PocketCards169Gap`

This table contains the gap count for each of the 169 hand types. The index cooresponds PocketHand169Enum.

#### `PocketHands._PocketCards169Connected`

This table is true for entries in the PocketCards169 table that are connected.

#### `PocketHands._PocketCards169Suited`

This table contains the suited boolean value for each of the 169 hand types. The index cooresponds PocketHand169Enum.

#### `PocketHands._Pocket169Combinations`

The 1326 possible pocket cards ordered by the 169 unique holdem combinations. The index is equivalent to the number value of Hand.PocketHand169Enum.

#### `PocketHands.PocketTableMasks`

The 1326 pocket card combinations.

#### `PocketHands.PocketCards169Strings`

A string table cooresponding to the PocketHand169Enum enumeration. The index of each string should correspond to the integer value associated with enumerator entry.

#### `PocketHands._Pocket169ProbTable`

Probablity of beating one other random player with This pocket card combination.

### PocketHands.GroupTypeEnum

Pocket Card Groupings (Group1 is best, None is worst).

*(Full name: `HoldemHand.PocketHands.GroupTypeEnum`)*

| Value | Description |
|-------|-------------|
| `Group1` | Strongest |
| `Group2` | Very Strong |
| `Group3` |  |
| `Group4` |  |
| `Group5` |  |
| `Group6` |  |
| `Group7` |  |
| `Group8` |  |
| `None` | Not in the group |

## License

This project is licensed under the **GNU Lesser General Public License v3.0** (LGPL-3.0). See [`LICENSE`](LICENSE) for the full text.

SPDX-License-Identifier: LGPL-3.0-only
