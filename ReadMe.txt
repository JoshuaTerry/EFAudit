Purpose:

The purpose of this logging solution is to provide logging data in a format that is more humanly readable and easier to track.  

Problem:
Common logging methods have short falls when dealing with reference properties, providing only the ID that has changed.  This can make it difficult
to walk through the change log and follow the history of modifications made for a property.  Here is an example:

Table 1 People
Columns: ID (PK int), Name (varchar(50)), FavoriteThing (FK int))

Table 2 Things
Columns: ID(PK int), Name

People Table Record 1
1, Bob, 1

Things Table Records 
1, Red things
2, Blue things

At this point we have:
Bob who likes Red things.

If we update Bob and change his favorite things to blue a typical audit log will show the following:
A DateTime, People Table, ID 1, Property FavoriteThing 1
A DateTime, People Table, ID 1, Property FavoriteThing 2

We can of course manually go look up that table and do the joins to find out that 2 is blue.
Consider however if much later Things Table is updated so that record 1 reads "Maroon" instead of Red.

If we look back now it would appear that Bob used to like "Maroon Things" instead of "Red Things".  

Yes, it is possible to cross reference the DateTime and then filter the audit table looking for changes to that property in your timeframe, but the 
difficulty increases with time and record count.

Solution:
What this solution does is attempt to clarify the changes made to a record and make them available to be easily queried.

Records are logged into 3 tables:
ChangeSets
ObjectChanges
PropertyChanges

ChangeSets
A change set represents ALL of the changes performed by a user in a single transaction.  This can represent 1 or more entities in the system and can help
to isolate and determine the scope of the change.

ObjectChanges
This table represents all the objects that may have changed within a single result set.  This helps to identify the individual entities that were acted upon.

Property Changes
Each row in this table represents a single change to the value of a property on an entity.  Each change will tie back to a single entity in the ObjectChanges table.

When all tables are joined together you will have the following information for each change:

ChangeSet Id
Timestamp for the ChangeSet
Idenfier for the User making the changes
Type Name of the object changed
The Display Name of the object changed
The ID of the object changed
The Type of Change for the Object (Added, removed, updated)
The Property Name if the object was updated
The Property Type Name if the object was updated
The Original Display Name (for reference properties this is Value for the ID)
The Original Value (for value properties it is the value for reference properties it is the Id)
The New Display Name (for reference properties this is Value for the ID)
The New Value (for value properties it is the value for reference properties it is the Id)


