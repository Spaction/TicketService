use Tickets
Go
if object_id('Shows') is null
	Create table Shows
	(
		Id int identity(1,1) Primary Key,
		Title varchar(100),
		Description varchar(255)
	)


if object_id('Locations') is null
	Create table [Locations]
	(
		Id int identity(1,1) Primary Key,
		X int,
		Y int
	)


if object_id('Seats') is null
	Create table Seats
	(
		Id int identity(1,1) Primary Key,
		ShowId int,
		LocationId int,
		[Status] int default(0),
		HoldTime datetime,
		ReservedEmail varchar(255),
		Rating decimal(5,3),
		CONSTRAINT UC_Show_Loc UNIQUE (Showid,LocationId)
	)


if exists (Select top 1 1 from Sys.foreign_keys where name = 'FK_Seat_Show')
	Alter table Seats drop Constraint FK_Seat_Show

if exists (Select top 1 1 from Sys.foreign_keys where name = 'FK_Seat_Location')
	Alter table Seats drop Constraint FK_Seat_Location 


truncate table Seats
truncate table Shows
truncate table [Locations]

if not exists (Select top 1 1 from Sys.foreign_keys where name = 'FK_Seat_Show')
	Alter table Seats Add Constraint FK_Seat_Show Foreign Key (ShowId) references Shows(Id)

if not exists (Select top 1 1 from Sys.foreign_keys where name = 'FK_Seat_Location')
	Alter table Seats Add Constraint FK_Seat_Location Foreign Key (ShowId) references [Locations](Id)


Insert into Shows(Title,Description)
Values('Show 1','This is a place holder for show 1'),
('Show 2','This is a placeholder for show 2')

Insert into [Locations](X,Y)
Values
-- 1 ,   2 ,   3 ,   4 ,   5
(1,1),(1,2),(1,3),(1,4),(1,5),
(2,1),(2,2),(2,3),(2,4),(2,5),
(3,1),(3,2),(3,3),(3,4),(3,5),
(4,1),(4,2),(4,3),(4,4),(4,5),
(5,1),(5,2),(5,3),(5,4),(5,5)

Insert into Seats(ShowID,LocationId,Rating)
Values
(1,2,10),(1,3,11),(1,4,10),
(1,6,7),(1,7,8),(1,8,9),(1,9,8),(1,10,7),
(1,12,6),(1,13,7),(1,14,6);

Insert into Seats(ShowId,LocationId,Rating)
Values
(2,1,1),(2,2,2),(2,3,3),(2,4,2),(2,5,1),
(2,6,2),(2,7,3),(2,8,4),(2,9,3),(2,10,2),
(2,11,3),(2,12,4),(2,13,5),(2,14,4),(2,15,3),
(2,16,2),(2,17,3),(2,18,4),(2,19,3),(2,20,2),
(2,21,1),(2,22,2),(2,23,3),(2,24,2),(2,25,1)
