Instruction for creating BookingDb on MongoDB Server (Locally):

Download and install MongoDB Community Server if don't have:
https://www.mongodb.com/try/download/community

Check this path: C:\Program Files\MongoDB\Server\8.0\bin

1) Open system properties manually:
Press Win + R, type command |sysdm.cpl| and press Enter.
This will open the "System Properties" window.

2) Select the Path variable
In the System Properties section, find a variable with the name Path (Advanced -> Environment Variables).
Click on it, then click Edit.

3) Add the path to MongoDB
In the window that opens, click Create.
Add the path to the folder bin MongoDB:
C:\Program Files\MongoDB\Server\8.0\bin

4) Press OK in each window to save the changes.
 
5)Download and install MongoDB Shell:
https://www.mongodb.com/try/download/shell

6)Recommendation for installation:
Install MongoDB Shell to:
C:\Program Files\MongoDB\mongosh

__________________________________________________________
Open CMD in Windows->
Connection to Local MongoDB:
|mongod --version|
|mongod --dbpath C:\data\db|

New CMD window(can be done in MongoDB Shell):
|mongosh --version|
|mongosh|
|show dbs|

Switch to BookingDb (it will be created if it doesn't exist):
|use BookingDb|

Create the collections:
|db.createCollection("Roles")|
|db.createCollection("Users")|
|db.createCollection("Hotels")|
|db.createCollection("Rooms")|
|db.createCollection("Bookings")|
|db.createCollection("Payments")|

Confirm the collections were created:
|show collections|

Queries for checking collections:
db.Bookings.find({}).pretty()
db.Rooms.find({}).pretty()
db.Hotels.find({}).pretty()
db.Payments.find({}).pretty()
db.Users.find({}).pretty()
db.Roles.find({}).pretty()
