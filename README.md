# STS.General

The STS.General library contains common components shared amongst most of the applications developed by STS Soft.
The provided wide set of general methods, tools and collections have been strongly optimized for performance and are some of the fastest solutions a developer can find.

Some of the classes have been thoroughly explained in our STS Labs section: http://stssoft.com/forum/forums/21-STS-Labs


# IData

STS.General is also the "home" of the IData technology: http://stssoft.com/technologies/idata/.

IData is a summary name of a developed standalone API designed for ultra-fast binary serialization and comparison of user data. The IData technology is deeply integrated into the STSdb 4.0 engine and is one of the main reasons for the speed of STSdb.

The IData technology is 15х times faster than BinaryFormater by Microsoft, 3x times faster than Protocol Buffers by Google and 6x times faster than MessagePack by Sadayuki Furuhashi when reading and writing. The output size is 35% smaller than BinaryFormater , 20% smaller than that Protocol Buffers and 10% smaller than MessagePack.
