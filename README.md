# STS.General

The STS.General library contains common components shared amongst most of the applications developed by STS Soft.
The provided wide set of general methods, tools and collections have been strongly optimized for performance and are some of the fastest solutions a developer can find.

Some of the most avant-garde developments are:

* Fast binary serialization tools.
* Fast comparer tools (Comparer and EqualityComparer).
* Fast comparers for byte arrays.
* Methods for accessing private members in C#.
* Many ultra-fast extensions - for SortedSet<T>, List<T>, KeyValuePair<T> and etc.
* FastBinaryWriter and FastBinaryReader classes which are ~ 10-20% faster than the Microsoft's implementations.
* And many more.

Thorough explaination for some of the classes can be found in our STS Labs section: http://stssoft.com/forum/forums/21-STS-Labs.

# IData technology

STS.General is also the "home" of the IData technology: http://stssoft.com/technologies/idata/.

IData is a summary name of a developed standalone API designed for ultra-fast binary serialization and comparison of user data. The IData technology is deeply integrated into the STSdb 4.0 engine and is one of the main reasons for the speed of STSdb.

## Features

* Speed - Ultra-fast serialization and comparison of data.
* Compactness - The most space efficient serialization library.
* Simplicity - Designed for ease-of-use. The simplest and most powerful serialization library available.

## Performance

The IData technology is:

* 15х times faster than BinaryFormater by Microsoft.
* 3x times faster than Protocol Buffers by Google.
* 6x times faster than MessagePack by Sadayuki Furuhashi. 
 
The output size is:

* 35% smaller than BinaryFormater.
* 20% smaller than that Protocol Buffers.
* 10% smaller than MessagePack.

Detailed performance figures can be found here: http://stssoft.com/forum/threads/459-Persist-lt-T-gt-fast-objects-serialization.
